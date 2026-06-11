using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer,Personel")]
public class SatisIslemleriController : Controller
{
    private readonly AppDbContext _context;

    private static readonly string[] SatisPersoneliPozisyonlari =
    {
        "Kasiyer",
        "Satış Danışmanı",
        "Satis Danismani"
    };

    private static readonly string[] ToptanSatisPersoneliPozisyonlari =
    {
        "Satış Danışmanı",
        "Satis Danismani"
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

        if (model.SatisTuru == "Toptan" && !DogrudanToptanSatisYetkisiVarMi())
        {
            ModelState.AddModelError(
                nameof(model.SatisTuru),
                "Doğrudan toptan satış yalnızca Admin veya Yönetici tarafından yapılabilir. Standart toptan işlemler için Toptan Satış Talebi oluşturunuz.");
        }

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
        }
        else if (model.SatisTuru == "Toptan" && !ToptanSatisYapabilirMi(personel))
        {
            ModelState.AddModelError(
                nameof(model.PersonelId),
                "Seçilen personel toptan satış yapmaya yetkili değildir.");
        }

        if (personel is not null &&
            await AktifKasaKapanisiVarMi(personel.Id, DateTime.Today))
        {
            ModelState.AddModelError(
                "",
                "Bu kasiyer için gün sonu kasa kapanışı yapılmış. Yeni satış oluşturmak için önce kapanışın reddedilmesi veya yetkili tarafından yeniden açılması gerekir.");
        }

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(
                model.MusteriId,
                personelId,
                model.SatisTuru,
                model.OdemeTipi);

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

        var satirHesaplari = model.SepetUrunleri
    .Select(sepetUrunu =>
    {
        var urun = urunler.First(x => x.Id == sepetUrunu.UrunId);

        var satirAraToplam = Math.Round(
            urun.SatisFiyati * sepetUrunu.Adet,
            2,
            MidpointRounding.AwayFromZero);

        var satirIndirimTutari = Math.Round(
            satirAraToplam * indirimOrani / 100m,
            2,
            MidpointRounding.AwayFromZero);

        var vergiDahilTutar = Math.Round(
            satirAraToplam - satirIndirimTutari,
            2,
            MidpointRounding.AwayFromZero);

        var kdvOrani = urun.KdvOrani;
        var kdvCarpani = 1m + (kdvOrani / 100m);

        var vergiHaricTutar = kdvOrani > 0
            ? Math.Round(vergiDahilTutar / kdvCarpani, 2, MidpointRounding.AwayFromZero)
            : vergiDahilTutar;

        var kdvTutari = vergiDahilTutar - vergiHaricTutar;

        return new
        {
            Urun = urun,
            sepetUrunu.Adet,
            SatirAraToplam = satirAraToplam,
            SatirIndirimTutari = satirIndirimTutari,
            VergiDahilTutar = vergiDahilTutar,
            VergiHaricTutar = vergiHaricTutar,
            KdvOrani = kdvOrani,
            KdvTutari = kdvTutari
        };
    })
    .ToList();

        var toplamTutar = satirHesaplari.Sum(x => x.SatirAraToplam);
        var indirimTutari = satirHesaplari.Sum(x => x.SatirIndirimTutari);
        var netTutar = satirHesaplari.Sum(x => x.VergiDahilTutar);
        var toplamKdvTutari = satirHesaplari.Sum(x => x.KdvTutari);
        var vergiHaricToplam = satirHesaplari.Sum(x => x.VergiHaricTutar);
        var satisTarihi = DateTime.Now;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (await AktifKasaKapanisiVarMi(personel.Id, satisTarihi.Date))
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    "",
                    "Bu kasiyer için gün sonu kasa kapanışı yapılmış. Yeni satış oluşturmak için önce kapanışın reddedilmesi veya yetkili tarafından yeniden açılması gerekir.");

                await DropdownlariDoldur(
                    model.MusteriId,
                    personelId,
                    model.SatisTuru,
                    model.OdemeTipi);

                return View(model);
            }

            var satis = new Satis
            {
                MusteriId = model.MusteriId,
                PersonelId = personel.Id,
                SatisTarihi = satisTarihi,
                ToplamTutar = toplamTutar,
                IndirimTutari = indirimTutari,
                NetTutar = netTutar,
                ToplamKdvTutari = toplamKdvTutari,
                VergiHaricToplam = vergiHaricToplam,
                VergiDahilToplam = netTutar,
                OdemeTipi = model.OdemeTipi,
                SatisTuru = model.SatisTuru == "Toptan" ? "Toptan" : "Perakende",
                FaturaNo = "FAT-000000",
                FaturaSeri = "FAT",
                FaturaSiraNo = 0,
                FaturaTarihi = satisTarihi,
                BelgeTuru = "SatisBelgesi",
                FaturaDurumu = "Olusturuldu",
                UUID = null
            };

            _context.Satislar.Add(satis);
            await _context.SaveChangesAsync();

            satis.FaturaSiraNo = satis.Id;
            satis.FaturaNo = $"{satis.FaturaSeri}-{satis.Id:D6}";
            await _context.SaveChangesAsync();

            foreach (var hesap in satirHesaplari)
            {
                var urun = hesap.Urun;

                _context.SatisDetaylari.Add(new SatisDetayi
                {
                    SatisId = satis.Id,
                    UrunId = urun.Id,
                    Adet = hesap.Adet,
                    BirimFiyat = urun.SatisFiyati,

                    SatirIndirimTutari = hesap.SatirIndirimTutari,
                    KdvOrani = hesap.KdvOrani,
                    KdvTutari = hesap.KdvTutari,
                    VergiHaricTutar = hesap.VergiHaricTutar,
                    VergiDahilTutar = hesap.VergiDahilTutar,
                    ToplamTutar = hesap.VergiDahilTutar,

                    UrunAdiSnapshot = urun.UrunAdi,
                    BarkodSnapshot = urun.Barkod,
                    BedenSnapshot = urun.Beden,
                    RenkSnapshot = urun.Renk
                });

                urun.StokMiktari -= hesap.Adet;

                _context.StokHareketleri.Add(new StokHareketi
                {
                    UrunId = urun.Id,
                    HareketTipi = "SatisCikis",
                    Miktar = hesap.Adet,
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

        if (User.IsInRole("Kasiyer") || User.IsInRole("Personel"))
        {
            var personel = await OtomatikPersonelBul();

            if (personel is null || satis.PersonelId != personel.Id)
                return Forbid();
        }

        return View(satis);
    }

    private static bool ToptanSatisYapabilirMi(Personel personel)
    {
        return ToptanSatisPersoneliPozisyonlari.Contains(
            personel.Pozisyon,
            StringComparer.OrdinalIgnoreCase);
    }

    private bool DogrudanToptanSatisYetkisiVarMi()
    {
        return User.IsInRole("Admin") || User.IsInRole("Yonetici");
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
        var kullaniciIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(kullaniciIdText, out var kullaniciId))
        {
            var kullanici = await _context.Kullanicilar
                .AsNoTracking()
                .Include(x => x.Personel)
                .FirstOrDefaultAsync(x => x.Id == kullaniciId && x.AktifMi);

            if (kullanici?.Personel is not null &&
                kullanici.Personel.AktifMi &&
                SatisPersoneliPozisyonlari.Contains(kullanici.Personel.Pozisyon))
            {
                return kullanici.Personel;
            }

            if (!string.IsNullOrWhiteSpace(kullanici?.Email))
            {
                var emailEslesenPersonel = await _context.Personeller
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.AktifMi &&
                        x.Email == kullanici.Email &&
                        SatisPersoneliPozisyonlari.Contains(x.Pozisyon));

                if (emailEslesenPersonel is not null)
                    return emailEslesenPersonel;
            }

            if (!string.IsNullOrWhiteSpace(kullanici?.AdSoyad))
            {
                var adEslesenPersonel = await _context.Personeller
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.AktifMi &&
                        x.AdSoyad == kullanici.AdSoyad &&
                        SatisPersoneliPozisyonlari.Contains(x.Pozisyon));

                if (adEslesenPersonel is not null)
                    return adEslesenPersonel;
            }
        }

        return null;
    }

    private async Task DropdownlariDoldur(
        int? musteriId = null,
        int? personelId = null,
        string satisTuru = "Perakende",
        string? odemeTipi = null)
    {
        var musteriler = await _context.Musteriler
            .AsNoTracking()
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
            .AsNoTracking()
            .Where(x =>
                x.AktifMi &&
                SatisPersoneliPozisyonlari.Contains(x.Pozisyon))
            .OrderBy(x => x.Pozisyon == "Kasiyer" ? 0 : 1)
            .ThenBy(x => x.AdSoyad)
            .ToListAsync();

        var personelSecenekleri = satisPersonelleri
            .Select(x => new
            {
                x.Id,
                Etiket = x.AdSoyad + " - " + x.Pozisyon,
                x.Pozisyon,
                ToptanSatisYapabilir = ToptanSatisYapabilirMi(x)
            })
            .ToList();

        ViewData["PersonelId"] = new SelectList(
            personelSecenekleri,
            "Id",
            "Etiket",
            personelId);

        ViewData["SatisPersonelleriJson"] = personelSecenekleri;

        var bugun = DateTime.Today;
        var kasaKilitliPersonelIdleri = await _context.KasaKapanislari
            .AsNoTracking()
            .Where(x =>
                x.Tarih == bugun &&
                (x.Durum == KasaKapanisi.DurumHazirlandi ||
                 x.Durum == KasaKapanisi.DurumOnaylandi))
            .Select(x => x.KasaPersonelId)
            .Distinct()
            .ToListAsync();

        ViewData["KasaKilitliPersonelIdleri"] = kasaKilitliPersonelIdleri;
        ViewData["BugunKasaKapaliMi"] =
            personelId.HasValue &&
            kasaKilitliPersonelIdleri.Contains(personelId.Value);

        var otomatikPersonel = await OtomatikPersonelBul();

        ViewData["OtomatikPersonelAdi"] = otomatikPersonel is not null
            ? otomatikPersonel.AdSoyad + " - " + otomatikPersonel.Pozisyon
            : "Uygun personel bulunamadı";

        var personelOtomatikMi = !DogrudanToptanSatisYetkisiVarMi();

        ViewData["PersonelOtomatikMi"] = personelOtomatikMi;

        ViewData["PersonelEslesmeVarMi"] = !personelOtomatikMi || otomatikPersonel is not null;

        ViewData["PersonelHataMesaji"] = otomatikPersonel is null && personelOtomatikMi
            ? "Bu kullanıcıya bağlı satış personeli bulunamadı. Lütfen kullanıcı-personel eşleştirmesini kontrol edin."
            : null;

        var urunler = await _context.Urunler
            .AsNoTracking()
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
                x.KdvOrani,
                x.StokMiktari,
                Gorunum = x.UrunAdi
                    + " | Barkod: " + x.Barkod
                    + " | Stok: " + x.StokMiktari
                    + " | Fiyat: " + x.SatisFiyati + " TL"
            })
            .ToListAsync();

        ViewData["UrunId"] = new SelectList(urunler, "Id", "Gorunum");
        ViewData["UrunlerJson"] = urunler;

        var odemeTipleri = new[]
{
    new { Value = "Nakit", Text = "Nakit" },
    new { Value = "KrediKarti", Text = "Kredi Kartı" },
    new { Value = "BankaKarti", Text = "Banka Kartı" },
    new { Value = "Havale", Text = "Havale" }
};

        ViewData["OdemeTipleri"] = new SelectList(
            odemeTipleri,
            "Value",
            "Text",
            odemeTipi);

        ViewData["SatisTuru"] = satisTuru;
    }

    private Task<bool> AktifKasaKapanisiVarMi(
        int personelId,
        DateTime tarih)
    {
        var gun = tarih.Date;

        return _context.KasaKapanislari
            .AsNoTracking()
            .AnyAsync(x =>
                x.KasaPersonelId == personelId &&
                x.Tarih == gun &&
                (x.Durum == KasaKapanisi.DurumHazirlandi ||
                 x.Durum == KasaKapanisi.DurumOnaylandi));
    }
}
