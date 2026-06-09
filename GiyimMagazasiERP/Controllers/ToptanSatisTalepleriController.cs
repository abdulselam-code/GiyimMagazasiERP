using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer,Personel,Muhasebe")]
public class ToptanSatisTalepleriController : Controller
{
    private readonly AppDbContext _context;

    private static readonly string[] TalepPersoneliPozisyonlari =
    {
        "Kasiyer",
        "Satış Danışmanı",
        "Satis Danismani",
        "Mağaza Müdürü",
        "Magaza Muduru",
        "Yönetici",
        "Yonetici",
        "Admin"
    };

    private static readonly string[] OdemeTipleri =
    {
        "Nakit",
        "KrediKarti",
        "BankaKarti",
        "Havale"
    };

    public ToptanSatisTalepleriController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null || !TalepOlusturabilirMi(kullanici))
            return Forbid();

        var personelId = AdminVeyaYoneticiMi()
            ? null
            : kullanici.PersonelId;

        await DropdownlariDoldur(null, personelId, null);

        return View(new ToptanSatisTalepOlusturViewModel
        {
            TalepEdenPersonelId = personelId
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ToptanSatisTalepOlusturViewModel model)
    {
        model.Sepet ??= new();

        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null || !TalepOlusturabilirMi(kullanici))
            return Forbid();

        if (!model.MusteriId.HasValue)
        {
            ModelState.AddModelError(
                nameof(model.MusteriId),
                "Toptan satış talebi için müşteri seçilmelidir.");
        }

        if (!OdemeTipleri.Contains(model.OdemeTipi))
        {
            ModelState.AddModelError(
                nameof(model.OdemeTipi),
                "Geçerli bir ödeme tipi seçilmelidir.");
        }

        if (model.Sepet.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "Talep oluşturmak için sepete en az bir ürün eklenmelidir.");
        }

        if (model.Sepet.Any(x => x.UrunId <= 0 || x.Adet <= 0))
        {
            ModelState.AddModelError(
                "",
                "Sepette geçersiz ürün veya adet bulunmaktadır.");
        }

        var personelId = AdminVeyaYoneticiMi()
            ? model.TalepEdenPersonelId
            : kullanici.PersonelId;

        Personel? talepEdenPersonel = null;

        if (personelId.HasValue)
        {
            talepEdenPersonel = await _context.Personeller
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == personelId.Value &&
                    x.AktifMi &&
                    TalepPersoneliPozisyonlari.Contains(x.Pozisyon));
        }

        if (talepEdenPersonel is null)
        {
            ModelState.AddModelError(
                nameof(model.TalepEdenPersonelId),
                "Talep oluşturmaya uygun personel bulunamadı.");
        }

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(
                model.MusteriId,
                personelId,
                model.OdemeTipi);

            model.TalepEdenPersonelId = personelId;
            return View(model);
        }

        var musteri = await _context.Musteriler
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == model.MusteriId!.Value);

        if (musteri is null)
        {
            ModelState.AddModelError(
                nameof(model.MusteriId),
                "Seçilen müşteri bulunamadı.");

            await DropdownlariDoldur(
                model.MusteriId,
                personelId,
                model.OdemeTipi);

            return View(model);
        }

        // Aynı ürün manipülasyonla birden fazla satır gönderilirse birleştirilir.
        var sepet = model.Sepet
            .GroupBy(x => x.UrunId)
            .Select(x => new
            {
                UrunId = x.Key,
                Adet = x.Sum(y => y.Adet)
            })
            .ToList();

        var urunIdleri = sepet.Select(x => x.UrunId).ToList();

        var urunler = await _context.Urunler
            .AsNoTracking()
            .Where(x => urunIdleri.Contains(x.Id))
            .ToListAsync();

        if (urunler.Count != urunIdleri.Count)
        {
            ModelState.AddModelError(
                "",
                "Sepette bulunan ürünlerden biri bulunamadı.");

            await DropdownlariDoldur(
                model.MusteriId,
                personelId,
                model.OdemeTipi);

            return View(model);
        }

        foreach (var sepetUrunu in sepet)
        {
            var urun = urunler.First(x => x.Id == sepetUrunu.UrunId);

            if (!urun.AktifMi)
            {
                ModelState.AddModelError(
                    "",
                    $"{urun.UrunAdi} aktif durumda değildir.");
            }
            else if (urun.StokMiktari < sepetUrunu.Adet)
            {
                ModelState.AddModelError(
                    "",
                    $"{urun.UrunAdi} için yeterli stok yok. " +
                    $"Mevcut stok: {urun.StokMiktari}");
            }
        }

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(
                model.MusteriId,
                personelId,
                model.OdemeTipi);

            return View(model);
        }

        var indirimOrani = musteri.IndirimOrani;

        var satirHesaplari = sepet.Select(sepetUrunu =>
        {
            var urun = urunler.First(x => x.Id == sepetUrunu.UrunId);

            var satirAraToplam = Math.Round(
                urun.SatisFiyati * sepetUrunu.Adet,
                2,
                MidpointRounding.AwayFromZero);

            var satirIndirim = Math.Round(
                satirAraToplam * indirimOrani / 100m,
                2,
                MidpointRounding.AwayFromZero);

            var vergiDahilTutar = Math.Round(
                satirAraToplam - satirIndirim,
                2,
                MidpointRounding.AwayFromZero);

            var kdvCarpani = 1m + urun.KdvOrani / 100m;

            var vergiHaricTutar = urun.KdvOrani > 0
                ? Math.Round(
                    vergiDahilTutar / kdvCarpani,
                    2,
                    MidpointRounding.AwayFromZero)
                : vergiDahilTutar;

            var kdvTutari = vergiDahilTutar - vergiHaricTutar;

            return new
            {
                Urun = urun,
                sepetUrunu.Adet,
                SatirAraToplam = satirAraToplam,
                SatirIndirim = satirIndirim,
                VergiDahilTutar = vergiDahilTutar,
                VergiHaricTutar = vergiHaricTutar,
                KdvTutari = kdvTutari
            };
        }).ToList();

        var talepTarihi = DateTime.Now;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            // TalepNo unique olduğu için ilk kayıtta geçici, benzersiz değer kullanılır.
            var geciciTalepNo =
                "TMP-" + Guid.NewGuid().ToString("N")[..20];

            var talep = new ToptanSatisTalebi
            {
                TalepNo = geciciTalepNo,
                MusteriId = musteri.Id,
                TalepEdenPersonelId = talepEdenPersonel!.Id,
                TalepEdenKullaniciId = kullanici.Id,
                OdemeTipi = model.OdemeTipi,
                Aciklama = model.Aciklama?.Trim(),
                Durum = ToptanSatisTalebi.DurumYoneticiOnayiBekliyor,
                TalepTarihi = talepTarihi,

                ToplamTutar = satirHesaplari.Sum(x => x.SatirAraToplam),
                IndirimTutari = satirHesaplari.Sum(x => x.SatirIndirim),
                NetTutar = satirHesaplari.Sum(x => x.VergiDahilTutar),
                ToplamKdvTutari = satirHesaplari.Sum(x => x.KdvTutari),
                VergiHaricToplam =
                    satirHesaplari.Sum(x => x.VergiHaricTutar),
                VergiDahilToplam =
                    satirHesaplari.Sum(x => x.VergiDahilTutar)
            };

            _context.ToptanSatisTalepleri.Add(talep);
            await _context.SaveChangesAsync();

            talep.TalepNo = $"TST-{talep.Id:D6}";

            foreach (var hesap in satirHesaplari)
            {
                _context.ToptanSatisTalepDetaylari.Add(
                    new ToptanSatisTalepDetayi
                    {
                        ToptanSatisTalebiId = talep.Id,
                        UrunId = hesap.Urun.Id,
                        Adet = hesap.Adet,
                        BirimFiyat = hesap.Urun.SatisFiyati,
                        SatirAraToplam = hesap.SatirAraToplam,
                        SatirIndirimTutari = hesap.SatirIndirim,
                        KdvOrani = hesap.Urun.KdvOrani,
                        KdvTutari = hesap.KdvTutari,
                        VergiHaricTutar = hesap.VergiHaricTutar,
                        VergiDahilTutar = hesap.VergiDahilTutar,

                        UrunAdiSnapshot = hesap.Urun.UrunAdi,
                        BarkodSnapshot = hesap.Urun.Barkod,
                        BedenSnapshot = hesap.Urun.Beden,
                        RenkSnapshot = hesap.Urun.Renk
                    });
            }

            _context.ToptanSatisTalepHareketleri.Add(
                new ToptanSatisTalepHareketi
                {
                    ToptanSatisTalebiId = talep.Id,
                    KullaniciId = kullanici.Id,
                    OncekiDurum = null,
                    YeniDurum = talep.Durum,
                    IslemTarihi = talepTarihi,
                    Aciklama = "Toptan satış talebi oluşturuldu."
                });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Basari"] =
                $"{talep.TalepNo} numaralı toptan satış talebi oluşturuldu.";

            return RedirectToAction(nameof(Create));
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "Toptan satış talebi kaydedilirken hata oluştu.");

            await DropdownlariDoldur(
                model.MusteriId,
                personelId,
                model.OdemeTipi);

            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> YoneticiOnayla(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        var talep = await _context.ToptanSatisTalepleri
            .FirstOrDefaultAsync(x => x.Id == id);

        if (talep is null)
            return NotFound();

        if (talep.Durum != ToptanSatisTalebi.DurumYoneticiOnayiBekliyor)
        {
            TempData["Hata"] =
                "Bu talep yönetici onayı için uygun durumda değildir.";

            return RedirectToAction(nameof(Details), new { id });
        }

        var islemTarihi = DateTime.Now;
        var oncekiDurum = talep.Durum;

        talep.Durum =
            ToptanSatisTalebi.DurumMuhasebeOnayiBekliyor;

        talep.YoneticiOnaylayanKullaniciId = kullanici.Id;
        talep.YoneticiOnayTarihi = islemTarihi;

        _context.ToptanSatisTalepHareketleri.Add(
            new ToptanSatisTalepHareketi
            {
                ToptanSatisTalebiId = talep.Id,
                KullaniciId = kullanici.Id,
                OncekiDurum = oncekiDurum,
                YeniDurum =
                    ToptanSatisTalebi.DurumMuhasebeOnayiBekliyor,
                IslemTarihi = islemTarihi,
                Aciklama = "Talep yönetici tarafından onaylandı."
            });

        try
        {
            // Talep güncellemesi ve hareket kaydı tek SaveChanges ile kaydedilir.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Hata"] =
                "Talep başka bir kullanıcı tarafından güncellendi. " +
                "Lütfen sayfayı yenileyip tekrar kontrol edin.";

            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Basari"] =
            "Talep yönetici onayından geçti. Muhasebe onayı bekleniyor.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Reddet(
        int id,
        string? redNedeni)
    {
        redNedeni = redNedeni?.Trim();

        if (string.IsNullOrWhiteSpace(redNedeni))
        {
            TempData["Hata"] = "Red nedeni zorunludur.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (redNedeni.Length > 500)
        {
            TempData["Hata"] =
                "Red nedeni en fazla 500 karakter olabilir.";

            return RedirectToAction(nameof(Details), new { id });
        }

        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        var talep = await _context.ToptanSatisTalepleri
            .FirstOrDefaultAsync(x => x.Id == id);

        if (talep is null)
            return NotFound();

        if (talep.Durum != ToptanSatisTalebi.DurumYoneticiOnayiBekliyor)
        {
            TempData["Hata"] =
                "Bu talep yönetici onayı için uygun durumda değildir.";

            return RedirectToAction(nameof(Details), new { id });
        }

        var islemTarihi = DateTime.Now;
        var oncekiDurum = talep.Durum;

        talep.Durum = ToptanSatisTalebi.DurumReddedildi;
        talep.ReddedenKullaniciId = kullanici.Id;
        talep.RedTarihi = islemTarihi;
        talep.RedNedeni = redNedeni;

        _context.ToptanSatisTalepHareketleri.Add(
            new ToptanSatisTalepHareketi
            {
                ToptanSatisTalebiId = talep.Id,
                KullaniciId = kullanici.Id,
                OncekiDurum = oncekiDurum,
                YeniDurum = ToptanSatisTalebi.DurumReddedildi,
                IslemTarihi = islemTarihi,
                Aciklama = $"Talep reddedildi. Neden: {redNedeni}"
            });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Hata"] =
                "Talep başka bir kullanıcı tarafından güncellendi. " +
                "Lütfen sayfayı yenileyip tekrar kontrol edin.";

            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["Basari"] = "Talep reddedildi.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Muhasebe")]
    public async Task<IActionResult> MuhasebeOnayla(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable);

        try
        {
            var talep = await _context.ToptanSatisTalepleri
                .Include(x => x.Musteri)
                .Include(x => x.TalepEdenPersonel)
                .Include(x => x.Detaylar)
                    .ThenInclude(x => x.Urun)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (talep is null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (talep.SatisId.HasValue)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Bu talep zaten satışa dönüştürülmüş.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (talep.Durum !=
                ToptanSatisTalebi.DurumMuhasebeOnayiBekliyor)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Bu talep muhasebe onayı için uygun durumda değildir.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (talep.Musteri is null)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] = "Talebe bağlı müşteri bulunamadı.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (!talep.TalepEdenPersonelId.HasValue ||
                talep.TalepEdenPersonel is null ||
                !talep.TalepEdenPersonel.AktifMi)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Talebi oluşturan aktif satış personeli bulunamadı.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (talep.Detaylar.Count == 0)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Talepte satışa dönüştürülecek ürün bulunamadı.";

                return RedirectToAction(nameof(Details), new { id });
            }

            foreach (var urunGrubu in talep.Detaylar.GroupBy(x => x.UrunId))
            {
                var ilkDetay = urunGrubu.First();
                var urun = ilkDetay.Urun;
                var toplamAdet = urunGrubu.Sum(x => x.Adet);

                if (urun is null)
                {
                    await transaction.RollbackAsync();

                    TempData["Hata"] =
                        $"{ilkDetay.UrunAdiSnapshot} ürünü bulunamadı.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                if (!urun.AktifMi)
                {
                    await transaction.RollbackAsync();

                    TempData["Hata"] =
                        $"{ilkDetay.UrunAdiSnapshot} aktif durumda değildir.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                if (urun.StokMiktari < toplamAdet)
                {
                    await transaction.RollbackAsync();

                    TempData["Hata"] =
                        $"{ilkDetay.UrunAdiSnapshot} için yeterli stok yok. " +
                        $"Mevcut stok: {urun.StokMiktari}";

                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            var islemTarihi = DateTime.Now;

            var satis = new Satis
            {
                MusteriId = talep.MusteriId,
                PersonelId = talep.TalepEdenPersonelId.Value,
                SatisTarihi = islemTarihi,

                ToplamTutar = talep.ToplamTutar,
                IndirimTutari = talep.IndirimTutari,
                NetTutar = talep.NetTutar,
                ToplamKdvTutari = talep.ToplamKdvTutari,
                VergiHaricToplam = talep.VergiHaricToplam,
                VergiDahilToplam = talep.VergiDahilToplam,

                OdemeTipi = talep.OdemeTipi,
                SatisTuru = "Toptan",

                FaturaNo = "FAT-000000",
                FaturaSeri = "FAT",
                FaturaSiraNo = 0,
                FaturaTarihi = islemTarihi,
                BelgeTuru = "SatisBelgesi",
                FaturaDurumu = "Olusturuldu",
                UUID = null
            };

            _context.Satislar.Add(satis);
            await _context.SaveChangesAsync();

            satis.FaturaSiraNo = satis.Id;
            satis.FaturaNo = $"{satis.FaturaSeri}-{satis.Id:D6}";

            foreach (var detay in talep.Detaylar)
            {
                _context.SatisDetaylari.Add(new SatisDetayi
                {
                    SatisId = satis.Id,
                    UrunId = detay.UrunId,
                    Adet = detay.Adet,
                    BirimFiyat = detay.BirimFiyat,

                    ToplamTutar = detay.VergiDahilTutar,
                    SatirIndirimTutari = detay.SatirIndirimTutari,
                    KdvOrani = detay.KdvOrani,
                    KdvTutari = detay.KdvTutari,
                    VergiHaricTutar = detay.VergiHaricTutar,
                    VergiDahilTutar = detay.VergiDahilTutar,

                    UrunAdiSnapshot = detay.UrunAdiSnapshot,
                    BarkodSnapshot = detay.BarkodSnapshot,
                    BedenSnapshot = detay.BedenSnapshot,
                    RenkSnapshot = detay.RenkSnapshot
                });

                detay.Urun.StokMiktari -= detay.Adet;

                _context.StokHareketleri.Add(new StokHareketi
                {
                    UrunId = detay.UrunId,
                    HareketTipi = "SatisCikis",
                    Miktar = detay.Adet,
                    Tarih = islemTarihi,
                    Aciklama =
                        "Toptan satış talebinden satışa dönüştürüldü. " +
                        $"Talep No: {talep.TalepNo}"
                });
            }

            _context.FinansHareketleri.Add(new FinansHareketi
            {
                SatisId = satis.Id,
                KullaniciId = kullanici.Id,
                HareketTipi = "Gelir",
                Kategori = "Satis Geliri",
                Tutar = talep.NetTutar,
                Tarih = islemTarihi,
                Aciklama =
                    "Toptan satış talebinden oluşan satış. " +
                    $"Talep No: {talep.TalepNo}"
            });

            talep.Musteri.ToplamHarcama += talep.NetTutar;
            talep.Musteri.SadakatPuani +=
                Math.Max(1, (int)(talep.NetTutar / 100m));

            var oncekiDurum = talep.Durum;

            talep.SatisId = satis.Id;
            talep.SatisaDonusturulmeTarihi = islemTarihi;
            talep.MuhasebeOnaylayanKullaniciId = kullanici.Id;
            talep.MuhasebeOnayTarihi = islemTarihi;
            talep.Durum =
                ToptanSatisTalebi.DurumSatisaDonusturuldu;

            _context.ToptanSatisTalepHareketleri.Add(
                new ToptanSatisTalepHareketi
                {
                    ToptanSatisTalebiId = talep.Id,
                    KullaniciId = kullanici.Id,
                    OncekiDurum = oncekiDurum,
                    YeniDurum =
                        ToptanSatisTalebi.DurumSatisaDonusturuldu,
                    IslemTarihi = islemTarihi,
                    Aciklama =
                        "Talep muhasebe tarafından onaylandı ve " +
                        "satışa dönüştürüldü."
                });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Basari"] =
                "Talep muhasebe tarafından onaylandı ve " +
                "satışa dönüştürüldü.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();

            TempData["Hata"] =
                "Talep başka bir kullanıcı tarafından güncellendi. " +
                "Lütfen tekrar kontrol edin.";

            return RedirectToAction(nameof(Details), new { id });
        }
        catch
        {
            await transaction.RollbackAsync();

            TempData["Hata"] =
                "Talep satışa dönüştürülürken hata oluştu. " +
                "Hiçbir işlem kaydedilmedi.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }

    private static bool TalepGorebilirMi(
    ToptanSatisTalebi talep,
    Kullanici kullanici)
    {
        if (kullanici.Rol is "Admin" or "Yonetici" or "Muhasebe")
            return true;

        if (talep.TalepEdenKullaniciId == kullanici.Id)
            return true;

        return kullanici.PersonelId.HasValue &&
               talep.TalepEdenPersonelId == kullanici.PersonelId.Value;
    }
    private bool AdminVeyaYoneticiMi()
    {
        return User.IsInRole("Admin") || User.IsInRole("Yonetici");
    }

    private static bool TalepOlusturabilirMi(Kullanici kullanici)
    {
        if (kullanici.Rol is "Admin" or "Yonetici")
            return true;

        return kullanici.Personel is not null &&
               kullanici.Personel.AktifMi &&
               TalepPersoneliPozisyonlari.Contains(
                   kullanici.Personel.Pozisyon);
    }

    private async Task<Kullanici?> GirisYapanKullaniciyiGetir()
    {
        var kullaniciIdMetni =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(kullaniciIdMetni, out var kullaniciId))
            return null;

        return await _context.Kullanicilar
            .AsNoTracking()
            .Include(x => x.Personel)
            .FirstOrDefaultAsync(x =>
                x.Id == kullaniciId &&
                x.AktifMi);
    }

    private async Task DropdownlariDoldur(
        int? musteriId,
        int? personelId,
        string? odemeTipi)
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

        ViewData["MusteriId"] = new SelectList(
            musteriler,
            "Id",
            "AdSoyad",
            musteriId);

        ViewData["MusterilerJson"] = musteriler;

        var personeller = await _context.Personeller
            .AsNoTracking()
            .Where(x =>
                x.AktifMi &&
                TalepPersoneliPozisyonlari.Contains(x.Pozisyon))
            .OrderBy(x => x.AdSoyad)
            .Select(x => new
            {
                x.Id,
                Etiket = x.AdSoyad + " - " + x.Pozisyon
            })
            .ToListAsync();

        ViewData["TalepEdenPersonelId"] = new SelectList(
            personeller,
            "Id",
            "Etiket",
            personelId);

        ViewData["PersonelOtomatikMi"] = !AdminVeyaYoneticiMi();

        ViewData["OtomatikPersonelAdi"] =
            personelId.HasValue
                ? personeller
                    .FirstOrDefault(x => x.Id == personelId.Value)
                    ?.Etiket
                : null;

        var urunler = await _context.Urunler
            .AsNoTracking()
            .Where(x => x.AktifMi && x.StokMiktari > 0)
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
            })
            .ToListAsync();

        ViewData["UrunId"] = new SelectList(
            urunler,
            "Id",
            "Gorunum");

        ViewData["UrunlerJson"] = urunler;

        var odemeSecenekleri = new[]
        {
            new { Value = "Nakit", Text = "Nakit" },
            new { Value = "KrediKarti", Text = "Kredi Kartı" },
            new { Value = "BankaKarti", Text = "Banka Kartı" },
            new { Value = "Havale", Text = "Havale" }
        };

        ViewData["OdemeTipleri"] = new SelectList(
            odemeSecenekleri,
            "Value",
            "Text",
            odemeTipi);
    }
    [Authorize(Roles = "Admin,Yonetici,Muhasebe")]
    public async Task<IActionResult> Index(
    string? arama,
    string durum = "Tumu",
    DateTime? baslangicTarihi = null,
    DateTime? bitisTarihi = null,
    int page = 1,
    int pageSize = 10)
    {
        var izinliSayfaBoyutlari = new[] { 10, 25, 50, 100 };
        var izinliDurumlar = new[]
        {
        "Tumu",
        ToptanSatisTalebi.DurumYoneticiOnayiBekliyor,
        ToptanSatisTalebi.DurumMuhasebeOnayiBekliyor,
        ToptanSatisTalebi.DurumReddedildi,
        ToptanSatisTalebi.DurumSatisaDonusturuldu
    };

        if (page < 1)
            page = 1;

        if (!izinliSayfaBoyutlari.Contains(pageSize))
            pageSize = 10;

        if (!izinliDurumlar.Contains(durum))
            durum = "Tumu";

        var query = _context.ToptanSatisTalepleri
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.TalepEdenPersonel)
            .Include(x => x.TalepEdenKullanici)
            .Include(x => x.Satis)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.TalepNo.Contains(arama) ||
                x.Musteri.AdSoyad.Contains(arama) ||
                (x.TalepEdenPersonel != null &&
                 x.TalepEdenPersonel.AdSoyad.Contains(arama)) ||
                x.OdemeTipi.Contains(arama));
        }

        if (durum != "Tumu")
            query = query.Where(x => x.Durum == durum);

        if (baslangicTarihi.HasValue)
        {
            var baslangic = baslangicTarihi.Value.Date;
            query = query.Where(x => x.TalepTarihi >= baslangic);
        }

        if (bitisTarihi.HasValue)
        {
            var bitisExclusive = bitisTarihi.Value.Date.AddDays(1);
            query = query.Where(x => x.TalepTarihi < bitisExclusive);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var talepler = await query
            .OrderByDescending(x => x.TalepTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["Durum"] = durum;
        ViewData["BaslangicTarihi"] =
            baslangicTarihi?.ToString("yyyy-MM-dd");
        ViewData["BitisTarihi"] =
            bitisTarihi?.ToString("yyyy-MM-dd");

        return View(new PagedResultViewModel<ToptanSatisTalebi>
        {
            Items = talepler,
            Arama = arama,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        });
    }

    [Authorize(Roles = "Admin,Yonetici,Kasiyer,Personel")]
    public async Task<IActionResult> BenimTaleplerim(
        string? arama,
        string durum = "Tumu",
        int page = 1,
        int pageSize = 10)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        var izinliSayfaBoyutlari = new[] { 10, 25, 50, 100 };

        if (page < 1)
            page = 1;

        if (!izinliSayfaBoyutlari.Contains(pageSize))
            pageSize = 10;

        var query = _context.ToptanSatisTalepleri
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.TalepEdenPersonel)
            .Include(x => x.Satis)
            .Where(x =>
                x.TalepEdenKullaniciId == kullanici.Id ||
                (kullanici.PersonelId.HasValue &&
                 x.TalepEdenPersonelId == kullanici.PersonelId.Value));

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.TalepNo.Contains(arama) ||
                x.Musteri.AdSoyad.Contains(arama) ||
                x.OdemeTipi.Contains(arama));
        }

        if (!string.IsNullOrWhiteSpace(durum) && durum != "Tumu")
            query = query.Where(x => x.Durum == durum);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var talepler = await query
            .OrderByDescending(x => x.TalepTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["Durum"] = durum;

        return View(new PagedResultViewModel<ToptanSatisTalebi>
        {
            Items = talepler,
            Arama = arama,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        var talep = await _context.ToptanSatisTalepleri
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.TalepEdenPersonel)
            .Include(x => x.TalepEdenKullanici)
            .Include(x => x.YoneticiOnaylayanKullanici)
            .Include(x => x.MuhasebeOnaylayanKullanici)
            .Include(x => x.ReddedenKullanici)
            .Include(x => x.Satis)
            .Include(x => x.Detaylar)
                .ThenInclude(x => x.Urun)
            .Include(x => x.Hareketler)
                .ThenInclude(x => x.Kullanici)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (talep is null)
            return NotFound();

        if (!TalepGorebilirMi(talep, kullanici))
            return Forbid();

        return View(talep);
    }
}