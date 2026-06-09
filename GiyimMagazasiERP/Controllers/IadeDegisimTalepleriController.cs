using System.Data;
using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize]
public class IadeDegisimTalepleriController : Controller
{
    private readonly AppDbContext _context;

    private static readonly string[] IzinliDurumlar =
    {
        IadeDegisimTalebi.DurumYoneticiOnayiBekliyor,
        IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor,
        IadeDegisimTalebi.DurumReddedildi,
        IadeDegisimTalebi.DurumIptalEdildi,
        IadeDegisimTalebi.DurumTamamlandi
    };

    private static readonly string[] AktifIadeDurumlari =
    {
        IadeDegisimTalebi.DurumYoneticiOnayiBekliyor,
        IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor
    };

    private static readonly string[] IzinliUrunDurumlari =
    {
        IadeDegisimTalepDetayi.UrunDurumuSatilabilir,
        IadeDegisimTalepDetayi.UrunDurumuHasarli,
        IadeDegisimTalepDetayi.UrunDurumuIncelemeGerekli
    };

    public IadeDegisimTalepleriController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Yonetici,Muhasebe,Depo")]
    public async Task<IActionResult> Index(
        string? arama,
        string durum = "Tumu",
        string islemTipi = "Tumu",
        int page = 1,
        int pageSize = 10)
    {
        FiltreleriDogrula(ref durum, ref islemTipi, ref page, ref pageSize);

        var query = TalepListeSorgusu();
        query = TalepFiltreleriniUygula(query, arama, durum, islemTipi);

        var model = await SayfaliListeOlustur(
            query,
            arama,
            page,
            pageSize);

        ViewData["Durum"] = durum;
        ViewData["IslemTipi"] = islemTipi;

        return View(model);
    }

    [Authorize(Roles = "Admin,Yonetici,Muhasebe,Kasiyer,Personel")]
    public async Task<IActionResult> BenimTaleplerim(
        string? arama,
        string durum = "Tumu",
        string islemTipi = "Tumu",
        int page = 1,
        int pageSize = 10)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        FiltreleriDogrula(ref durum, ref islemTipi, ref page, ref pageSize);

        var query = TalepListeSorgusu()
            .Where(x =>
                x.TalepEdenKullaniciId == kullanici.Id ||
                (kullanici.PersonelId.HasValue &&
                 x.TalepEdenPersonelId == kullanici.PersonelId.Value));

        query = TalepFiltreleriniUygula(query, arama, durum, islemTipi);

        var model = await SayfaliListeOlustur(
            query,
            arama,
            page,
            pageSize);

        ViewData["Durum"] = durum;
        ViewData["IslemTipi"] = islemTipi;

        return View(model);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Yonetici,Kasiyer,Personel")]
    public async Task<IActionResult> Create(int? satisId)
    {
        if (!satisId.HasValue)
        {
            TempData["Hata"] =
                "İade talebi oluşturmak için bir satış seçilmelidir.";

            return RedirectToAction(nameof(BenimTaleplerim));
        }

        var model = new IadeDegisimTalepOlusturViewModel
        {
            SatisId = satisId,
            IslemTipi = IadeDegisimTalebi.IslemTipiIade
        };

        if (!await TalepOlusturmaModeliniDoldur(model))
            return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,Kasiyer,Personel")]
    public async Task<IActionResult> Create(
        IadeDegisimTalepOlusturViewModel model)
    {
        model.SecilenSatisDetayiIdleri ??= new();
        model.IadeAdetleri ??= new();
        model.IadeNedenleri ??= new();
        model.UrunDurumlari ??= new();
        model.StogaGeriAlinsinMi ??= new();

        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        if (!model.SatisId.HasValue)
        {
            ModelState.AddModelError(
                nameof(model.SatisId),
                "İade talebi için satış bilgisi zorunludur.");
        }

        if (model.IslemTipi != IadeDegisimTalebi.IslemTipiIade)
        {
            ModelState.AddModelError(
                nameof(model.IslemTipi),
                "Bu aşamada yalnızca iade talebi oluşturulabilir.");
        }

        if (model.Aciklama?.Trim().Length > 500)
        {
            ModelState.AddModelError(
                nameof(model.Aciklama),
                "Genel açıklama en fazla 500 karakter olabilir.");
        }

        var seciliDetayIdleri = model.SecilenSatisDetayiIdleri
            .Where(x => x > 0)
            .Distinct()
            .ToList();

        if (seciliDetayIdleri.Count == 0)
        {
            ModelState.AddModelError(
                "",
                "İade talebi için en az bir ürün seçilmelidir.");
        }

        if (!ModelState.IsValid)
        {
            if (model.SatisId.HasValue &&
                !await TalepOlusturmaModeliniDoldur(model))
            {
                return NotFound();
            }

            return View(model);
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var satis = await _context.Satislar
                .AsNoTracking()
                .Include(x => x.SatisDetaylari)
                .FirstOrDefaultAsync(x => x.Id == model.SatisId!.Value);

            if (satis is null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            var seciliDetaylar = satis.SatisDetaylari
                .Where(x => seciliDetayIdleri.Contains(x.Id))
                .ToList();

            if (seciliDetaylar.Count != seciliDetayIdleri.Count)
            {
                ModelState.AddModelError(
                    "",
                    "Seçilen ürünlerden biri bu satışa ait değildir.");
            }

            var kullanilanAdetler =
                await KullanilanIadeAdetleriniGetir(seciliDetayIdleri);

            var hesaplananSatirlar =
                new List<IadeSatirHesabi>();

            foreach (var detay in seciliDetaylar)
            {
                var kullanilanAdet = kullanilanAdetler
                    .GetValueOrDefault(detay.Id);

                var iadeEdilebilirAdet =
                    Math.Max(0, detay.Adet - kullanilanAdet);

                var iadeAdedi = model.IadeAdetleri
                    .GetValueOrDefault(detay.Id);

                var urunDurumu = model.UrunDurumlari
                    .GetValueOrDefault(
                        detay.Id,
                        IadeDegisimTalepDetayi.UrunDurumuSatilabilir);

                var iadeNedeni = model.IadeNedenleri
                    .GetValueOrDefault(detay.Id)
                    ?.Trim();

                if (iadeAdedi <= 0)
                {
                    ModelState.AddModelError(
                        "",
                        $"{detay.UrunAdiSnapshot} için iade adedi en az 1 olmalıdır.");
                }
                else if (iadeAdedi > iadeEdilebilirAdet)
                {
                    ModelState.AddModelError(
                        "",
                        $"{detay.UrunAdiSnapshot} için en fazla " +
                        $"{iadeEdilebilirAdet} adet iade talebi oluşturulabilir.");
                }

                if (!IzinliUrunDurumlari.Contains(urunDurumu))
                {
                    ModelState.AddModelError(
                        "",
                        $"{detay.UrunAdiSnapshot} için geçerli bir ürün durumu seçiniz.");
                }

                if (iadeNedeni?.Length > 300)
                {
                    ModelState.AddModelError(
                        "",
                        $"{detay.UrunAdiSnapshot} için iade nedeni " +
                        "en fazla 300 karakter olabilir.");
                }

                var stogaGeriAlinsinMi =
                    model.StogaGeriAlinsinMi.GetValueOrDefault(detay.Id);

                if (urunDurumu !=
                    IadeDegisimTalepDetayi.UrunDurumuSatilabilir)
                {
                    stogaGeriAlinsinMi = false;
                }

                if (iadeAdedi > 0 &&
                    iadeAdedi <= iadeEdilebilirAdet &&
                    IzinliUrunDurumlari.Contains(urunDurumu))
                {
                    hesaplananSatirlar.Add(
                        IadeSatirTutarlariniHesapla(
                            detay,
                            iadeAdedi,
                            urunDurumu,
                            stogaGeriAlinsinMi,
                            iadeNedeni));
                }
            }

            if (!ModelState.IsValid)
            {
                await transaction.RollbackAsync();

                if (!await TalepOlusturmaModeliniDoldur(model))
                    return NotFound();

                return View(model);
            }

            var talepTarihi = DateTime.Now;
            var talep = new IadeDegisimTalebi
            {
                TalepNo = "TMP-" + Guid.NewGuid().ToString("N")[..20],
                SatisId = satis.Id,
                MusteriId = satis.MusteriId,
                TalepEdenKullaniciId = kullanici.Id,
                TalepEdenPersonelId = kullanici.PersonelId,
                IslemTipi = IadeDegisimTalebi.IslemTipiIade,
                Durum = IadeDegisimTalebi.DurumYoneticiOnayiBekliyor,
                TalepTarihi = talepTarihi,
                Aciklama = model.Aciklama?.Trim(),
                OdemeTipiSnapshot = satis.OdemeTipi,
                ToplamIadeTutari =
                    hesaplananSatirlar.Sum(x => x.VergiDahilTutar),
                ToplamKdvTutari =
                    hesaplananSatirlar.Sum(x => x.KdvTutari),
                VergiHaricToplam =
                    hesaplananSatirlar.Sum(x => x.VergiHaricTutar),
                VergiDahilToplam =
                    hesaplananSatirlar.Sum(x => x.VergiDahilTutar)
            };

            _context.IadeDegisimTalepleri.Add(talep);
            await _context.SaveChangesAsync();

            talep.TalepNo = TalepNoOlustur(talep.Id);

            foreach (var hesap in hesaplananSatirlar)
            {
                _context.IadeDegisimTalepDetaylari.Add(
                    new IadeDegisimTalepDetayi
                    {
                        IadeDegisimTalebiId = talep.Id,
                        SatisDetayiId = hesap.SatisDetayi.Id,
                        UrunId = hesap.SatisDetayi.UrunId,
                        IadeAdedi = hesap.IadeAdedi,
                        BirimFiyat = hesap.SatisDetayi.BirimFiyat,
                        KdvOrani = hesap.SatisDetayi.KdvOrani,
                        SatirIndirimTutari = hesap.SatirIndirimTutari,
                        KdvTutari = hesap.KdvTutari,
                        VergiHaricTutar = hesap.VergiHaricTutar,
                        VergiDahilTutar = hesap.VergiDahilTutar,
                        IadeNedeni = hesap.IadeNedeni,
                        UrunDurumu = hesap.UrunDurumu,
                        StogaGeriAlinsinMi = hesap.StogaGeriAlinsinMi,
                        UrunAdiSnapshot =
                            hesap.SatisDetayi.UrunAdiSnapshot,
                        BarkodSnapshot =
                            hesap.SatisDetayi.BarkodSnapshot,
                        BedenSnapshot =
                            hesap.SatisDetayi.BedenSnapshot,
                        RenkSnapshot =
                            hesap.SatisDetayi.RenkSnapshot
                    });
            }

            _context.IadeDegisimTalepHareketleri.Add(
                new IadeDegisimTalepHareketi
                {
                    IadeDegisimTalebiId = talep.Id,
                    KullaniciId = kullanici.Id,
                    OncekiDurum = null,
                    YeniDurum =
                        IadeDegisimTalebi.DurumYoneticiOnayiBekliyor,
                    IslemTarihi = talepTarihi,
                    Aciklama = "İade talebi oluşturuldu."
                });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Basari"] =
                $"{talep.TalepNo} numaralı iade talebi oluşturuldu.";

            return RedirectToAction(nameof(Details), new { id = talep.Id });
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                "",
                "İade talebi kaydedilirken hata oluştu. Hiçbir işlem kaydedilmedi.");

            if (model.SatisId.HasValue)
                await TalepOlusturmaModeliniDoldur(model);

            return View(model);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        var talep = await _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Include(x => x.Satis)
            .Include(x => x.Musteri)
            .Include(x => x.TalepEdenKullanici)
            .Include(x => x.TalepEdenPersonel)
            .Include(x => x.Detaylar)
            .Include(x => x.Hareketler)
                .ThenInclude(x => x.Kullanici)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (talep is null)
            return NotFound();

        if (!TalepGorebilirMi(talep, kullanici))
            return Forbid();

        return View(talep);
    }

    private IQueryable<IadeDegisimTalebi> TalepListeSorgusu()
    {
        return _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Include(x => x.Satis)
            .Include(x => x.Musteri)
            .Include(x => x.TalepEdenKullanici)
            .Include(x => x.TalepEdenPersonel)
            .AsQueryable();
    }

    private static IQueryable<IadeDegisimTalebi> TalepFiltreleriniUygula(
        IQueryable<IadeDegisimTalebi> query,
        string? arama,
        string durum,
        string islemTipi)
    {
        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.TalepNo.Contains(arama) ||
                x.SatisId.ToString().Contains(arama) ||
                (x.Musteri != null &&
                 x.Musteri.AdSoyad.Contains(arama)) ||
                (x.TalepEdenPersonel != null &&
                 x.TalepEdenPersonel.AdSoyad.Contains(arama)) ||
                (x.TalepEdenKullanici != null &&
                 (x.TalepEdenKullanici.KullaniciAdi.Contains(arama) ||
                  (x.TalepEdenKullanici.AdSoyad != null &&
                   x.TalepEdenKullanici.AdSoyad.Contains(arama)))));
        }

        if (durum != "Tumu")
            query = query.Where(x => x.Durum == durum);

        if (islemTipi != "Tumu")
            query = query.Where(x => x.IslemTipi == islemTipi);

        return query;
    }

    private static void FiltreleriDogrula(
        ref string durum,
        ref string islemTipi,
        ref int page,
        ref int pageSize)
    {
        var izinliSayfaBoyutlari = new[] { 10, 25, 50, 100 };

        if (page < 1)
            page = 1;

        if (!izinliSayfaBoyutlari.Contains(pageSize))
            pageSize = 10;

        if (durum != "Tumu" && !IzinliDurumlar.Contains(durum))
            durum = "Tumu";

        if (islemTipi is not ("Tumu" or
            IadeDegisimTalebi.IslemTipiIade or
            IadeDegisimTalebi.IslemTipiDegisim))
        {
            islemTipi = "Tumu";
        }
    }

    private static async Task<PagedResultViewModel<IadeDegisimTalebi>>
        SayfaliListeOlustur(
            IQueryable<IadeDegisimTalebi> query,
            string? arama,
            int page,
            int pageSize)
    {
        var totalCount = await query.CountAsync();
        var totalPages =
            (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var items = await query
            .OrderByDescending(x => x.TalepTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultViewModel<IadeDegisimTalebi>
        {
            Items = items,
            Arama = arama,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    private async Task<bool> TalepOlusturmaModeliniDoldur(
        IadeDegisimTalepOlusturViewModel model)
    {
        if (!model.SatisId.HasValue)
            return false;

        var satis = await _context.Satislar
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .Include(x => x.SatisDetaylari)
            .FirstOrDefaultAsync(x => x.Id == model.SatisId.Value);

        if (satis is null)
            return false;

        var detayIdleri = satis.SatisDetaylari
            .Select(x => x.Id)
            .ToList();

        var tamamlananAdetler = await _context
            .IadeDegisimTalepDetaylari
            .AsNoTracking()
            .Where(x =>
                detayIdleri.Contains(x.SatisDetayiId) &&
                x.IadeDegisimTalebi.Durum ==
                    IadeDegisimTalebi.DurumTamamlandi)
            .GroupBy(x => x.SatisDetayiId)
            .Select(x => new
            {
                SatisDetayiId = x.Key,
                Adet = x.Sum(y => y.IadeAdedi)
            })
            .ToDictionaryAsync(x => x.SatisDetayiId, x => x.Adet);

        var aktifAdetler = await _context
            .IadeDegisimTalepDetaylari
            .AsNoTracking()
            .Where(x =>
                detayIdleri.Contains(x.SatisDetayiId) &&
                AktifIadeDurumlari.Contains(
                    x.IadeDegisimTalebi.Durum))
            .GroupBy(x => x.SatisDetayiId)
            .Select(x => new
            {
                SatisDetayiId = x.Key,
                Adet = x.Sum(y => y.IadeAdedi)
            })
            .ToDictionaryAsync(x => x.SatisDetayiId, x => x.Adet);

        model.Urunler = satis.SatisDetaylari
            .OrderBy(x => x.Id)
            .Select(x =>
            {
                var tamamlanan =
                    tamamlananAdetler.GetValueOrDefault(x.Id);
                var aktif = aktifAdetler.GetValueOrDefault(x.Id);

                return new IadeDegisimTalepUrunViewModel
                {
                    SatisDetayiId = x.Id,
                    UrunId = x.UrunId,
                    UrunAdi = x.UrunAdiSnapshot,
                    Barkod = x.BarkodSnapshot,
                    Beden = x.BedenSnapshot,
                    Renk = x.RenkSnapshot,
                    SatilanAdet = x.Adet,
                    DahaOnceIadeEdilenAdet = tamamlanan,
                    AktifBekleyenIadeAdedi = aktif,
                    IadeEdilebilirAdet =
                        Math.Max(0, x.Adet - tamamlanan - aktif),
                    BirimFiyat = x.BirimFiyat,
                    KdvOrani = x.KdvOrani,
                    SatirIndirimTutari = x.SatirIndirimTutari,
                    KdvTutari = x.KdvTutari,
                    VergiHaricTutar = x.VergiHaricTutar,
                    VergiDahilTutar = x.VergiDahilTutar
                };
            })
            .ToList();

        foreach (var urun in model.Urunler)
        {
            if (!model.IadeAdetleri.ContainsKey(urun.SatisDetayiId))
                model.IadeAdetleri[urun.SatisDetayiId] = 1;

            if (!model.UrunDurumlari.ContainsKey(urun.SatisDetayiId))
            {
                model.UrunDurumlari[urun.SatisDetayiId] =
                    IadeDegisimTalepDetayi.UrunDurumuSatilabilir;
            }

            if (!model.StogaGeriAlinsinMi.ContainsKey(
                urun.SatisDetayiId))
            {
                model.StogaGeriAlinsinMi[urun.SatisDetayiId] = true;
            }
        }

        ViewData["SatisTarihi"] = satis.SatisTarihi;
        ViewData["FaturaNo"] = satis.FaturaNo;
        ViewData["MusteriAdi"] =
            satis.Musteri?.AdSoyad ?? "Nihai Tüketici";
        ViewData["PersonelAdi"] = satis.Personel?.AdSoyad ?? "-";
        ViewData["OdemeTipi"] = satis.OdemeTipi;
        ViewData["SatisToplami"] = satis.NetTutar;

        return true;
    }

    private async Task<Dictionary<int, int>>
        KullanilanIadeAdetleriniGetir(List<int> satisDetayiIdleri)
    {
        return await _context.IadeDegisimTalepDetaylari
            .AsNoTracking()
            .Where(x =>
                satisDetayiIdleri.Contains(x.SatisDetayiId) &&
                (x.IadeDegisimTalebi.Durum ==
                    IadeDegisimTalebi.DurumTamamlandi ||
                 AktifIadeDurumlari.Contains(
                    x.IadeDegisimTalebi.Durum)))
            .GroupBy(x => x.SatisDetayiId)
            .Select(x => new
            {
                SatisDetayiId = x.Key,
                Adet = x.Sum(y => y.IadeAdedi)
            })
            .ToDictionaryAsync(x => x.SatisDetayiId, x => x.Adet);
    }

    private static IadeSatirHesabi IadeSatirTutarlariniHesapla(
        SatisDetayi detay,
        int iadeAdedi,
        string urunDurumu,
        bool stogaGeriAlinsinMi,
        string? iadeNedeni)
    {
        var oran = iadeAdedi / (decimal)detay.Adet;

        return new IadeSatirHesabi(
            detay,
            iadeAdedi,
            Yuvarla(detay.SatirIndirimTutari * oran),
            Yuvarla(detay.KdvTutari * oran),
            Yuvarla(detay.VergiHaricTutar * oran),
            Yuvarla(detay.VergiDahilTutar * oran),
            urunDurumu,
            stogaGeriAlinsinMi,
            string.IsNullOrWhiteSpace(iadeNedeni)
                ? null
                : iadeNedeni.Trim());
    }

    private static decimal Yuvarla(decimal tutar)
    {
        return Math.Round(
            tutar,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static string TalepNoOlustur(int talepId)
    {
        return $"IADE-TLP-{talepId:D6}";
    }

    private static bool TalepGorebilirMi(
        IadeDegisimTalebi talep,
        Kullanici kullanici)
    {
        if (kullanici.Rol is
            "Admin" or "Yonetici" or "Muhasebe" or "Depo")
        {
            return true;
        }

        return talep.TalepEdenKullaniciId == kullanici.Id ||
               (kullanici.PersonelId.HasValue &&
                talep.TalepEdenPersonelId ==
                    kullanici.PersonelId.Value);
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

    private sealed record IadeSatirHesabi(
        SatisDetayi SatisDetayi,
        int IadeAdedi,
        decimal SatirIndirimTutari,
        decimal KdvTutari,
        decimal VergiHaricTutar,
        decimal VergiDahilTutar,
        string UrunDurumu,
        bool StogaGeriAlinsinMi,
        string? IadeNedeni);
}
