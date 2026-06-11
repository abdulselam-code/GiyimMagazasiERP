using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,InsanKaynaklari,Kasiyer,Personel,Depo,Muhasebe")]
public class PersonelIzinleriController : Controller
{
    private static readonly string[] GecerliDurumlar =
    {
        PersonelIzni.DurumOnayBekliyor,
        PersonelIzni.DurumOnaylandi,
        PersonelIzni.DurumReddedildi,
        PersonelIzni.DurumIptalEdildi
    };

    private readonly AppDbContext _context;

    public PersonelIzinleriController(AppDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Admin,Yonetici,InsanKaynaklari")]
    public async Task<IActionResult> Index(
        string? arama,
        string durum = "Tumu",
        string izinTuru = "Tumu",
        DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null,
        int page = 1,
        int pageSize = 10)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var yonetebilir = IzinleriYonetebilirMi();
        var personelEslesmesiVar = kullanici.PersonelId.HasValue;

        var query = IzinSorgusu();

        if (!yonetebilir)
        {
            query = personelEslesmesiVar
                ? query.Where(x => x.PersonelId == kullanici.PersonelId!.Value)
                : query.Where(x => false);
        }

        query = FiltreleriUygula(
            query,
            arama,
            durum,
            izinTuru,
            baslangicTarihi,
            bitisTarihi);

        var model = await Sayfala(
            query,
            arama,
            durum,
            izinTuru,
            baslangicTarihi,
            bitisTarihi,
            page,
            pageSize);

        model.PersonelEslesmesiVarMi = yonetebilir || personelEslesmesiVar;

        var yil = DateTime.Today.Year;
        var aktifPersonelIds = await _context.Personeller
            .AsNoTracking()
            .Where(x => x.AktifMi)
            .Select(x => x.Id)
            .ToListAsync();
        var bakiyeler = await GetIzinBakiyeleriAsync(aktifPersonelIds, yil);
        var yilBaslangici = new DateTime(yil, 1, 1);
        var sonrakiYilBaslangici = yilBaslangici.AddYears(1);

        model.ToplamYillikIzinHakki = bakiyeler.Sum(x => x.ToplamIzinHakki);
        model.ToplamKullanilanIzinGunu = bakiyeler.Sum(x => x.KullanilanIzinGunu);
        model.ToplamKalanIzinGunu = bakiyeler.Sum(x => x.KalanIzinGunu);
        model.DigerOnayliIzinGunu = bakiyeler.Sum(x => x.DigerOnayliIzinGunu);
        model.OnayBekleyenIzinSayisi = await _context.PersonelIzinleri
            .AsNoTracking()
            .CountAsync(x =>
                x.Durum == PersonelIzni.DurumOnayBekliyor &&
                x.BaslangicTarihi < sonrakiYilBaslangici &&
                x.BitisTarihi >= yilBaslangici);

        return View(model);
    }

    public async Task<IActionResult> BenimIzinlerim(
        string? arama,
        string durum = "Tumu",
        string izinTuru = "Tumu",
        DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null,
        int page = 1,
        int pageSize = 10)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var query = IzinSorgusu();

        query = kullanici.PersonelId.HasValue
            ? query.Where(x => x.PersonelId == kullanici.PersonelId.Value)
            : query.Where(x => false);

        query = FiltreleriUygula(
            query,
            arama,
            durum,
            izinTuru,
            baslangicTarihi,
            bitisTarihi);

        var model = await Sayfala(
            query,
            arama,
            durum,
            izinTuru,
            baslangicTarihi,
            bitisTarihi,
            page,
            pageSize);

        model.PersonelEslesmesiVarMi = kullanici.PersonelId.HasValue;
        if (kullanici.PersonelId.HasValue)
        {
            model.IzinBakiyesi = await GetIzinBakiyesiAsync(
                kullanici.PersonelId.Value,
                DateTime.Today.Year);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        if (!IzinleriYonetebilirMi() && !kullanici.PersonelId.HasValue)
        {
            TempData["Hata"] =
                "Bu kullanıcıya bağlı aktif personel kaydı bulunmadığı için izin talebi oluşturulamaz.";
            return RedirectToAction(nameof(BenimIzinlerim));
        }

        await CreateVerileriniDoldur(kullanici, kullanici.PersonelId);

        return View(new PersonelIzniOlusturViewModel
        {
            PersonelId = kullanici.PersonelId,
            BaslangicTarihi = DateTime.Today,
            BitisTarihi = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonelIzniOlusturViewModel model)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var yonetebilir = IzinleriYonetebilirMi();
        var personelId = yonetebilir ? model.PersonelId : kullanici.PersonelId;
        model.PersonelId = personelId;

        if (!personelId.HasValue)
        {
            ModelState.AddModelError(
                nameof(model.PersonelId),
                "İzin talebi oluşturulacak personel bulunamadı.");
        }

        if (!PersonelIzni.IzinTurleri.Contains(model.IzinTuru))
        {
            ModelState.AddModelError(
                nameof(model.IzinTuru),
                "Geçerli bir izin türü seçilmelidir.");
        }

        if (!model.BaslangicTarihi.HasValue || !model.BitisTarihi.HasValue)
        {
            ModelState.AddModelError("", "Başlangıç ve bitiş tarihleri zorunludur.");
        }
        else if (model.BaslangicTarihi.Value.Date > model.BitisTarihi.Value.Date)
        {
            ModelState.AddModelError(
                nameof(model.BitisTarihi),
                "Bitiş tarihi başlangıç tarihinden önce olamaz.");
        }

        Personel? personel = null;
        if (personelId.HasValue)
        {
            personel = await _context.Personeller
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == personelId.Value && x.AktifMi);

            if (personel is null)
            {
                ModelState.AddModelError(
                    nameof(model.PersonelId),
                    "Seçilen personel bulunamadı veya aktif değil.");
            }
        }

        if (!yonetebilir &&
            kullanici.PersonelId != personelId)
        {
            return Forbid();
        }

        if (personel is not null &&
            model.BaslangicTarihi.HasValue &&
            model.BitisTarihi.HasValue &&
            model.BaslangicTarihi.Value.Date <= model.BitisTarihi.Value.Date &&
            await IzinCakisiyorMu(
                personel.Id,
                model.BaslangicTarihi.Value.Date,
                model.BitisTarihi.Value.Date))
        {
            ModelState.AddModelError(
                "",
                "Seçilen tarih aralığında bu personele ait bekleyen veya onaylı başka bir izin bulunmaktadır.");
        }

        if (personel is not null &&
            model.IzinTuru == "Yıllık İzin" &&
            model.BaslangicTarihi.HasValue &&
            model.BitisTarihi.HasValue &&
            model.BaslangicTarihi.Value.Date <= model.BitisTarihi.Value.Date &&
            !await YillikIzinBakiyesiYeterliMi(
                personel.Id,
                model.BaslangicTarihi.Value.Date,
                model.BitisTarihi.Value.Date))
        {
            ModelState.AddModelError(
                "",
                "Talep edilen yıllık izin günü kalan izin hakkını aşamaz.");
        }

        if (!ModelState.IsValid)
        {
            await CreateVerileriniDoldur(kullanici, personelId);
            return View(model);
        }

        var baslangic = model.BaslangicTarihi!.Value.Date;
        var bitis = model.BitisTarihi!.Value.Date;

        var izin = new PersonelIzni
        {
            PersonelId = personel!.Id,
            KullaniciId = kullanici.Id,
            IzinTuru = model.IzinTuru,
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            GunSayisi = (bitis - baslangic).Days + 1,
            Aciklama = string.IsNullOrWhiteSpace(model.Aciklama)
                ? null
                : model.Aciklama.Trim(),
            Durum = PersonelIzni.DurumOnayBekliyor,
            OlusturmaTarihi = DateTime.Now
        };

        _context.PersonelIzinleri.Add(izin);
        await _context.SaveChangesAsync();

        TempData["Basari"] = "İzin talebi oluşturuldu ve onay sürecine gönderildi.";

        if (yonetebilir)
            return RedirectToAction(nameof(Details), new { id = izin.Id });

        return RedirectToAction(nameof(BenimIzinlerim));
    }

    public async Task<IActionResult> Details(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var izin = await IzinSorgusu()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (izin is null)
            return NotFound();

        if (!IzinleriYonetebilirMi() &&
            (!kullanici.PersonelId.HasValue ||
             izin.PersonelId != kullanici.PersonelId.Value))
        {
            return Forbid();
        }

        ViewData["TalepSahibiMi"] =
            kullanici.PersonelId.HasValue &&
            izin.PersonelId == kullanici.PersonelId.Value;
        ViewData["IzinBakiyesi"] = await GetIzinBakiyesiAsync(
            izin.PersonelId,
            izin.BaslangicTarihi.Year);

        return View(izin);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,InsanKaynaklari")]
    public async Task<IActionResult> Onayla(int id, string rowVersion)
    {
        var izin = await _context.PersonelIzinleri
            .FirstOrDefaultAsync(x => x.Id == id);

        if (izin is null)
            return NotFound();

        if (izin.Durum != PersonelIzni.DurumOnayBekliyor)
        {
            TempData["Hata"] = "Yalnızca onay bekleyen izin talepleri onaylanabilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (await IzinCakisiyorMu(
                izin.PersonelId,
                izin.BaslangicTarihi,
                izin.BitisTarihi,
                izin.Id))
        {
            TempData["Hata"] =
                "Bu izin başka bir bekleyen veya onaylı izinle çakıştığı için onaylanamadı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (izin.IzinTuru == "Yıllık İzin" &&
            !await YillikIzinBakiyesiYeterliMi(
                izin.PersonelId,
                izin.BaslangicTarihi,
                izin.BitisTarihi))
        {
            TempData["Hata"] =
                "Personelin kalan yıllık izin hakkı bu talebi onaylamak için yeterli değildir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!RowVersionAyarla(izin, rowVersion))
        {
            TempData["Hata"] = "İzin kaydının sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var kullaniciId = GirisYapanKullaniciId();
        if (!kullaniciId.HasValue)
            return Forbid();

        izin.Durum = PersonelIzni.DurumOnaylandi;
        izin.OnaylayanKullaniciId = kullaniciId;
        izin.OnayTarihi = DateTime.Now;
        izin.GuncellemeTarihi = DateTime.Now;
        izin.RedNedeni = null;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "İzin talebi onaylandı.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,InsanKaynaklari")]
    public async Task<IActionResult> Reddet(
        int id,
        string? redNedeni,
        string rowVersion)
    {
        var izin = await _context.PersonelIzinleri
            .FirstOrDefaultAsync(x => x.Id == id);

        if (izin is null)
            return NotFound();

        if (izin.Durum != PersonelIzni.DurumOnayBekliyor)
        {
            TempData["Hata"] = "Yalnızca onay bekleyen izin talepleri reddedilebilir.";
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

        if (!RowVersionAyarla(izin, rowVersion))
        {
            TempData["Hata"] = "İzin kaydının sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var kullaniciId = GirisYapanKullaniciId();
        if (!kullaniciId.HasValue)
            return Forbid();

        izin.Durum = PersonelIzni.DurumReddedildi;
        izin.OnaylayanKullaniciId = kullaniciId;
        izin.OnayTarihi = DateTime.Now;
        izin.RedNedeni = redNedeni.Trim();
        izin.GuncellemeTarihi = DateTime.Now;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "İzin talebi reddedildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IptalEt(int id, string rowVersion)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var izin = await _context.PersonelIzinleri
            .FirstOrDefaultAsync(x => x.Id == id);

        if (izin is null)
            return NotFound();

        var sahibi =
            kullanici.PersonelId.HasValue &&
            izin.PersonelId == kullanici.PersonelId.Value;

        if (!IzinleriYonetebilirMi() && !sahibi)
            return Forbid();

        if (izin.Durum != PersonelIzni.DurumOnayBekliyor)
        {
            TempData["Hata"] = "Yalnızca onay bekleyen izin talepleri iptal edilebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!RowVersionAyarla(izin, rowVersion))
        {
            TempData["Hata"] = "İzin kaydının sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        izin.Durum = PersonelIzni.DurumIptalEdildi;
        izin.IptalTarihi = DateTime.Now;
        izin.GuncellemeTarihi = DateTime.Now;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "İzin talebi iptal edildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private IQueryable<PersonelIzni> IzinSorgusu()
    {
        return _context.PersonelIzinleri
            .AsNoTracking()
            .Include(x => x.Personel)
            .Include(x => x.Kullanici)
            .Include(x => x.OnaylayanKullanici)
            .AsQueryable();
    }

    private static IQueryable<PersonelIzni> FiltreleriUygula(
        IQueryable<PersonelIzni> query,
        string? arama,
        string durum,
        string izinTuru,
        DateTime? baslangicTarihi,
        DateTime? bitisTarihi)
    {
        if (!string.IsNullOrWhiteSpace(arama))
        {
            var metin = arama.Trim();
            query = query.Where(x =>
                x.Personel.AdSoyad.Contains(metin) ||
                x.IzinTuru.Contains(metin) ||
                x.Kullanici.KullaniciAdi.Contains(metin) ||
                (x.Kullanici.AdSoyad != null && x.Kullanici.AdSoyad.Contains(metin)) ||
                (x.Aciklama != null && x.Aciklama.Contains(metin)));
        }

        if (GecerliDurumlar.Contains(durum))
            query = query.Where(x => x.Durum == durum);

        if (PersonelIzni.IzinTurleri.Contains(izinTuru))
            query = query.Where(x => x.IzinTuru == izinTuru);

        if (baslangicTarihi.HasValue)
            query = query.Where(x => x.BitisTarihi >= baslangicTarihi.Value.Date);

        if (bitisTarihi.HasValue)
            query = query.Where(x => x.BaslangicTarihi <= bitisTarihi.Value.Date);

        return query;
    }

    private static async Task<PersonelIzniListeViewModel> Sayfala(
        IQueryable<PersonelIzni> query,
        string? arama,
        string durum,
        string izinTuru,
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
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        page = Math.Min(page, totalPages);

        var items = await query
            .OrderByDescending(x => x.OlusturmaTarihi)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PersonelIzniListeViewModel
        {
            Arama = arama,
            Durum = GecerliDurumlar.Contains(durum) ? durum : "Tumu",
            IzinTuru = PersonelIzni.IzinTurleri.Contains(izinTuru)
                ? izinTuru
                : "Tumu",
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items
        };
    }

    private async Task<bool> IzinCakisiyorMu(
        int personelId,
        DateTime baslangic,
        DateTime bitis,
        int? haricIzinId = null)
    {
        return await _context.PersonelIzinleri
            .AsNoTracking()
            .AnyAsync(x =>
                x.PersonelId == personelId &&
                (!haricIzinId.HasValue || x.Id != haricIzinId.Value) &&
                (x.Durum == PersonelIzni.DurumOnayBekliyor ||
                 x.Durum == PersonelIzni.DurumOnaylandi) &&
                x.BaslangicTarihi <= bitis &&
                x.BitisTarihi >= baslangic);
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

    private bool IzinleriYonetebilirMi()
    {
        return User.IsInRole("Admin") ||
               User.IsInRole("Yonetici") ||
               User.IsInRole("InsanKaynaklari");
    }

    private async Task CreateVerileriniDoldur(
        Kullanici kullanici,
        int? seciliPersonelId)
    {
        ViewData["IzinTurleri"] = new SelectList(PersonelIzni.IzinTurleri);
        ViewData["YoneticiModu"] = IzinleriYonetebilirMi();
        ViewData["KendiPersonelAdi"] =
            kullanici.Personel?.AdSoyad ?? "Bağlı personel kaydı bulunamadı";
        ViewData["KendiPersonelDetayi"] = kullanici.Personel is null
            ? null
            : $"{kullanici.Personel.Pozisyon} / {kullanici.Personel.Departman}";

        var yil = DateTime.Today.Year;

        if (IzinleriYonetebilirMi())
        {
            var personeller = await _context.Personeller
                .AsNoTracking()
                .Where(x => x.AktifMi)
                .OrderBy(x => x.AdSoyad)
                .Select(x => new
                {
                    x.Id,
                    Etiket = x.AdSoyad + " - " + x.Pozisyon
                })
                .ToListAsync();

            ViewData["Personeller"] =
                new SelectList(personeller, "Id", "Etiket", seciliPersonelId);

            ViewData["IzinBakiyeleri"] = await GetIzinBakiyeleriAsync(
                personeller.Select(x => x.Id),
                yil);
        }
        else if (kullanici.PersonelId.HasValue)
        {
            ViewData["IzinBakiyesi"] = await GetIzinBakiyesiAsync(
                kullanici.PersonelId.Value,
                yil);
        }
    }

    private async Task<PersonelIzinBakiyesiViewModel> GetIzinBakiyesiAsync(
        int personelId,
        int yil)
    {
        var bakiyeler = await GetIzinBakiyeleriAsync(new[] { personelId }, yil);
        return bakiyeler.FirstOrDefault() ?? new PersonelIzinBakiyesiViewModel
        {
            PersonelId = personelId,
            Yil = yil,
            YillikIzinHakki = 14m
        };
    }

    private async Task<List<PersonelIzinBakiyesiViewModel>> GetIzinBakiyeleriAsync(
        IEnumerable<int> personelIds,
        int yil)
    {
        var idler = personelIds.Distinct().ToList();
        if (idler.Count == 0)
            return new List<PersonelIzinBakiyesiViewModel>();

        var personeller = await _context.Personeller
            .AsNoTracking()
            .Where(x => idler.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.AdSoyad,
                x.Pozisyon,
                x.Departman
            })
            .ToListAsync();

        var kayitliBakiyeler = await _context.PersonelIzinBakiyeleri
            .AsNoTracking()
            .Where(x => idler.Contains(x.PersonelId) && x.Yil == yil)
            .ToDictionaryAsync(x => x.PersonelId);

        var yilBaslangici = new DateTime(yil, 1, 1);
        var sonrakiYilBaslangici = yilBaslangici.AddYears(1);
        var izinler = await _context.PersonelIzinleri
            .AsNoTracking()
            .Where(x =>
                idler.Contains(x.PersonelId) &&
                x.Durum == PersonelIzni.DurumOnaylandi &&
                x.BaslangicTarihi < sonrakiYilBaslangici &&
                x.BitisTarihi >= yilBaslangici)
            .Select(x => new
            {
                x.PersonelId,
                x.IzinTuru,
                x.BaslangicTarihi,
                x.BitisTarihi
            })
            .ToListAsync();

        return personeller
            .Select(personel =>
            {
                kayitliBakiyeler.TryGetValue(personel.Id, out var bakiye);
                var kullanilan = izinler
                    .Where(x =>
                        x.PersonelId == personel.Id &&
                        x.IzinTuru == "Yıllık İzin")
                    .Sum(x => YildakiIzinGununuHesapla(
                        x.BaslangicTarihi,
                        x.BitisTarihi,
                        yil));
                var digerOnayli = izinler
                    .Where(x =>
                        x.PersonelId == personel.Id &&
                        x.IzinTuru != "Yıllık İzin")
                    .Sum(x => YildakiIzinGununuHesapla(
                        x.BaslangicTarihi,
                        x.BitisTarihi,
                        yil));
                var yillikHak = bakiye?.YillikIzinHakki ?? 14m;
                var devreden = bakiye?.DevredenIzinGunu ?? 0m;

                return new PersonelIzinBakiyesiViewModel
                {
                    PersonelId = personel.Id,
                    PersonelAdi = personel.AdSoyad,
                    Pozisyon = personel.Pozisyon,
                    Departman = personel.Departman,
                    Yil = yil,
                    YillikIzinHakki = yillikHak,
                    DevredenIzinGunu = devreden,
                    KullanilanIzinGunu = kullanilan,
                    KalanIzinGunu = yillikHak + devreden - kullanilan,
                    DigerOnayliIzinGunu = digerOnayli
                };
            })
            .OrderBy(x => x.PersonelAdi)
            .ToList();
    }

    private async Task<bool> YillikIzinBakiyesiYeterliMi(
        int personelId,
        DateTime baslangic,
        DateTime bitis)
    {
        for (var yil = baslangic.Year; yil <= bitis.Year; yil++)
        {
            var talepEdilenGun = YildakiIzinGununuHesapla(
                baslangic,
                bitis,
                yil);
            var bakiye = await GetIzinBakiyesiAsync(personelId, yil);

            if (talepEdilenGun > bakiye.KalanIzinGunu)
                return false;
        }

        return true;
    }

    private static decimal YildakiIzinGununuHesapla(
        DateTime baslangic,
        DateTime bitis,
        int yil)
    {
        var yilBaslangici = new DateTime(yil, 1, 1);
        var yilBitisi = yilBaslangici.AddYears(1).AddDays(-1);
        var hesapBaslangici = baslangic.Date < yilBaslangici
            ? yilBaslangici
            : baslangic.Date;
        var hesapBitisi = bitis.Date > yilBitisi
            ? yilBitisi
            : bitis.Date;

        return hesapBitisi < hesapBaslangici
            ? 0m
            : (hesapBitisi - hesapBaslangici).Days + 1;
    }

    private bool RowVersionAyarla(PersonelIzni izin, string rowVersion)
    {
        try
        {
            _context.Entry(izin)
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
                "İzin kaydı başka bir kullanıcı tarafından güncellendi. Sayfayı yenileyip tekrar deneyiniz.";
            return false;
        }
    }
}
