using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer")]
public class SatisIslemleriController : Controller
{
    private readonly AppDbContext _context;

    private static readonly string[] SatisPersoneliPozisyonlari =
    {
        "Kasiyer",
        "Satış Danışmanı",
        "Satis Danismani",
        "Satış Temsilcisi",
        "Satis Temsilcisi"
    };

    public SatisIslemleriController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Create()
    {
        var otomatikPersonel = await OtomatikPersonelBul();

        await DropdownlariDoldur(null, otomatikPersonel?.Id, "Perakende", null);

        return View(new SatisOlusturViewModel
        {
            SatisTuru = "Perakende",
            PersonelId = otomatikPersonel?.Id
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SatisOlusturViewModel model)
    {
        model.SepetUrunleri ??= new List<SatisSepetUrunViewModel>();
        model.SatisTuru = model.SatisTuru == "Toptan" ? "Toptan" : "Perakende";

        if (model.SatisTuru == "Toptan" && !model.MusteriId.HasValue)
        {
            ModelState.AddModelError(nameof(model.MusteriId), "Toptan satış için müşteri seçilmelidir.");
        }

        if (!model.SepetUrunleri.Any())
        {
            ModelState.AddModelError("", "Satış tamamlamak için sepete en az bir ürün eklenmelidir.");
        }

        var personelId = await SatisPersonelIdBelirle(model.PersonelId);

        if (!personelId.HasValue)
        {
            ModelState.AddModelError("", "Satışı yapan uygun personel bulunamadı.");
        }

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(model.MusteriId, personelId ?? model.PersonelId, model.SatisTuru, model.OdemeTipi);
            model.PersonelId = personelId ?? model.PersonelId;
            return View(model);
        }

        Musteri? musteri = null;

        if (model.MusteriId.HasValue)
        {
            musteri = await _context.Musteriler
                .FirstOrDefaultAsync(x => x.Id == model.MusteriId.Value);

            if (musteri is null)
            {
                ModelState.AddModelError(nameof(model.MusteriId), "Seçilen müşteri bulunamadı.");
                await DropdownlariDoldur(model.MusteriId, personelId, model.SatisTuru, model.OdemeTipi);
                return View(model);
            }
        }

        var personel = await _context.Personeller
            .FirstOrDefaultAsync(x =>
                x.Id == personelId!.Value &&
                x.AktifMi &&
                SatisPersoneliPozisyonlari.Contains(x.Pozisyon));

        if (personel is null)
        {
            ModelState.AddModelError("", "Satışı yapan uygun personel bulunamadı.");
            await DropdownlariDoldur(model.MusteriId, personelId, model.SatisTuru, model.OdemeTipi);
            return View(model);
        }

        var sepetUrunIdleri = model.SepetUrunleri
            .Select(x => x.UrunId)
            .Distinct()
            .ToList();

        var urunler = await _context.Urunler
            .Where(x => sepetUrunIdleri.Contains(x.Id) && x.AktifMi)
            .ToListAsync();

        if (urunler.Count != sepetUrunIdleri.Count)
        {
            ModelState.AddModelError("", "Sepette bulunan ürünlerden biri bulunamadı veya pasif durumda.");
            await DropdownlariDoldur(model.MusteriId, personelId, model.SatisTuru, model.OdemeTipi);
            return View(model);
        }

        foreach (var sepetUrunu in model.SepetUrunleri)
        {
            var urun = urunler.First(x => x.Id == sepetUrunu.UrunId);

            if (sepetUrunu.Adet < 1)
            {
                ModelState.AddModelError("", $"{urun.UrunAdi} için adet en az 1 olmalıdır.");
            }

            if (urun.StokMiktari < sepetUrunu.Adet)
            {
                ModelState.AddModelError("", $"{urun.UrunAdi} için yeterli stok yok. Mevcut stok: {urun.StokMiktari}");
            }
        }

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(model.MusteriId, personelId, model.SatisTuru, model.OdemeTipi);
            return View(model);
        }

        var finansKullanicisi = await _context.Kullanicilar
            .Where(x => x.AktifMi)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (finansKullanicisi is null)
        {
            ModelState.AddModelError("", "Finans hareketi oluşturmak için aktif kullanıcı bulunamadı.");
            await DropdownlariDoldur(model.MusteriId, personelId, model.SatisTuru, model.OdemeTipi);
            return View(model);
        }

        var indirimOrani = musteri?.IndirimOrani ?? 0;

        decimal toplamTutar = 0;
        decimal indirimTutari = 0;

        foreach (var sepetUrunu in model.SepetUrunleri)
        {
            var urun = urunler.First(x => x.Id == sepetUrunu.UrunId);
            var satirAraToplam = urun.SatisFiyati * sepetUrunu.Adet;
            var satirIndirim = satirAraToplam * indirimOrani / 100;

            toplamTutar += satirAraToplam;
            indirimTutari += satirIndirim;
        }

        var netTutar = toplamTutar - indirimTutari;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var satis = new Satis
            {
                MusteriId = model.MusteriId,
                PersonelId = personel.Id,
                SatisTarihi = DateTime.Now,
                ToplamTutar = toplamTutar,
                IndirimTutari = indirimTutari,
                NetTutar = netTutar,
                OdemeTipi = model.OdemeTipi
            };

            _context.Satislar.Add(satis);
            await _context.SaveChangesAsync();

            foreach (var sepetUrunu in model.SepetUrunleri)
            {
                var urun = urunler.First(x => x.Id == sepetUrunu.UrunId);
                var satirToplam = urun.SatisFiyati * sepetUrunu.Adet;

                _context.SatisDetaylari.Add(new SatisDetayi
                {
                    SatisId = satis.Id,
                    UrunId = urun.Id,
                    Adet = sepetUrunu.Adet,
                    BirimFiyat = urun.SatisFiyati,
                    ToplamTutar = satirToplam
                });

                urun.StokMiktari -= sepetUrunu.Adet;

                _context.StokHareketleri.Add(new StokHareketi
                {
                    UrunId = urun.Id,
                    HareketTipi = "SatisCikis",
                    Miktar = sepetUrunu.Adet,
                    Tarih = DateTime.Now,
                    Aciklama = $"{model.SatisTuru} satış - Satış No: {satis.Id}"
                });
            }

            _context.FinansHareketleri.Add(new FinansHareketi
            {
                SatisId = satis.Id,
                KullaniciId = finansKullanicisi.Id,
                HareketTipi = "Gelir",
                Kategori = "Satis Geliri",
                Tutar = netTutar,
                Tarih = DateTime.Now,
                Aciklama = $"{model.SatisTuru} satış geliri - Satış No: {satis.Id}"
            });

            if (musteri is not null)
            {
                musteri.ToplamHarcama += netTutar;
                musteri.SadakatPuani += Math.Max(1, (int)(netTutar / 100));
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Success), new { id = satis.Id });
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError("", "Satış kaydedilirken hata oluştu. İşlem tamamlanmadı.");
            await DropdownlariDoldur(model.MusteriId, personelId, model.SatisTuru, model.OdemeTipi);
            return View(model);
        }
    }

    public async Task<IActionResult> Success(int id)
    {
        var satis = await _context.Satislar
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .Include(x => x.SatisDetaylari)
                .ThenInclude(x => x.Urun)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (satis is null)
            return NotFound();

        return View(satis);
    }

    private async Task<int?> SatisPersonelIdBelirle(int? secilenPersonelId)
    {
        if (User.IsInRole("Admin") || User.IsInRole("Yonetici"))
            return secilenPersonelId;

        var otomatikPersonel = await OtomatikPersonelBul();
        return otomatikPersonel?.Id;
    }

    private async Task<Personel?> OtomatikPersonelBul()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var adSoyad = User.Identity?.Name;

        var query = _context.Personeller
            .Where(x => x.AktifMi && SatisPersoneliPozisyonlari.Contains(x.Pozisyon));

        if (!string.IsNullOrWhiteSpace(email))
        {
            var emailEslesen = await query.FirstOrDefaultAsync(x => x.Email == email);

            if (emailEslesen is not null)
                return emailEslesen;
        }

        if (!string.IsNullOrWhiteSpace(adSoyad))
        {
            var adEslesen = await query.FirstOrDefaultAsync(x => x.AdSoyad == adSoyad);

            if (adEslesen is not null)
                return adEslesen;
        }

        return await query.OrderBy(x => x.Id).FirstOrDefaultAsync();
    }

    private async Task DropdownlariDoldur(
        int? musteriId = null,
        int? personelId = null,
        string satisTuru = "Perakende",
        string? odemeTipi = null)
    {
        var musteriler = await _context.Musteriler
            .OrderBy(x => x.AdSoyad)
            .Select(x => new
            {
                x.Id,
                x.AdSoyad,
                x.IndirimOrani
            })
            .ToListAsync();

        ViewData["MusteriId"] = new SelectList(musteriler, "Id", "AdSoyad", musteriId);
        ViewData["MusterilerJson"] = musteriler;

        var satisPersonelleri = await _context.Personeller
            .Where(x => x.AktifMi && SatisPersoneliPozisyonlari.Contains(x.Pozisyon))
            .OrderBy(x => x.AdSoyad)
            .ToListAsync();

        ViewData["PersonelId"] = new SelectList(satisPersonelleri, "Id", "AdSoyad", personelId);

        var otomatikPersonel = await OtomatikPersonelBul();
        ViewData["OtomatikPersonelAdi"] = otomatikPersonel?.AdSoyad ?? "Uygun personel bulunamadı";
        ViewData["PersonelOtomatikMi"] = User.IsInRole("Kasiyer");

        var urunler = await _context.Urunler
            .Where(x => x.AktifMi)
            .OrderBy(x => x.UrunAdi)
            .Select(x => new
            {
                x.Id,
                x.UrunAdi,
                x.Barkod,
                x.Beden,
                x.Renk,
                x.SatisFiyati,
                x.StokMiktari,
                Gorunum = x.UrunAdi
                    + " | Barkod: " + x.Barkod
                    + " | Stok: " + x.StokMiktari
                    + " | Fiyat: " + x.SatisFiyati + " TL"
            })
            .ToListAsync();

        ViewData["UrunId"] = new SelectList(urunler, "Id", "Gorunum");
        ViewData["UrunlerJson"] = urunler;

        ViewData["OdemeTipleri"] = new SelectList(
            new[] { "Nakit", "KrediKarti", "BankaKarti", "Havale" },
            odemeTipi);

        ViewData["SatisTuru"] = satisTuru;
    }
}