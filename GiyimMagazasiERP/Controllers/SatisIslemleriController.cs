using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class SatisIslemleriController : Controller
{
    private readonly AppDbContext _context;

    public SatisIslemleriController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Create()
    {
        await DropdownlariDoldur();

        return View(new SatisOlusturViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SatisOlusturViewModel model)
    {
        if (!model.MusteriId.HasValue)
        {
            ModelState.AddModelError(nameof(model.MusteriId),
                "Satış işlemi için müşteri seçilmelidir.");
        }

        if (model.SepetUrunleri is null || !model.SepetUrunleri.Any())
        {
            ModelState.AddModelError("",
                "Satış tamamlamak için sepete en az bir ürün eklenmelidir.");
        }

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.OdemeTipi);
            return View(model);
        }

        var musteri = await _context.Musteriler
            .FirstOrDefaultAsync(x => x.Id == model.MusteriId);

        if (musteri is null)
        {
            ModelState.AddModelError(nameof(model.MusteriId),
                "Seçilen müşteri bulunamadı.");

            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.OdemeTipi);
            return View(model);
        }

        var personelVarMi = await _context.Personeller
            .AnyAsync(x => x.Id == model.PersonelId && x.AktifMi);

        if (!personelVarMi)
        {
            ModelState.AddModelError(nameof(model.PersonelId),
                "Seçilen personel bulunamadı veya pasif durumda.");

            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.OdemeTipi);
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

            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.OdemeTipi);
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
                ModelState.AddModelError("",
                    $"{urun.UrunAdi} için yeterli stok yok. Mevcut stok: {urun.StokMiktari}");
            }
        }

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.OdemeTipi);
            return View(model);
        }

        var finansKullanicisi = await _context.Kullanicilar
            .Where(x => x.AktifMi)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (finansKullanicisi is null)
        {
            ModelState.AddModelError("", "Finans hareketi oluşturmak için aktif kullanıcı bulunamadı.");

            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.OdemeTipi);
            return View(model);
        }

        var indirimOrani = musteri.IndirimOrani;

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
                MusteriId = musteri.Id,
                PersonelId = model.PersonelId,
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
                    Aciklama = $"Satış No: {satis.Id}"
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
                Aciklama = $"Satış No: {satis.Id} satış geliri"
            });

            musteri.ToplamHarcama += netTutar;

            var kazanilanPuan = Math.Max(1, (int)(netTutar / 100));
            musteri.SadakatPuani += kazanilanPuan;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Success), new { id = satis.Id });
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError("", "Satış kaydedilirken hata oluştu. İşlem tamamlanmadı.");

            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.OdemeTipi);
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

    private async Task DropdownlariDoldur(
        int? musteriId = null,
        int? personelId = null,
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

        ViewData["MusteriId"] = new SelectList(
            musteriler,
            "Id",
            "AdSoyad",
            musteriId);

        ViewData["MusterilerJson"] = musteriler;

        ViewData["PersonelId"] = new SelectList(
            await _context.Personeller
                .Where(x => x.AktifMi)
                .OrderBy(x => x.AdSoyad)
                .ToListAsync(),
            "Id",
            "AdSoyad",
            personelId);

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

        ViewData["UrunId"] = new SelectList(
            urunler,
            "Id",
            "Gorunum");

        ViewData["UrunlerJson"] = urunler;

        ViewData["OdemeTipleri"] = new SelectList(
            new[] { "Nakit", "KrediKarti", "BankaKarti", "Havale" },
            odemeTipi);
    }
}