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

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> YoneticiOnayla(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();

        if (kullanici is null)
            return Forbid();

        var talep = await _context.IadeDegisimTalepleri
            .FirstOrDefaultAsync(x => x.Id == id);

        if (talep is null)
            return NotFound();

        if (talep.Durum !=
            IadeDegisimTalebi.DurumYoneticiOnayiBekliyor)
        {
            TempData["Hata"] =
                "Bu talep yönetici onayı için uygun durumda değildir.";

            return RedirectToAction(nameof(Details), new { id });
        }

        var islemTarihi = DateTime.Now;
        var oncekiDurum = talep.Durum;

        talep.Durum =
            IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor;

        talep.YoneticiOnaylayanKullaniciId = kullanici.Id;
        talep.YoneticiOnayTarihi = islemTarihi;

        _context.IadeDegisimTalepHareketleri.Add(
            new IadeDegisimTalepHareketi
            {
                IadeDegisimTalebiId = talep.Id,
                KullaniciId = kullanici.Id,
                OncekiDurum = oncekiDurum,
                YeniDurum =
                    IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor,
                IslemTarihi = islemTarihi,
                Aciklama =
                    "İade talebi yönetici tarafından onaylandı."
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

        TempData["Basari"] =
            "İade talebi yönetici tarafından onaylandı. " +
            "Muhasebe onayı bekleniyor.";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,Muhasebe")]
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

        var talep = await _context.IadeDegisimTalepleri
            .FirstOrDefaultAsync(x => x.Id == id);

        if (talep is null)
            return NotFound();

        var reddedilebilirMi =
            talep.Durum ==
                IadeDegisimTalebi.DurumYoneticiOnayiBekliyor ||
            talep.Durum ==
                IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor;

        if (!reddedilebilirMi)
        {
            TempData["Hata"] =
                "Bu talep mevcut durumunda reddedilemez.";

            return RedirectToAction(nameof(Details), new { id });
        }

        var islemTarihi = DateTime.Now;
        var oncekiDurum = talep.Durum;

        talep.Durum = IadeDegisimTalebi.DurumReddedildi;
        talep.ReddedenKullaniciId = kullanici.Id;
        talep.RedTarihi = islemTarihi;
        talep.RedNedeni = redNedeni;

        _context.IadeDegisimTalepHareketleri.Add(
            new IadeDegisimTalepHareketi
            {
                IadeDegisimTalebiId = talep.Id,
                KullaniciId = kullanici.Id,
                OncekiDurum = oncekiDurum,
                YeniDurum = IadeDegisimTalebi.DurumReddedildi,
                IslemTarihi = islemTarihi,
                Aciklama =
                    $"İade talebi reddedildi. Neden: {redNedeni}"
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

        TempData["Basari"] = "İade talebi reddedildi.";

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
                IsolationLevel.Serializable);

        try
        {
            var talep = await _context.IadeDegisimTalepleri
                .Include(x => x.Satis)
                .Include(x => x.Musteri)
                .Include(x => x.Detaylar)
                    .ThenInclude(x => x.SatisDetayi)
                .Include(x => x.Detaylar)
                    .ThenInclude(x => x.Urun)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (talep is null)
            {
                await transaction.RollbackAsync();
                return NotFound();
            }

            if (talep.Durum !=
                IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Bu talep muhasebe onayı için uygun durumda değildir.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (talep.IslemTipi != IadeDegisimTalebi.IslemTipiIade)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Bu işlem yalnızca iade talepleri için kullanılabilir.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (talep.TamamlanmaTarihi.HasValue ||
                !string.IsNullOrWhiteSpace(talep.IadeBelgeNo) ||
                talep.FinansHareketiId.HasValue)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Bu iade talebi daha önce tamamlanmış.";

                return RedirectToAction(nameof(Details), new { id });
            }

            if (talep.Detaylar.Count == 0)
            {
                await transaction.RollbackAsync();

                TempData["Hata"] =
                    "Talebe bağlı iade ürünü bulunamadı.";

                return RedirectToAction(nameof(Details), new { id });
            }

            var satisDetayiIdleri = talep.Detaylar
                .Select(x => x.SatisDetayiId)
                .Distinct()
                .ToList();

            // Mevcut talep hariç tamamlanan ve diğer aktif talepler.
            var digerKullanilanAdetler =
                await _context.IadeDegisimTalepDetaylari
                    .AsNoTracking()
                    .Where(x =>
                        satisDetayiIdleri.Contains(x.SatisDetayiId) &&
                        x.IadeDegisimTalebiId != talep.Id &&
                        (
                            x.IadeDegisimTalebi.Durum ==
                                IadeDegisimTalebi.DurumTamamlandi ||
                            x.IadeDegisimTalebi.Durum ==
                                IadeDegisimTalebi
                                    .DurumYoneticiOnayiBekliyor ||
                            x.IadeDegisimTalebi.Durum ==
                                IadeDegisimTalebi
                                    .DurumMuhasebeOnayiBekliyor
                        ))
                    .GroupBy(x => x.SatisDetayiId)
                    .Select(x => new
                    {
                        SatisDetayiId = x.Key,
                        Adet = x.Sum(y => y.IadeAdedi)
                    })
                    .ToDictionaryAsync(
                        x => x.SatisDetayiId,
                        x => x.Adet);

            foreach (var detay in talep.Detaylar)
            {
                if (detay.SatisDetayi is null)
                {
                    await transaction.RollbackAsync();

                    TempData["Hata"] =
                        $"{detay.UrunAdiSnapshot} ürününün " +
                        "kaynak satış satırı bulunamadı.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                if (detay.Urun is null)
                {
                    await transaction.RollbackAsync();

                    TempData["Hata"] =
                        $"{detay.UrunAdiSnapshot} ürünü bulunamadı.";

                    return RedirectToAction(nameof(Details), new { id });
                }

                var digerKullanilanAdet =
                    digerKullanilanAdetler.GetValueOrDefault(
                        detay.SatisDetayiId);

                var kalanIadeEdilebilirAdet = Math.Max(
                    0,
                    detay.SatisDetayi.Adet - digerKullanilanAdet);

                if (detay.IadeAdedi <= 0 ||
                    detay.IadeAdedi > kalanIadeEdilebilirAdet)
                {
                    await transaction.RollbackAsync();

                    TempData["Hata"] =
                        $"{detay.UrunAdiSnapshot} için iade adedi " +
                        "artık geçerli değildir. Talep yeniden kontrol edilmelidir.";

                    return RedirectToAction(nameof(Details), new { id });
                }
            }

            var islemTarihi = DateTime.Now;

            foreach (var detay in talep.Detaylar)
            {
                var stogaGirecekMi =
                    detay.UrunDurumu ==
                        IadeDegisimTalepDetayi.UrunDurumuSatilabilir &&
                    detay.StogaGeriAlinsinMi;

                if (!stogaGirecekMi)
                    continue;

                detay.Urun.StokMiktari += detay.IadeAdedi;

                _context.StokHareketleri.Add(
                    new StokHareketi
                    {
                        UrunId = detay.UrunId,
                        HareketTipi = "IadeGiris",
                        Miktar = detay.IadeAdedi,
                        Tarih = islemTarihi,
                        Aciklama =
                            "İade işlemiyle stoğa giriş. " +
                            $"Talep No: {talep.TalepNo}"
                    });
            }

            var odemeTipi =
                talep.OdemeTipiSnapshot ??
                talep.Satis?.OdemeTipi ??
                "-";

            var finansHareketi = new FinansHareketi
            {
                SatisId = talep.SatisId,
                KullaniciId = kullanici.Id,
                HareketTipi = "Gider",
                Kategori = "Satış İadesi",
                Tutar = talep.ToplamIadeTutari,
                Tarih = islemTarihi,
                Aciklama =
                    $"Satış iadesi. Talep No: {talep.TalepNo}, " +
                    $"Satış No: #{talep.SatisId}, " +
                    $"Ödeme Tipi: {odemeTipi}"
            };

            _context.FinansHareketleri.Add(finansHareketi);

            if (talep.Musteri is not null)
            {
                talep.Musteri.ToplamHarcama = Math.Max(
                    0m,
                    talep.Musteri.ToplamHarcama -
                    talep.ToplamIadeTutari);

                var azaltilacakPuan =
                    SadakatPuaniAzaltmaMiktari(
                        talep.ToplamIadeTutari);

                talep.Musteri.SadakatPuani = Math.Max(
                    0,
                    talep.Musteri.SadakatPuani -
                    azaltilacakPuan);
            }

            var oncekiDurum = talep.Durum;

            // Finans hareketinin Id değerini almak için
            // transaction içerisinde ilk kayıt yapılır.
            await _context.SaveChangesAsync();

            talep.IadeBelgeNo = IadeBelgeNoOlustur(talep.Id);
            talep.Durum = IadeDegisimTalebi.DurumTamamlandi;
            talep.MuhasebeOnaylayanKullaniciId = kullanici.Id;
            talep.MuhasebeOnayTarihi = islemTarihi;
            talep.TamamlanmaTarihi = islemTarihi;
            talep.FinansHareketiId = finansHareketi.Id;

            _context.IadeDegisimTalepHareketleri.Add(
                new IadeDegisimTalepHareketi
                {
                    IadeDegisimTalebiId = talep.Id,
                    KullaniciId = kullanici.Id,
                    OncekiDurum = oncekiDurum,
                    YeniDurum =
                        IadeDegisimTalebi.DurumTamamlandi,
                    IslemTarihi = islemTarihi,
                    Aciklama =
                        "İade talebi muhasebe tarafından tamamlandı."
                });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Basari"] =
                "İade talebi tamamlandı. Stok ve finans kayıtları işlendi.";

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
                "İade işlemi tamamlanırken hata oluştu. " +
                "Hiçbir stok veya finans işlemi kaydedilmedi.";

            return RedirectToAction(nameof(Details), new { id });
        }
    }
    [HttpGet]
    [Authorize(Roles = "Admin,Yonetici,Muhasebe,Depo")]
    public async Task<IActionResult> IadeBelgeleri(
        string? arama,
        DateTime? baslangicTarihi,
        DateTime? bitisTarihi,
        int page = 1,
        int pageSize = 10)
    {
        var izinliSayfaBoyutlari = new[] { 10, 25, 50, 100 };

        if (page < 1)
            page = 1;

        if (!izinliSayfaBoyutlari.Contains(pageSize))
            pageSize = 10;

        arama = string.IsNullOrWhiteSpace(arama)
            ? null
            : arama.Trim();

        var query = _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Include(x => x.Satis)
            .Include(x => x.Musteri)
            .Include(x => x.TalepEdenKullanici)
            .Include(x => x.TalepEdenPersonel)
            .Include(x => x.MuhasebeOnaylayanKullanici)
            .Include(x => x.FinansHareketi)
            .Where(x =>
                x.Durum == IadeDegisimTalebi.DurumTamamlandi &&
                x.IadeBelgeNo != null &&
                x.IadeBelgeNo != "")
            .AsQueryable();

        if (arama is not null)
        {
            query = query.Where(x =>
                x.IadeBelgeNo!.Contains(arama) ||
                x.TalepNo.Contains(arama) ||
                x.SatisId.ToString().Contains(arama) ||
                x.Satis.FaturaNo.Contains(arama) ||
                (x.Musteri != null &&
                 (x.Musteri.AdSoyad.Contains(arama) ||
                  (x.Musteri.Telefon != null &&
                   x.Musteri.Telefon.Contains(arama)))) ||
                (x.TalepEdenPersonel != null &&
                 x.TalepEdenPersonel.AdSoyad.Contains(arama)) ||
                (x.TalepEdenKullanici != null &&
                 (x.TalepEdenKullanici.KullaniciAdi.Contains(arama) ||
                  (x.TalepEdenKullanici.AdSoyad != null &&
                   x.TalepEdenKullanici.AdSoyad.Contains(arama)))));
        }

        if (baslangicTarihi.HasValue)
        {
            var baslangic = baslangicTarihi.Value.Date;

            query = query.Where(x =>
                (x.TamamlanmaTarihi ?? x.TalepTarihi) >= baslangic);
        }

        if (bitisTarihi.HasValue)
        {
            var bitisExclusive = bitisTarihi.Value.Date.AddDays(1);

            query = query.Where(x =>
                (x.TamamlanmaTarihi ?? x.TalepTarihi) <
                bitisExclusive);
        }

        var totalCount = await query.CountAsync();
        var totalPages =
            (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var items = await query
            .OrderByDescending(x =>
                x.TamamlanmaTarihi ?? x.TalepTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["BaslangicTarihi"] = baslangicTarihi;
        ViewData["BitisTarihi"] = bitisTarihi;

        return View(
            new PagedResultViewModel<IadeDegisimTalebi>
            {
                Items = items,
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

        var talep = await _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Include(x => x.Satis)
            .Include(x => x.Musteri)
            .Include(x => x.TalepEdenKullanici)
            .Include(x => x.TalepEdenPersonel)
            .Include(x => x.YoneticiOnaylayanKullanici)
            .Include(x => x.MuhasebeOnaylayanKullanici)
            .Include(x => x.ReddedenKullanici)
            .Include(x => x.FinansHareketi)
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Belge(int id)
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
            .Include(x => x.YoneticiOnaylayanKullanici)
            .Include(x => x.MuhasebeOnaylayanKullanici)
            .Include(x => x.Detaylar)
            .Include(x => x.Hareketler)
                .ThenInclude(x => x.Kullanici)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (talep is null)
            return NotFound();

        if (!BelgeGorebilirMi(talep, kullanici))
            return Forbid();

        if (talep.Durum != IadeDegisimTalebi.DurumTamamlandi)
        {
            TempData["Hata"] =
                "İade belgesi yalnızca tamamlanan talepler için görüntülenebilir.";

            return RedirectToAction(nameof(Details), new { id });
        }

        if (string.IsNullOrWhiteSpace(talep.IadeBelgeNo))
        {
            TempData["Hata"] =
                "Bu talep için iade belgesi henüz oluşturulmamış.";

            return RedirectToAction(nameof(Details), new { id });
        }

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
            .ToDictionaryAsync(
                x => x.SatisDetayiId,
                x => x.Adet);

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
            .ToDictionaryAsync(
                x => x.SatisDetayiId,
                x => x.Adet);

        model.Urunler = satis.SatisDetaylari
            .OrderBy(x => x.Id)
            .Select(x =>
            {
                var tamamlanan =
                    tamamlananAdetler.GetValueOrDefault(x.Id);

                var aktif =
                    aktifAdetler.GetValueOrDefault(x.Id);

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
            .ToDictionaryAsync(
                x => x.SatisDetayiId,
                x => x.Adet);
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

    private static string IadeBelgeNoOlustur(int talepId)
    {
        return $"IADE-{talepId:D6}";
    }

    private static int SadakatPuaniAzaltmaMiktari(
        decimal iadeTutari)
    {
        if (iadeTutari <= 0)
            return 0;

        // Satış akışında her 100 TL için minimum 1 puan veriliyor.
        return Math.Max(1, (int)(iadeTutari / 100m));
    }

    private static bool BelgeGorebilirMi(
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
