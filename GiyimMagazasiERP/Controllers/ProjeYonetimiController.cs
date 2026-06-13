using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,InsanKaynaklari,Muhasebe")]
public class ProjeYonetimiController : Controller
{
    private const string AnaProjeAdi = "Giyim Mağazası ERP";
    private readonly AppDbContext _context;

    public ProjeYonetimiController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var proje = await AnaProjeyiGetir();
        if (proje is null)
            return View(new ProjeYonetimiDashboardViewModel());

        var gorevler = await GorevSorgusu(proje.Id).ToListAsync();
        var butce = await _context.ProjeButceKalemleri.AsNoTracking()
            .Where(x => x.ProjeId == proje.Id).ToListAsync();
        var toplamEfor = gorevler.Sum(x => x.PlanlananSaat);
        var tamamlananEfor = gorevler.Sum(x =>
            x.PlanlananSaat * Math.Clamp(x.TamamlanmaYuzdesi, 0, 100) / 100m);
        var gider = butce.Where(x => x.KalemTuru == "Gider")
            .Sum(x => x.GerceklesenTutar);
        var gelir = butce.Where(x => x.KalemTuru == "Gelir")
            .Sum(x => x.GerceklesenTutar);

        return View(new ProjeYonetimiDashboardViewModel
        {
            Proje = proje,
            ToplamGorev = gorevler.Count,
            TamamlananGorev = gorevler.Count(x => x.Durum == "Tamamlandı"),
            DevamEdenGorev = gorevler.Count(x =>
                x.Durum == "Devam Ediyor" || x.Durum == "Test Ediliyor"),
            KalanGorev = gorevler.Count(x => x.TamamlanmaYuzdesi < 100),
            KritikGorev = gorevler.Count(x =>
                x.Oncelik == "Kritik" && x.TamamlanmaYuzdesi < 100),
            TamamlanmaOrani = toplamEfor > 0
                ? Math.Round(tamamlananEfor / toplamEfor * 100m, 2)
                : 0,
            PlanlananButce = proje.PlanlananButce,
            GerceklesenGider = gider,
            NetButce = gelir - gider,
            SonGorevler = gorevler.OrderByDescending(x => x.BitisTarihi)
                .Take(8).ToList()
        });
    }

    public async Task<IActionResult> EkipRaporu()
    {
        var proje = await AnaProjeyiGetir();
        var model = proje is null
            ? new List<ProjeEkipRaporSatiriViewModel>()
            : await EkipRaporunuGetir(proje.Id);
        return View(model);
    }

    public async Task<IActionResult> GanttGorev()
    {
        var proje = await AnaProjeyiGetir();
        if (proje is null)
            return View("Gantt", new ProjeGanttViewModel { Baslik = "Göreve Göre Gantt" });

        var gorevler = await GorevSorgusu(proje.Id)
            .OrderBy(x => x.BaslangicTarihi).ThenBy(x => x.BitisTarihi)
            .ToListAsync();
        return View("Gantt", GanttOlustur(
            "Göreve Göre Gantt Şeması",
            proje,
            gorevler.Select(x => new ProjeGanttSatirViewModel
            {
                Baslik = x.GorevAdi,
                AltBaslik = $"{x.ModulAdi} · {x.SorumluEkipUyesi?.AdSoyad ?? "Atanmadı"}",
                Baslangic = x.BaslangicTarihi,
                Bitis = x.BitisTarihi,
                Durum = x.Durum,
                Oncelik = x.Oncelik,
                TamamlanmaYuzdesi = Math.Clamp(x.TamamlanmaYuzdesi, 0, 100)
            }).ToList()));
    }

    public async Task<IActionResult> GanttEkip()
    {
        var proje = await AnaProjeyiGetir();
        if (proje is null)
            return View("Gantt", new ProjeGanttViewModel { Baslik = "Ekibe Göre Gantt" });

        var ekip = await _context.ProjeEkipUyeleri.AsNoTracking()
            .Include(x => x.Gorevler)
            .Where(x => x.ProjeId == proje.Id && x.AktifMi)
            .OrderBy(x => x.AdSoyad).ToListAsync();
        var satirlar = ekip.Where(x => x.Gorevler.Count > 0)
            .Select(x => new ProjeGanttSatirViewModel
            {
                Baslik = x.AdSoyad,
                AltBaslik = $"{x.Rol} · {x.Gorevler.Count} görev",
                Baslangic = x.Gorevler.Min(g => g.BaslangicTarihi),
                Bitis = x.Gorevler.Max(g => g.BitisTarihi),
                Durum = x.Gorevler.All(g => g.Durum == "Tamamlandı")
                    ? "Tamamlandı"
                    : x.Gorevler.Any(g => g.Durum == "Test Ediliyor")
                        ? "Test Ediliyor"
                        : "Devam Ediyor",
                Oncelik = x.Gorevler.Any(g => g.Oncelik == "Kritik")
                    ? "Kritik"
                    : "Normal",
                TamamlanmaYuzdesi = (int)Math.Round(
                    x.Gorevler.Average(g => Math.Clamp(g.TamamlanmaYuzdesi, 0, 100)))
            }).ToList();
        return View("Gantt", GanttOlustur("Ekip Üyesine Göre Gantt Şeması", proje, satirlar));
    }

    public async Task<IActionResult> KritikYol()
    {
        var proje = await AnaProjeyiGetir();
        if (proje is null)
            return View(new ProjeKritikYolViewModel());

        var gorevler = await GorevSorgusu(proje.Id).ToListAsync();
        var baglar = await _context.ProjeGorevBagimliliklari.AsNoTracking()
            .Where(x => gorevler.Select(g => g.Id).Contains(x.GorevId))
            .ToListAsync();
        return View(KritikYoluHesapla(proje, gorevler, baglar));
    }

    public async Task<IActionResult> Raporlar()
    {
        var proje = await AnaProjeyiGetir();
        if (proje is null)
            return View(new ProjeRaporViewModel());

        var gorevler = await GorevSorgusu(proje.Id).ToListAsync();
        var butce = await _context.ProjeButceKalemleri.AsNoTracking()
            .Where(x => x.ProjeId == proje.Id).OrderBy(x => x.Kategori)
            .ToListAsync();
        var gider = butce.Where(x => x.KalemTuru == "Gider")
            .Sum(x => x.GerceklesenTutar);
        var gelir = butce.Where(x => x.KalemTuru == "Gelir")
            .Sum(x => x.GerceklesenTutar);

        return View(new ProjeRaporViewModel
        {
            ToplamGorev = gorevler.Count,
            Tamamlanan = gorevler.Count(x => x.Durum == "Tamamlandı"),
            DevamEden = gorevler.Count(x => x.Durum == "Devam Ediyor"),
            Testte = gorevler.Count(x => x.Durum == "Test Ediliyor"),
            Geciken = gorevler.Count(x =>
                x.BitisTarihi.Date < DateTime.Today && x.TamamlanmaYuzdesi < 100),
            PlanlananButce = proje.PlanlananButce,
            GerceklesenGider = gider,
            KalanButce = proje.PlanlananButce - gider,
            NetButce = gelir - gider,
            EkipRaporu = await EkipRaporunuGetir(proje.Id),
            ModulRaporu = gorevler.GroupBy(x => x.ModulAdi)
                .Select(g => new ProjeModulRaporSatiriViewModel
                {
                    ModulAdi = g.Key,
                    GorevSayisi = g.Count(),
                    TamamlanmaOrani = Math.Round(
                        g.Average(x => (decimal)x.TamamlanmaYuzdesi),
                        2),
                    TestDurumu = g.All(x => x.Durum == "Tamamlandı")
                        ? "Tamamlandı"
                        : g.Any(x => x.Durum == "Test Ediliyor")
                            ? "Test Ediliyor"
                            : "Devam Ediyor"
                }).OrderBy(x => x.ModulAdi).ToList(),
            KritikGorevler = gorevler.Where(x => x.Oncelik == "Kritik").ToList(),
            GecikenGorevler = gorevler.Where(x =>
                x.BitisTarihi.Date < DateTime.Today && x.TamamlanmaYuzdesi < 100).ToList(),
            ButceAsanKalemler = butce.Where(x =>
                x.KalemTuru == "Gider" && x.GerceklesenTutar > x.PlanlananTutar).ToList()
        });
    }

    public async Task<IActionResult> Butce()
    {
        var proje = await AnaProjeyiGetir();
        var kalemler = proje is null
            ? new List<ProjeButceKalemi>()
            : await _context.ProjeButceKalemleri.AsNoTracking()
                .Where(x => x.ProjeId == proje.Id)
                .OrderBy(x => x.KalemTuru).ThenBy(x => x.Kategori).ToListAsync();
        var gelir = kalemler.Where(x => x.KalemTuru == "Gelir").Sum(x => x.GerceklesenTutar);
        var gider = kalemler.Where(x => x.KalemTuru == "Gider").Sum(x => x.GerceklesenTutar);
        var planlananGider = kalemler.Where(x => x.KalemTuru == "Gider").Sum(x => x.PlanlananTutar);
        return View(new ProjeButceViewModel
        {
            ToplamGelir = gelir,
            ToplamGider = gider,
            NetButce = gelir - gider,
            ButceKullanimOrani = planlananGider > 0
                ? Math.Round(gider / planlananGider * 100m, 2)
                : 0,
            Kalemler = kalemler
        });
    }

    public IActionResult VeritabaniDokumani()
    {
        return View();
    }

    private Task<Proje?> AnaProjeyiGetir()
    {
        return _context.Projeler.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProjeAdi == AnaProjeAdi);
    }

    private IQueryable<ProjeGorevi> GorevSorgusu(int projeId)
    {
        return _context.ProjeGorevleri.AsNoTracking()
            .Include(x => x.SorumluEkipUyesi)
            .Where(x => x.ProjeId == projeId);
    }

    private async Task<List<ProjeEkipRaporSatiriViewModel>> EkipRaporunuGetir(int projeId)
    {
        var ekip = await _context.ProjeEkipUyeleri.AsNoTracking()
            .Include(x => x.Gorevler)
            .Where(x => x.ProjeId == projeId && x.AktifMi)
            .OrderBy(x => x.AdSoyad).ToListAsync();
        var toplamSaat = ekip.SelectMany(x => x.Gorevler).Sum(x => x.PlanlananSaat);
        return ekip.Select(x => new ProjeEkipRaporSatiriViewModel
        {
            EkipUyesi = x.AdSoyad,
            Rol = x.Rol,
            AtananGorev = x.Gorevler.Count,
            TamamlananGorev = x.Gorevler.Count(g => g.Durum == "Tamamlandı"),
            DevamEdenGorev = x.Gorevler.Count(g => g.Durum != "Tamamlandı"),
            PlanlananSaat = x.Gorevler.Sum(g => g.PlanlananSaat),
            GerceklesenSaat = x.Gorevler.Sum(g => g.GerceklesenSaat),
            IsYukuYuzdesi = toplamSaat > 0
                ? Math.Round(x.Gorevler.Sum(g => g.PlanlananSaat) / toplamSaat * 100m, 2)
                : 0
        }).ToList();
    }

    private static ProjeGanttViewModel GanttOlustur(
        string baslik,
        Proje proje,
        List<ProjeGanttSatirViewModel> satirlar)
    {
        var baslangic = proje.BaslangicTarihi.Date;
        var bitis = proje.PlanlananBitisTarihi.Date;
        var toplamGun = Math.Max(1, (bitis - baslangic).Days + 1);
        foreach (var satir in satirlar)
        {
            satir.SolYuzde = Math.Round(
                Math.Max(0, (satir.Baslangic.Date - baslangic).Days) / (decimal)toplamGun * 100m, 2);
            satir.GenislikYuzde = Math.Round(
                Math.Max(1, (satir.Bitis.Date - satir.Baslangic.Date).Days + 1) /
                (decimal)toplamGun * 100m, 2);
        }
        return new ProjeGanttViewModel
        {
            Baslik = baslik,
            Baslangic = baslangic,
            Bitis = bitis,
            Satirlar = satirlar
        };
    }

    private static ProjeKritikYolViewModel KritikYoluHesapla(
        Proje proje,
        List<ProjeGorevi> gorevler,
        List<ProjeGorevBagimliligi> baglar)
    {
        if (gorevler.Count == 0)
            return new ProjeKritikYolViewModel();

        var ids = gorevler.Select(x => x.Id).ToHashSet();
        var onculler = ids.ToDictionary(
            id => id,
            id => baglar.Where(x => x.GorevId == id)
                .Select(x => x.BagimliOlduguGorevId).Where(ids.Contains).ToList());
        var ardillar = ids.ToDictionary(
            id => id,
            id => baglar.Where(x => x.BagimliOlduguGorevId == id)
                .Select(x => x.GorevId).Where(ids.Contains).ToList());
        var derece = ids.ToDictionary(id => id, id => onculler[id].Count);
        var kuyruk = new Queue<int>(derece.Where(x => x.Value == 0).Select(x => x.Key));
        var sirali = new List<int>();
        while (kuyruk.Count > 0)
        {
            var id = kuyruk.Dequeue();
            sirali.Add(id);
            foreach (var ardil in ardillar[id])
                if (--derece[ardil] == 0)
                    kuyruk.Enqueue(ardil);
        }
        if (sirali.Count != gorevler.Count)
            sirali = gorevler.OrderBy(x => x.BaslangicTarihi).Select(x => x.Id).ToList();

        var sure = gorevler.ToDictionary(
            x => x.Id,
            x => Math.Max(1m, (decimal)(x.BitisTarihi.Date - x.BaslangicTarihi.Date).Days + 1));
        var erkenBaslangic = ids.ToDictionary(id => id, _ => 0m);
        var erkenBitis = ids.ToDictionary(id => id, _ => 0m);
        foreach (var id in sirali)
        {
            erkenBaslangic[id] = onculler[id].Count == 0
                ? 0
                : onculler[id].Max(x => erkenBitis[x]);
            erkenBitis[id] = erkenBaslangic[id] + sure[id];
        }
        var agSuresi = erkenBitis.Values.Max();
        var gecBitis = ids.ToDictionary(id => id, _ => agSuresi);
        var gecBaslangic = ids.ToDictionary(id => id, _ => 0m);
        foreach (var id in sirali.AsEnumerable().Reverse())
        {
            gecBitis[id] = ardillar[id].Count == 0
                ? agSuresi
                : ardillar[id].Min(x => gecBaslangic[x]);
            gecBaslangic[id] = gecBitis[id] - sure[id];
        }

        var adlar = gorevler.ToDictionary(x => x.Id, x => x.GorevAdi);
        var satirlar = sirali.Select(id => new ProjeKritikYolSatiriViewModel
        {
            GorevId = id,
            GorevAdi = adlar[id],
            PlanlananSureGun = sure[id],
            EnErkenBaslangic = erkenBaslangic[id],
            EnErkenBitis = erkenBitis[id],
            EnGecBaslangic = gecBaslangic[id],
            EnGecBitis = gecBitis[id],
            BollukSuresi = Math.Max(0, gecBaslangic[id] - erkenBaslangic[id]),
            KritikMi = Math.Abs(gecBaslangic[id] - erkenBaslangic[id]) < 0.01m
        }).ToList();
        var kritikler = satirlar.Where(x => x.KritikMi)
            .OrderBy(x => x.EnErkenBaslangic).Select(x => x.GorevAdi);
        return new ProjeKritikYolViewModel
        {
            KritikYolVarMi = satirlar.Any(x => x.KritikMi),
            ProjeBaslangicTarihi = proje.BaslangicTarihi.Date,
            ProjeBitisTarihi = proje.PlanlananBitisTarihi.Date,
            ProjeSuresiGun = Math.Max(
                1,
                (proje.PlanlananBitisTarihi.Date - proje.BaslangicTarihi.Date).Days + 1),
            KritikYolMetni = string.Join(" → ", kritikler),
            Gorevler = satirlar
        };
    }
}
