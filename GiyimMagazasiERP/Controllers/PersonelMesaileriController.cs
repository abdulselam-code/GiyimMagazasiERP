using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,InsanKaynaklari,Muhasebe,Kasiyer,Personel,Depo")]
public class PersonelMesaileriController : Controller
{
    private static readonly string[] GecerliDurumlar =
    {
        PersonelMesaiKaydi.DurumOnayBekliyor,
        PersonelMesaiKaydi.DurumOnaylandi,
        PersonelMesaiKaydi.DurumReddedildi,
        PersonelMesaiKaydi.DurumIptalEdildi
    };

    private readonly AppDbContext _context;

    public PersonelMesaileriController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? arama,
        string durum = "Tumu",
        string mesaiTuru = "Tumu",
        int? personelId = null,
        DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null,
        int page = 1,
        int pageSize = 10)
    {
        if (!MesaileriYonetebilirMi() && !User.IsInRole("Muhasebe"))
            return RedirectToAction(nameof(BenimMesailerim));

        var sadeceOnayli = User.IsInRole("Muhasebe");
        var query = MesaiSorgusu();

        if (sadeceOnayli)
        {
            query = query.Where(x => x.Durum == PersonelMesaiKaydi.DurumOnaylandi);
            durum = PersonelMesaiKaydi.DurumOnaylandi;
        }

        query = FiltreleriUygula(
            query,
            arama,
            durum,
            mesaiTuru,
            personelId,
            baslangicTarihi,
            bitisTarihi);

        var model = await Sayfala(
            query,
            arama,
            durum,
            mesaiTuru,
            personelId,
            baslangicTarihi,
            bitisTarihi,
            page,
            pageSize);

        model.SadeceOnayliKayitlar = sadeceOnayli;
        model.Personeller = await AktifPersonelSecenekleriniGetir();
        model.Ozet = await OzetGetir(null, sadeceOnayli);

        return View(model);
    }

    public async Task<IActionResult> BenimMesailerim(
        string? arama,
        string durum = "Tumu",
        string mesaiTuru = "Tumu",
        DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null,
        int page = 1,
        int pageSize = 10)
    {
        if (User.IsInRole("Muhasebe"))
            return RedirectToAction(nameof(Index));

        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var query = MesaiSorgusu();
        query = kullanici.PersonelId.HasValue
            ? query.Where(x => x.PersonelId == kullanici.PersonelId.Value)
            : query.Where(x => false);

        query = FiltreleriUygula(
            query,
            arama,
            durum,
            mesaiTuru,
            null,
            baslangicTarihi,
            bitisTarihi);

        var model = await Sayfala(
            query,
            arama,
            durum,
            mesaiTuru,
            null,
            baslangicTarihi,
            bitisTarihi,
            page,
            pageSize);

        model.PersonelEslesmesiVarMi = kullanici.PersonelId.HasValue;
        model.Ozet = kullanici.PersonelId.HasValue
            ? await OzetGetir(kullanici.PersonelId.Value, false)
            : new PersonelMesaiOzetViewModel();

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (User.IsInRole("Muhasebe"))
            return Forbid();

        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        if (!MesaileriYonetebilirMi() && !kullanici.PersonelId.HasValue)
        {
            TempData["Hata"] =
                "Bu kullanıcıya bağlı aktif personel kaydı bulunmadığı için mesai talebi oluşturulamaz.";
            return RedirectToAction(nameof(BenimMesailerim));
        }

        await CreateVerileriniDoldur(kullanici, kullanici.PersonelId);

        return View(new PersonelMesaiOlusturViewModel
        {
            PersonelId = kullanici.PersonelId,
            Tarih = DateTime.Today,
            VardiyaBaslangic = new TimeSpan(9, 0, 0),
            VardiyaBitis = new TimeSpan(18, 0, 0),
            MesaiTuru = MesaileriYonetebilirMi()
                ? "Normal Vardiya"
                : "Fazla Mesai"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PersonelMesaiOlusturViewModel model)
    {
        if (User.IsInRole("Muhasebe"))
            return Forbid();

        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var yonetebilir = MesaileriYonetebilirMi();
        var personelId = yonetebilir ? model.PersonelId : kullanici.PersonelId;
        model.PersonelId = personelId;

        if (!personelId.HasValue)
            ModelState.AddModelError(nameof(model.PersonelId), "Mesai kaydı oluşturulacak personel bulunamadı.");

        var izinliTurler = yonetebilir
            ? PersonelMesaiKaydi.MesaiTurleri
            : PersonelMesaiKaydi.PersonelTalepTurleri;

        if (!izinliTurler.Contains(model.MesaiTuru))
            ModelState.AddModelError(nameof(model.MesaiTuru), "Bu mesai türünü seçme yetkiniz bulunmuyor.");

        if (!model.Tarih.HasValue)
            ModelState.AddModelError(nameof(model.Tarih), "Tarih seçilmelidir.");

        if (!model.VardiyaBaslangic.HasValue || !model.VardiyaBitis.HasValue)
        {
            ModelState.AddModelError("", "Vardiya başlangıç ve bitiş saatleri zorunludur.");
        }
        else if (model.VardiyaBaslangic.Value == model.VardiyaBitis.Value)
        {
            ModelState.AddModelError(nameof(model.VardiyaBitis), "Vardiya başlangıç ve bitiş saatleri aynı olamaz.");
        }

        var tekGercekSaatGirildi =
            model.GercekGiris.HasValue != model.GercekCikis.HasValue;
        if (tekGercekSaatGirildi)
        {
            ModelState.AddModelError(
                "",
                "Gerçek giriş ve gerçek çıkış saatleri birlikte girilmelidir.");
        }
        else if (model.GercekGiris.HasValue &&
                 model.GercekCikis.HasValue &&
                 model.GercekGiris.Value == model.GercekCikis.Value)
        {
            ModelState.AddModelError(
                nameof(model.GercekCikis),
                "Gerçek giriş ve çıkış saatleri aynı olamaz.");
        }

        Personel? personel = null;
        if (personelId.HasValue)
        {
            personel = await _context.Personeller
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == personelId.Value && x.AktifMi);

            if (personel is null)
                ModelState.AddModelError(nameof(model.PersonelId), "Seçilen personel bulunamadı veya aktif değil.");
        }

        if (!yonetebilir && kullanici.PersonelId != personelId)
            return Forbid();

        if (personel is not null &&
            model.Tarih.HasValue &&
            model.VardiyaBaslangic.HasValue &&
            model.VardiyaBitis.HasValue &&
            model.VardiyaBaslangic.Value != model.VardiyaBitis.Value &&
            await MesaiCakisiyorMu(
                personel.Id,
                model.Tarih.Value.Date,
                model.VardiyaBaslangic.Value,
                model.VardiyaBitis.Value))
        {
            ModelState.AddModelError(
                "",
                "Seçilen tarih ve saat aralığında bu personele ait bekleyen veya onaylı başka bir mesai kaydı bulunuyor.");
        }

        if (!ModelState.IsValid)
        {
            await CreateVerileriniDoldur(kullanici, personelId);
            return View(model);
        }

        var planlananSaat = SureHesapla(
            model.VardiyaBaslangic!.Value,
            model.VardiyaBitis!.Value);
        var gerceklesenSaat =
            model.GercekGiris.HasValue && model.GercekCikis.HasValue
                ? SureHesapla(model.GercekGiris.Value, model.GercekCikis.Value)
                : 0m;
        var fazlaMesaiSaati = FazlaMesaiHesapla(
            model.MesaiTuru,
            planlananSaat,
            gerceklesenSaat);

        var kayit = new PersonelMesaiKaydi
        {
            PersonelId = personel!.Id,
            KullaniciId = kullanici.Id,
            Tarih = model.Tarih!.Value.Date,
            VardiyaBaslangic = model.VardiyaBaslangic.Value,
            VardiyaBitis = model.VardiyaBitis.Value,
            GercekGiris = model.GercekGiris,
            GercekCikis = model.GercekCikis,
            PlanlananSaat = planlananSaat,
            GerceklesenSaat = gerceklesenSaat,
            FazlaMesaiSaati = fazlaMesaiSaati,
            MesaiTuru = model.MesaiTuru,
            Durum = PersonelMesaiKaydi.DurumOnayBekliyor,
            Aciklama = string.IsNullOrWhiteSpace(model.Aciklama)
                ? null
                : model.Aciklama.Trim(),
            OlusturmaTarihi = DateTime.Now
        };

        _context.PersonelMesaiKayitlari.Add(kayit);
        await _context.SaveChangesAsync();

        TempData["Basari"] = "Mesai kaydı oluşturuldu ve onay sürecine gönderildi.";
        return RedirectToAction(nameof(Details), new { id = kayit.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var kayit = await MesaiSorgusu()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kayit is null)
            return NotFound();

        if (User.IsInRole("Muhasebe") &&
            kayit.Durum != PersonelMesaiKaydi.DurumOnaylandi)
        {
            return Forbid();
        }

        if (!MesaileriYonetebilirMi() &&
            !User.IsInRole("Muhasebe") &&
            (!kullanici.PersonelId.HasValue ||
             kayit.PersonelId != kullanici.PersonelId.Value))
        {
            return Forbid();
        }

        ViewData["TalepSahibiMi"] =
            kullanici.PersonelId.HasValue &&
            kayit.PersonelId == kullanici.PersonelId.Value;

        return View(kayit);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,InsanKaynaklari")]
    public async Task<IActionResult> Onayla(int id, string rowVersion)
    {
        var kayit = await _context.PersonelMesaiKayitlari
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kayit is null)
            return NotFound();

        if (kayit.Durum != PersonelMesaiKaydi.DurumOnayBekliyor)
        {
            TempData["Hata"] = "Yalnızca onay bekleyen mesai kayıtları onaylanabilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (await MesaiCakisiyorMu(
                kayit.PersonelId,
                kayit.Tarih,
                kayit.VardiyaBaslangic,
                kayit.VardiyaBitis,
                kayit.Id))
        {
            TempData["Hata"] = "Bu kayıt başka bir bekleyen veya onaylı mesai kaydıyla çakıştığı için onaylanamadı.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!RowVersionAyarla(kayit, rowVersion))
        {
            TempData["Hata"] = "Mesai kaydının sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var kullaniciId = GirisYapanKullaniciId();
        if (!kullaniciId.HasValue)
            return Forbid();

        kayit.FazlaMesaiSaati = FazlaMesaiHesapla(
            kayit.MesaiTuru,
            kayit.PlanlananSaat,
            kayit.GerceklesenSaat);
        kayit.Durum = PersonelMesaiKaydi.DurumOnaylandi;
        kayit.OnaylayanKullaniciId = kullaniciId.Value;
        kayit.OnayTarihi = DateTime.Now;
        kayit.RedNedeni = null;
        kayit.GuncellemeTarihi = DateTime.Now;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "Mesai kaydı onaylandı.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,InsanKaynaklari")]
    public async Task<IActionResult> Reddet(int id, string? redNedeni, string rowVersion)
    {
        var kayit = await _context.PersonelMesaiKayitlari
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kayit is null)
            return NotFound();

        if (kayit.Durum != PersonelMesaiKaydi.DurumOnayBekliyor)
        {
            TempData["Hata"] = "Yalnızca onay bekleyen mesai kayıtları reddedilebilir.";
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

        if (!RowVersionAyarla(kayit, rowVersion))
        {
            TempData["Hata"] = "Mesai kaydının sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var kullaniciId = GirisYapanKullaniciId();
        if (!kullaniciId.HasValue)
            return Forbid();

        kayit.Durum = PersonelMesaiKaydi.DurumReddedildi;
        kayit.OnaylayanKullaniciId = kullaniciId.Value;
        kayit.OnayTarihi = DateTime.Now;
        kayit.RedNedeni = redNedeni.Trim();
        kayit.GuncellemeTarihi = DateTime.Now;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "Mesai kaydı reddedildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IptalEt(int id, string rowVersion)
    {
        var kullanici = await GirisYapanKullaniciyiGetir();
        if (kullanici is null)
            return Forbid();

        var kayit = await _context.PersonelMesaiKayitlari
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kayit is null)
            return NotFound();

        var sahibi =
            kullanici.PersonelId.HasValue &&
            kayit.PersonelId == kullanici.PersonelId.Value;

        if (!MesaileriYonetebilirMi() && !sahibi)
            return Forbid();

        if (kayit.Durum != PersonelMesaiKaydi.DurumOnayBekliyor)
        {
            TempData["Hata"] = "Yalnızca onay bekleyen mesai kayıtları iptal edilebilir.";
            return RedirectToAction(nameof(Details), new { id });
        }

        if (!RowVersionAyarla(kayit, rowVersion))
        {
            TempData["Hata"] = "Mesai kaydının sürüm bilgisi geçersiz.";
            return RedirectToAction(nameof(Details), new { id });
        }

        kayit.Durum = PersonelMesaiKaydi.DurumIptalEdildi;
        kayit.IptalTarihi = DateTime.Now;
        kayit.GuncellemeTarihi = DateTime.Now;

        if (!await DegisiklikleriKaydet())
            return RedirectToAction(nameof(Details), new { id });

        TempData["Basari"] = "Mesai kaydı iptal edildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private IQueryable<PersonelMesaiKaydi> MesaiSorgusu()
    {
        return _context.PersonelMesaiKayitlari
            .AsNoTracking()
            .Include(x => x.Personel)
            .Include(x => x.Kullanici)
            .Include(x => x.OnaylayanKullanici)
            .AsQueryable();
    }

    private static IQueryable<PersonelMesaiKaydi> FiltreleriUygula(
        IQueryable<PersonelMesaiKaydi> query,
        string? arama,
        string durum,
        string mesaiTuru,
        int? personelId,
        DateTime? baslangicTarihi,
        DateTime? bitisTarihi)
    {
        if (!string.IsNullOrWhiteSpace(arama))
        {
            var metin = arama.Trim();
            query = query.Where(x =>
                x.Personel.AdSoyad.Contains(metin) ||
                x.Personel.Pozisyon.Contains(metin) ||
                x.MesaiTuru.Contains(metin) ||
                x.Kullanici.KullaniciAdi.Contains(metin) ||
                (x.Aciklama != null && x.Aciklama.Contains(metin)));
        }

        if (GecerliDurumlar.Contains(durum))
            query = query.Where(x => x.Durum == durum);

        if (PersonelMesaiKaydi.MesaiTurleri.Contains(mesaiTuru))
            query = query.Where(x => x.MesaiTuru == mesaiTuru);

        if (personelId.HasValue)
            query = query.Where(x => x.PersonelId == personelId.Value);

        if (baslangicTarihi.HasValue)
            query = query.Where(x => x.Tarih >= baslangicTarihi.Value.Date);

        if (bitisTarihi.HasValue)
            query = query.Where(x => x.Tarih <= bitisTarihi.Value.Date);

        return query;
    }

    private static async Task<PersonelMesaiListeViewModel> Sayfala(
        IQueryable<PersonelMesaiKaydi> query,
        string? arama,
        string durum,
        string mesaiTuru,
        int? personelId,
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
            .OrderByDescending(x => x.Tarih)
            .ThenBy(x => x.VardiyaBaslangic)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PersonelMesaiListeViewModel
        {
            Arama = arama,
            Durum = GecerliDurumlar.Contains(durum) ? durum : "Tumu",
            MesaiTuru = PersonelMesaiKaydi.MesaiTurleri.Contains(mesaiTuru)
                ? mesaiTuru
                : "Tumu",
            PersonelId = personelId,
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items
        };
    }

    private async Task<PersonelMesaiOzetViewModel> OzetGetir(
        int? personelId,
        bool sadeceOnayli)
    {
        var ayBaslangici = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var sonrakiAy = ayBaslangici.AddMonths(1);
        var query = _context.PersonelMesaiKayitlari.AsNoTracking();

        if (personelId.HasValue)
            query = query.Where(x => x.PersonelId == personelId.Value);

        if (sadeceOnayli)
            query = query.Where(x => x.Durum == PersonelMesaiKaydi.DurumOnaylandi);

        var aylik = query.Where(x => x.Tarih >= ayBaslangici && x.Tarih < sonrakiAy);

        return new PersonelMesaiOzetViewModel
        {
            ToplamKayit = await query.CountAsync(),
            OnayBekleyenSayisi = await query.CountAsync(
                x => x.Durum == PersonelMesaiKaydi.DurumOnayBekliyor),
            BuAyToplamPlanlananSaat = await aylik
                .Where(x => x.Durum == PersonelMesaiKaydi.DurumOnaylandi)
                .SumAsync(x => (decimal?)x.PlanlananSaat) ?? 0m,
            BuAyToplamGerceklesenSaat = await aylik
                .Where(x => x.Durum == PersonelMesaiKaydi.DurumOnaylandi)
                .SumAsync(x => (decimal?)x.GerceklesenSaat) ?? 0m,
            BuAyToplamFazlaMesai = await aylik
                .Where(x => PersonelMesaiKaydi.FazlaMesaiKapsamindakiTurler.Contains(x.MesaiTuru))
                .SumAsync(x => (decimal?)(
                    x.FazlaMesaiSaati > 0
                        ? x.FazlaMesaiSaati
                        : x.GerceklesenSaat > 0
                            ? x.GerceklesenSaat
                            : x.PlanlananSaat)) ?? 0m,
            OnayliFazlaMesaiSaati = await aylik
                .Where(x =>
                    x.Durum == PersonelMesaiKaydi.DurumOnaylandi &&
                    PersonelMesaiKaydi.FazlaMesaiKapsamindakiTurler.Contains(x.MesaiTuru))
                .SumAsync(x => (decimal?)(
                    x.FazlaMesaiSaati > 0
                        ? x.FazlaMesaiSaati
                        : x.GerceklesenSaat > 0
                            ? x.GerceklesenSaat
                            : x.PlanlananSaat)) ?? 0m
        };
    }

    private async Task<bool> MesaiCakisiyorMu(
        int personelId,
        DateTime tarih,
        TimeSpan baslangic,
        TimeSpan bitis,
        int? haricKayitId = null)
    {
        var kayitlar = await _context.PersonelMesaiKayitlari
            .AsNoTracking()
            .Where(x =>
                x.PersonelId == personelId &&
                x.Tarih == tarih.Date &&
                (!haricKayitId.HasValue || x.Id != haricKayitId.Value) &&
                (x.Durum == PersonelMesaiKaydi.DurumOnayBekliyor ||
                 x.Durum == PersonelMesaiKaydi.DurumOnaylandi))
            .Select(x => new { x.VardiyaBaslangic, x.VardiyaBitis })
            .ToListAsync();

        var yeniAralik = NormalizeAralik(baslangic, bitis);
        return kayitlar.Any(x =>
        {
            var mevcutAralik = NormalizeAralik(x.VardiyaBaslangic, x.VardiyaBitis);
            return yeniAralik.Baslangic < mevcutAralik.Bitis &&
                   yeniAralik.Bitis > mevcutAralik.Baslangic;
        });
    }

    private static (double Baslangic, double Bitis) NormalizeAralik(
        TimeSpan baslangic,
        TimeSpan bitis)
    {
        var baslangicSaat = baslangic.TotalHours;
        var bitisSaat = bitis.TotalHours;
        if (bitisSaat < baslangicSaat)
            bitisSaat += 24;

        return (baslangicSaat, bitisSaat);
    }

    private static decimal SureHesapla(TimeSpan baslangic, TimeSpan bitis)
    {
        var sure = bitis - baslangic;
        if (sure < TimeSpan.Zero)
            sure = sure.Add(TimeSpan.FromDays(1));

        return Math.Round(
            (decimal)sure.TotalHours,
            2,
            MidpointRounding.AwayFromZero);
    }

    private static decimal FazlaMesaiHesapla(
        string mesaiTuru,
        decimal planlananSaat,
        decimal gerceklesenSaat)
    {
        if (PersonelMesaiKaydi.FazlaMesaiKapsamindaMi(mesaiTuru))
            return gerceklesenSaat > 0 ? gerceklesenSaat : planlananSaat;

        return Math.Max(0m, gerceklesenSaat - planlananSaat);
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

    private bool MesaileriYonetebilirMi()
    {
        return User.IsInRole("Admin") ||
               User.IsInRole("Yonetici") ||
               User.IsInRole("InsanKaynaklari");
    }

    private async Task CreateVerileriniDoldur(
        Kullanici kullanici,
        int? seciliPersonelId)
    {
        var yonetebilir = MesaileriYonetebilirMi();
        ViewData["YoneticiModu"] = yonetebilir;
        ViewData["KendiPersonelAdi"] =
            kullanici.Personel?.AdSoyad ?? "Bağlı personel kaydı bulunamadı";
        ViewData["KendiPersonelDetayi"] = kullanici.Personel is null
            ? null
            : $"{kullanici.Personel.Pozisyon} / {kullanici.Personel.Departman}";
        ViewData["MesaiTurleri"] = yonetebilir
            ? new SelectList(PersonelMesaiKaydi.MesaiTurleri)
            : new SelectList(PersonelMesaiKaydi.PersonelTalepTurleri);

        if (yonetebilir)
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
        }
    }

    private async Task<List<SelectListItem>> AktifPersonelSecenekleriniGetir()
    {
        return await _context.Personeller
            .AsNoTracking()
            .Where(x => x.AktifMi)
            .OrderBy(x => x.AdSoyad)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.AdSoyad + " - " + x.Pozisyon
            })
            .ToListAsync();
    }

    private bool RowVersionAyarla(PersonelMesaiKaydi kayit, string rowVersion)
    {
        try
        {
            _context.Entry(kayit)
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
                "Mesai kaydı başka bir kullanıcı tarafından güncellendi. Sayfayı yenileyip tekrar deneyiniz.";
            return false;
        }
    }
}
