using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Muhasebe,Kasiyer")]
public class KasaKapanislariController : Controller
{
    private static readonly string[] GecerliDurumlar =
    {
        KasaKapanisi.DurumHazirlandi,
        KasaKapanisi.DurumOnaylandi,
        KasaKapanisi.DurumReddedildi
    };

    private readonly AppDbContext _context;

    public KasaKapanislariController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? arama,
        string durum = "Tumu",
        int? kasaPersonelId = null,
        DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null,
        int page = 1,
        int pageSize = 10)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        if (User.IsInRole("Kasiyer"))
        {
            if (!kullanici.PersonelId.HasValue)
                return Forbid();

            return RedirectToAction(nameof(BenimKapanislarim), new
            {
                arama,
                durum,
                baslangicTarihi,
                bitisTarihi,
                page,
                pageSize
            });
        }

        var query = KapanisSorgusu();
        query = FiltreleriUygula(
            query,
            arama,
            durum,
            kasaPersonelId,
            baslangicTarihi,
            bitisTarihi);

        var model = await Sayfala(
            query,
            arama,
            durum,
            kasaPersonelId,
            baslangicTarihi,
            bitisTarihi,
            page,
            pageSize);

        model.Kasiyerler = await KasiyerSecenekleriniGetir();
        await BugunOzetiniDoldur(model, null);
        return View(model);
    }

    public async Task<IActionResult> BenimKapanislarim(
        string? arama,
        string durum = "Tumu",
        DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null,
        int page = 1,
        int pageSize = 10)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var query = KapanisSorgusu()
            .Where(x => x.KasaKullaniciId == kullanici.Id);

        query = FiltreleriUygula(
            query,
            arama,
            durum,
            null,
            baslangicTarihi,
            bitisTarihi);

        var model = await Sayfala(
            query,
            arama,
            durum,
            null,
            baslangicTarihi,
            bitisTarihi,
            page,
            pageSize);

        await BugunOzetiniDoldur(model, kullanici.Id);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(
        DateTime? tarih = null,
        int? kasaPersonelId = null)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var seciliTarih = (tarih ?? DateTime.Today).Date;
        int? seciliPersonelId;

        if (User.IsInRole("Kasiyer"))
        {
            if (!kullanici.PersonelId.HasValue)
            {
                TempData["Hata"] =
                    "Bu kullanıcıya bağlı personel kaydı bulunmadığı için kasa kapanışı oluşturulamaz.";
                return RedirectToAction(nameof(BenimKapanislarim));
            }

            seciliPersonelId = kullanici.PersonelId;
        }
        else
        {
            seciliPersonelId = kasaPersonelId;
        }

        var model = new KasaKapanisiOlusturViewModel
        {
            Tarih = seciliTarih,
            KasaPersonelId = seciliPersonelId
        };

        if (seciliPersonelId.HasValue)
        {
            var ozet = await BeklenenKasayiHesapla(
                seciliPersonelId.Value,
                seciliTarih);
            OzetiModeleAktar(model, ozet);
        }

        await CreateVerileriniDoldur(seciliPersonelId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(KasaKapanisiOlusturViewModel model)
    {
        SayilanTutarHatalariniTurkcelestir();

        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var personelId = User.IsInRole("Kasiyer")
            ? kullanici.PersonelId
            : model.KasaPersonelId;
        model.KasaPersonelId = personelId;
        model.Tarih = model.Tarih.Date;

        if (!personelId.HasValue)
        {
            ModelState.AddModelError(
                nameof(model.KasaPersonelId),
                "Kasa personeli seçilmelidir.");
        }

        Personel? kasaPersoneli = null;
        Kullanici? kasaKullanicisi = null;

        if (personelId.HasValue)
        {
            kasaPersoneli = await _context.Personeller
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == personelId.Value &&
                    x.AktifMi &&
                    x.Pozisyon == "Kasiyer");

            if (kasaPersoneli is null)
            {
                ModelState.AddModelError(
                    nameof(model.KasaPersonelId),
                    "Seçilen personel aktif bir kasiyer değildir.");
            }

            kasaKullanicisi = await _context.Kullanicilar
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.PersonelId == personelId.Value &&
                    x.AktifMi &&
                    x.Rol == "Kasiyer");

            if (kasaKullanicisi is null)
            {
                ModelState.AddModelError(
                    nameof(model.KasaPersonelId),
                    "Seçilen kasiyere bağlı aktif kullanıcı hesabı bulunamadı.");
            }
        }

        if (User.IsInRole("Kasiyer") &&
            kullanici.PersonelId != personelId)
        {
            return Forbid();
        }

        if (personelId.HasValue &&
            await _context.KasaKapanislari
                .AsNoTracking()
                .AnyAsync(x =>
                    x.KasaPersonelId == personelId.Value &&
                    x.Tarih == model.Tarih))
        {
            ModelState.AddModelError(
                "",
                "Bu kasiyer ve tarih için daha önce kasa kapanışı oluşturulmuş.");
        }

        var ozet = personelId.HasValue
            ? await BeklenenKasayiHesapla(personelId.Value, model.Tarih)
            : new KasaKapanisiOzetViewModel();
        OzetiModeleAktar(model, ozet);

        if (!ModelState.IsValid)
        {
            await CreateVerileriniDoldur(personelId);
            return View(model);
        }

        var sayilanToplam =
            model.SayilanNakit +
            model.SayilanKrediKarti +
            model.SayilanHavale;

        var kapanis = new KasaKapanisi
        {
            KapanisNo = "TMP-" + Guid.NewGuid().ToString("N")[..20],
            KasaPersonelId = kasaPersoneli!.Id,
            KasaKullaniciId = kasaKullanicisi!.Id,
            Tarih = model.Tarih,
            BeklenenNakit = ozet.BeklenenNakit,
            BeklenenKrediKarti = ozet.BeklenenKrediKarti,
            BeklenenHavale = ozet.BeklenenHavale,
            BeklenenToplam = ozet.BeklenenToplam,
            SayilanNakit = model.SayilanNakit,
            SayilanKrediKarti = model.SayilanKrediKarti,
            SayilanHavale = model.SayilanHavale,
            SayilanToplam = sayilanToplam,
            FarkNakit = model.SayilanNakit - ozet.BeklenenNakit,
            FarkKrediKarti =
                model.SayilanKrediKarti - ozet.BeklenenKrediKarti,
            FarkHavale = model.SayilanHavale - ozet.BeklenenHavale,
            FarkToplam = sayilanToplam - ozet.BeklenenToplam,
            SatisSayisi = ozet.SatisSayisi,
            IadeSayisi = ozet.IadeSayisi,
            IadeToplami = ozet.IadeToplami,
            Durum = KasaKapanisi.DurumHazirlandi,
            Aciklama = AciklamayiHazirla(
                model.Aciklama,
                ozet.DagitilamayanIadeToplami),
            OlusturmaTarihi = DateTime.Now
        };

        _context.KasaKapanislari.Add(kapanis);

        try
        {
            await _context.SaveChangesAsync();
            kapanis.KapanisNo = $"KASA-KPN-{kapanis.Id:D6}";
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            TempData["Hata"] =
                "Kasa kapanışı kaydedilemedi. Aynı kasiyer ve tarih için mevcut bir kayıt olabilir.";
            return RedirectToAction(nameof(Create), new
            {
                tarih = model.Tarih.ToString("yyyy-MM-dd"),
                kasaPersonelId = personelId
            });
        }

        TempData["Basari"] =
            $"{kapanis.KapanisNo} numaralı kasa kapanışı oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = kapanis.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var kapanis = await KapanisSorgusu()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kapanis is null)
            return NotFound();

        if (User.IsInRole("Kasiyer") &&
            kapanis.KasaKullaniciId != kullanici.Id)
        {
            return Forbid();
        }

        return View(kapanis);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,Muhasebe")]
    public async Task<IActionResult> Onayla(int id, string rowVersion)
    {
        var kapanis = await _context.KasaKapanislari
            .FirstOrDefaultAsync(x => x.Id == id);
        if (kapanis is null)
            return NotFound();

        if (kapanis.Durum != KasaKapanisi.DurumHazirlandi)
        {
            TempData["Hata"] =
                "Yalnızca hazırlanmış kasa kapanışları onaylanabilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!RowVersionAyarla(kapanis, rowVersion))
        {
            TempData["Hata"] = "Kasa kapanışı sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var guncelOzet = await BeklenenKasayiHesapla(
            kapanis.KasaPersonelId,
            kapanis.Tarih);

        if (KapanisOzetiDegismisMi(kapanis, guncelOzet))
        {
            TempData["Hata"] =
                "Kasa kapanışı oluşturulduktan sonra satış/iade hareketi değişmiş. Lütfen kapanışı yeniden hazırlayın.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var kullaniciId = GirisYapanKullaniciId();
        if (!kullaniciId.HasValue)
            return Forbid();

        kapanis.Durum = KasaKapanisi.DurumOnaylandi;
        kapanis.OnaylayanKullaniciId = kullaniciId.Value;
        kapanis.OnayTarihi = DateTime.Now;
        kapanis.GuncellemeTarihi = DateTime.Now;
        kapanis.RedNedeni = null;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "Kasa kapanışı onaylandı.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,Muhasebe")]
    public async Task<IActionResult> Reddet(
        int id,
        string? redNedeni,
        string rowVersion)
    {
        var kapanis = await _context.KasaKapanislari
            .FirstOrDefaultAsync(x => x.Id == id);
        if (kapanis is null)
            return NotFound();

        if (kapanis.Durum != KasaKapanisi.DurumHazirlandi)
        {
            TempData["Hata"] =
                "Yalnızca hazırlanmış kasa kapanışları reddedilebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (string.IsNullOrWhiteSpace(redNedeni))
        {
            TempData["Hata"] = "Red nedeni zorunludur.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (redNedeni.Trim().Length > 500)
        {
            TempData["Hata"] = "Red nedeni en fazla 500 karakter olabilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!RowVersionAyarla(kapanis, rowVersion))
        {
            TempData["Hata"] = "Kasa kapanışı sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var kullaniciId = GirisYapanKullaniciId();
        if (!kullaniciId.HasValue)
            return Forbid();

        kapanis.Durum = KasaKapanisi.DurumReddedildi;
        kapanis.OnaylayanKullaniciId = kullaniciId.Value;
        kapanis.OnayTarihi = DateTime.Now;
        kapanis.RedNedeni = redNedeni.Trim();
        kapanis.GuncellemeTarihi = DateTime.Now;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "Kasa kapanışı reddedildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private IQueryable<KasaKapanisi> KapanisSorgusu()
    {
        return _context.KasaKapanislari
            .AsNoTracking()
            .Include(x => x.KasaPersonel)
            .Include(x => x.KasaKullanici)
            .Include(x => x.OnaylayanKullanici)
            .AsQueryable();
    }

    private static IQueryable<KasaKapanisi> FiltreleriUygula(
        IQueryable<KasaKapanisi> query,
        string? arama,
        string durum,
        int? kasaPersonelId,
        DateTime? baslangicTarihi,
        DateTime? bitisTarihi)
    {
        if (!string.IsNullOrWhiteSpace(arama))
        {
            var metin = arama.Trim();
            query = query.Where(x =>
                x.KapanisNo.Contains(metin) ||
                x.KasaPersonel.AdSoyad.Contains(metin) ||
                x.KasaKullanici.KullaniciAdi.Contains(metin));
        }

        if (GecerliDurumlar.Contains(durum))
            query = query.Where(x => x.Durum == durum);

        if (kasaPersonelId.HasValue)
            query = query.Where(x => x.KasaPersonelId == kasaPersonelId.Value);

        if (baslangicTarihi.HasValue)
            query = query.Where(x => x.Tarih >= baslangicTarihi.Value.Date);

        if (bitisTarihi.HasValue)
            query = query.Where(x => x.Tarih <= bitisTarihi.Value.Date);

        return query;
    }

    private static async Task<KasaKapanisiListeViewModel> Sayfala(
        IQueryable<KasaKapanisi> query,
        string? arama,
        string durum,
        int? kasaPersonelId,
        DateTime? baslangicTarihi,
        DateTime? bitisTarihi,
        int page,
        int pageSize)
    {
        var gecerliSayfaBoyutlari = new[] { 10, 25, 50, 100 };
        if (!gecerliSayfaBoyutlari.Contains(pageSize))
            pageSize = 10;

        page = Math.Max(1, page);
        var totalCount = await query.CountAsync();
        var totalPages = Math.Max(
            1,
            (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Min(page, totalPages);

        return new KasaKapanisiListeViewModel
        {
            Arama = arama,
            Durum = GecerliDurumlar.Contains(durum) ? durum : "Tumu",
            KasaPersonelId = kasaPersonelId,
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = await query
                .OrderByDescending(x => x.Tarih)
                .ThenByDescending(x => x.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync()
        };
    }

    private async Task<KasaKapanisiOzetViewModel> BeklenenKasayiHesapla(
        int personelId,
        DateTime tarih)
    {
        var baslangic = tarih.Date;
        var bitis = baslangic.AddDays(1);

        var satislar = await _context.Satislar
            .AsNoTracking()
            .Where(x =>
                x.PersonelId == personelId &&
                x.SatisTarihi >= baslangic &&
                x.SatisTarihi < bitis)
            .Select(x => new
            {
                x.OdemeTipi,
                x.NetTutar
            })
            .ToListAsync();

        var iadeler = await _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Where(x =>
                x.Durum == IadeDegisimTalebi.DurumTamamlandi &&
                x.TamamlanmaTarihi >= baslangic &&
                x.TamamlanmaTarihi < bitis &&
                x.Satis.PersonelId == personelId)
            .Select(x => new
            {
                OdemeTipi = x.OdemeTipiSnapshot ?? x.Satis.OdemeTipi,
                x.ToplamIadeTutari
            })
            .ToListAsync();

        decimal SatisToplami(string odemeTipi) =>
            satislar
                .Where(x => OdemeTipiEslesiyor(x.OdemeTipi, odemeTipi))
                .Sum(x => x.NetTutar);

        decimal IadeToplami(string odemeTipi) =>
            iadeler
                .Where(x => OdemeTipiEslesiyor(x.OdemeTipi, odemeTipi))
                .Sum(x => x.ToplamIadeTutari);

        var nakit = SatisToplami("Nakit") - IadeToplami("Nakit");
        var krediKarti =
            SatisToplami("KrediKarti") - IadeToplami("KrediKarti");
        var havale = SatisToplami("Havale") - IadeToplami("Havale");
        var dagitilanIade =
            IadeToplami("Nakit") +
            IadeToplami("KrediKarti") +
            IadeToplami("Havale");
        var toplamIade = iadeler.Sum(x => x.ToplamIadeTutari);

        return new KasaKapanisiOzetViewModel
        {
            BeklenenNakit = nakit,
            BeklenenKrediKarti = krediKarti,
            BeklenenHavale = havale,
            BeklenenToplam = nakit + krediKarti + havale,
            SatisSayisi = satislar.Count,
            IadeSayisi = iadeler.Count,
            IadeToplami = toplamIade,
            DagitilamayanIadeToplami = toplamIade - dagitilanIade
        };
    }

    private static bool OdemeTipiEslesiyor(
        string? kayitliOdemeTipi,
        string hedef)
    {
        if (string.IsNullOrWhiteSpace(kayitliOdemeTipi))
            return false;

        var deger = kayitliOdemeTipi
            .Replace(" ", "", StringComparison.Ordinal)
            .Replace("ı", "i", StringComparison.OrdinalIgnoreCase);

        return hedef == "KrediKarti"
            ? deger.Equals("KrediKarti", StringComparison.OrdinalIgnoreCase) ||
              deger.Equals("Kart", StringComparison.OrdinalIgnoreCase)
            : deger.Equals(hedef, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<List<SelectListItem>> KasiyerSecenekleriniGetir()
    {
        return await _context.Personeller
            .AsNoTracking()
            .Where(x => x.AktifMi && x.Pozisyon == "Kasiyer")
            .OrderBy(x => x.AdSoyad)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.AdSoyad + " - " + x.Pozisyon
            })
            .ToListAsync();
    }

    private async Task CreateVerileriniDoldur(int? seciliPersonelId)
    {
        ViewData["YoneticiModu"] = !User.IsInRole("Kasiyer");
        ViewData["Kasiyerler"] = new SelectList(
            await KasiyerSecenekleriniGetir(),
            "Value",
            "Text",
            seciliPersonelId?.ToString());
    }

    private static void OzetiModeleAktar(
        KasaKapanisiOlusturViewModel model,
        KasaKapanisiOzetViewModel ozet)
    {
        model.BeklenenNakit = ozet.BeklenenNakit;
        model.BeklenenKrediKarti = ozet.BeklenenKrediKarti;
        model.BeklenenHavale = ozet.BeklenenHavale;
        model.BeklenenToplam = ozet.BeklenenToplam;
        model.SatisSayisi = ozet.SatisSayisi;
        model.IadeSayisi = ozet.IadeSayisi;
        model.IadeToplami = ozet.IadeToplami;
        model.DagitilamayanIadeToplami = ozet.DagitilamayanIadeToplami;
    }

    private async Task BugunOzetiniDoldur(
        KasaKapanisiListeViewModel model,
        int? kullaniciId)
    {
        var bugun = DateTime.Today;
        var query = _context.KasaKapanislari
            .AsNoTracking()
            .Where(x =>
                x.Tarih == bugun &&
                (x.Durum == KasaKapanisi.DurumHazirlandi ||
                 x.Durum == KasaKapanisi.DurumOnaylandi));

        if (kullaniciId.HasValue)
            query = query.Where(x => x.KasaKullaniciId == kullaniciId.Value);

        model.BugunkuBeklenenToplam =
            await query.SumAsync(x => (decimal?)x.BeklenenToplam) ?? 0;
        model.BugunkuSayilanToplam =
            await query.SumAsync(x => (decimal?)x.SayilanToplam) ?? 0;
        model.BugunkuFark =
            await query.SumAsync(x => (decimal?)x.FarkToplam) ?? 0;
        model.OnayBekleyenKapanis =
            await query.CountAsync(x => x.Durum == KasaKapanisi.DurumHazirlandi);
    }

    private async Task<Kullanici?> GirisYapanKullaniciyiGetir()
    {
        var kullaniciId = GirisYapanKullaniciId();
        if (!kullaniciId.HasValue)
            return null;

        return await _context.Kullanicilar
            .AsNoTracking()
            .Include(x => x.Personel)
            .FirstOrDefaultAsync(x => x.Id == kullaniciId.Value && x.AktifMi);
    }

    private int? GirisYapanKullaniciId()
    {
        var deger = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(deger, out var id) ? id : null;
    }

    private bool RowVersionAyarla(
        KasaKapanisi kapanis,
        string rowVersion)
    {
        try
        {
            _context.Entry(kapanis)
                .Property(x => x.RowVersion)
                .OriginalValue = Convert.FromBase64String(rowVersion);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private async Task<bool> DegisiklikleriKaydet()
    {
        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Hata"] =
                "Kasa kapanışı başka bir kullanıcı tarafından güncellendi. Sayfayı yenileyip tekrar deneyiniz.";
            return false;
        }
    }

    private static string? AciklamayiHazirla(
        string? aciklama,
        decimal dagitilamayanIade)
    {
        var metin = string.IsNullOrWhiteSpace(aciklama)
            ? null
            : aciklama.Trim();

        if (dagitilamayanIade <= 0)
            return metin;

        var not =
            $"Ödeme tipine dağıtılamayan iade toplamı: {dagitilamayanIade:N2} TL.";
        return string.IsNullOrWhiteSpace(metin)
            ? not
            : $"{metin} {not}";
    }

    private static bool KapanisOzetiDegismisMi(
        KasaKapanisi kapanis,
        KasaKapanisiOzetViewModel guncelOzet)
    {
        return kapanis.BeklenenNakit != guncelOzet.BeklenenNakit ||
               kapanis.BeklenenKrediKarti != guncelOzet.BeklenenKrediKarti ||
               kapanis.BeklenenHavale != guncelOzet.BeklenenHavale ||
               kapanis.BeklenenToplam != guncelOzet.BeklenenToplam ||
               kapanis.SatisSayisi != guncelOzet.SatisSayisi ||
               kapanis.IadeSayisi != guncelOzet.IadeSayisi ||
               kapanis.IadeToplami != guncelOzet.IadeToplami;
    }

    private void SayilanTutarHatalariniTurkcelestir()
    {
        TutarAlaniHatasiniTurkcelestir(
            nameof(KasaKapanisiOlusturViewModel.SayilanNakit),
            "Sayılan nakit");
        TutarAlaniHatasiniTurkcelestir(
            nameof(KasaKapanisiOlusturViewModel.SayilanKrediKarti),
            "Sayılan kredi kartı");
        TutarAlaniHatasiniTurkcelestir(
            nameof(KasaKapanisiOlusturViewModel.SayilanHavale),
            "Sayılan havale");
    }

    private void TutarAlaniHatasiniTurkcelestir(
        string alanAdi,
        string gorunenAd)
    {
        if (!ModelState.TryGetValue(alanAdi, out var alan) ||
            alan.Errors.Count == 0)
        {
            return;
        }

        var bosMu = string.IsNullOrWhiteSpace(alan.AttemptedValue);
        alan.Errors.Clear();
        alan.Errors.Add(
            bosMu
                ? $"{gorunenAd} alanı zorunludur."
                : $"{gorunenAd} alanına geçerli bir tutar giriniz.");
    }
}
