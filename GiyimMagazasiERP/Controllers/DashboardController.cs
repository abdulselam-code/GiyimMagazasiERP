using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer,Depo,Muhasebe,InsanKaynaklari,Personel")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var bugun = DateTime.Today;
        var yarin = bugun.AddDays(1);
        var ayBaslangici = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        var toplamGelir = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gelir")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var toplamGider = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var model = new DashboardViewModel
        {
            KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
            Rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "Personel",

            ToplamUrunCesidi = await _context.Urunler.CountAsync(),

            ToplamStokAdedi = await _context.Urunler
                .SumAsync(x => (int?)x.StokMiktari) ?? 0,

            ToplamMusteri = await _context.Musteriler.CountAsync(),
            ToplamPersonel = await _context.Personeller.CountAsync(),
            AktifPersonel = await _context.Personeller.CountAsync(x => x.AktifMi),
            ToplamSatis = await _context.Satislar.CountAsync(),

            KritikStokSayisi = await _context.Urunler
                .CountAsync(x => x.AktifMi && x.StokMiktari <= x.MinimumStok),

            ToplamGelir = toplamGelir,
            ToplamGider = toplamGider,
            NetKazanc = toplamGelir - toplamGider,

            BugunkuSatisSayisi = await _context.Satislar
                .CountAsync(x => x.SatisTarihi >= bugun && x.SatisTarihi < yarin),

            BugunkuSatisGeliri = await _context.Satislar
                .Where(x => x.SatisTarihi >= bugun && x.SatisTarihi < yarin)
                .SumAsync(x => (decimal?)x.NetTutar) ?? 0,

            BugunkuGelir = await _context.FinansHareketleri
                .Where(x => x.HareketTipi == "Gelir" && x.Tarih >= bugun && x.Tarih < yarin)
                .SumAsync(x => (decimal?)x.Tutar) ?? 0,

            AylikGelir = await _context.FinansHareketleri
                .Where(x => x.HareketTipi == "Gelir" && x.Tarih >= ayBaslangici)
                .SumAsync(x => (decimal?)x.Tutar) ?? 0,

            AylikGider = await _context.FinansHareketleri
                .Where(x => x.HareketTipi == "Gider" && x.Tarih >= ayBaslangici)
                .SumAsync(x => (decimal?)x.Tutar) ?? 0,

            OrtalamaMaas = await _context.Personeller
                .Where(x => x.AktifMi)
                .AverageAsync(x => (decimal?)x.Maas) ?? 0
        };

        model.SonSatislar = await _context.Satislar
            .AsNoTracking()
            .Include(x => x.Musteri)
            .OrderByDescending(x => x.SatisTarihi)
            .Take(6)
            .Select(x => new DashboardSonSatisViewModel
            {
                SatisId = x.Id,
                SatisTarihi = x.SatisTarihi,
                MusteriAdi = x.Musteri != null ? x.Musteri.AdSoyad : "Kayıtsız Müşteri",
                NetTutar = x.NetTutar,
                OdemeTipi = x.OdemeTipi
            })
            .ToListAsync();

        model.KritikStokUrunleri = await _context.Urunler
            .AsNoTracking()
            .Where(x => x.AktifMi && x.StokMiktari <= x.MinimumStok)
            .OrderBy(x => x.StokMiktari)
            .Take(8)
            .Select(x => new DashboardKritikStokViewModel
            {
                UrunAdi = x.UrunAdi,
                StokMiktari = x.StokMiktari,
                MinimumStok = x.MinimumStok
            })
            .ToListAsync();

        model.SonStokHareketleri = await _context.StokHareketleri
            .AsNoTracking()
            .Include(x => x.Urun)
            .OrderByDescending(x => x.Tarih)
            .Take(8)
            .Select(x => new DashboardStokHareketiViewModel
            {
                Tarih = x.Tarih,
                UrunAdi = x.Urun.UrunAdi,
                HareketTipi = x.HareketTipi,
                Miktar = x.Miktar
            })
            .ToListAsync();

        model.EnYuksekGiderler = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .OrderByDescending(x => x.Tutar)
            .Take(8)
            .Select(x => new DashboardGiderViewModel
            {
                Tarih = x.Tarih,
                Kategori = x.Kategori,
                Aciklama = x.Aciklama,
                Tutar = x.Tutar
            })
            .ToListAsync();

        model.PersonelOzeti = await _context.Personeller
            .AsNoTracking()
            .OrderBy(x => x.AdSoyad)
            .Take(8)
            .Select(x => new DashboardPersonelOzetViewModel
            {
                AdSoyad = x.AdSoyad,
                Pozisyon = x.Pozisyon,
                Departman = x.Departman,
                Maas = x.Maas
            })
            .ToListAsync();

        model.HizliIslemler = HizliIslemleriGetir();

        return View(model);
    }

    private List<DashboardQuickActionViewModel> HizliIslemleriGetir()
    {
        if (User.IsInRole("Admin"))
        {
            return new()
            {
                Link("Satış Yap", "SatisIslemleri", "Create", "success"),
                Link("Ürünleri Yönet", "Urunler"),
                Link("Müşteriler", "Musteriler"),
                Link("Finans Hareketleri", "FinansHareketleri", "Index", "warning"),
                Link("Raporlar", "Raporlar", "Index", "info"),
                Link("SQL Panel", "SqlYonetici", "Index", "dark"),
                Link("DB Panel", "VeritabaniYonetici", "Index", "dark")
            };
        }

        if (User.IsInRole("Yonetici"))
        {
            return new()
            {
                Link("Satış Yap", "SatisIslemleri", "Create", "success"),
                Link("Ürünleri Yönet", "Urunler"),
                Link("Müşteriler", "Musteriler"),
                Link("Personeller", "Personeller"),
                Link("Raporlar", "Raporlar", "Index", "info"),
                Link("Faturalar", "Faturalar")
            };
        }

        if (User.IsInRole("Kasiyer"))
        {
            return new()
            {
                Link("Satış Yap", "SatisIslemleri", "Create", "success"),
                Link("Satışlar", "Satislar"),
                Link("Müşteriler", "Musteriler"),
                Link("Faturalar", "Faturalar")
            };
        }

        if (User.IsInRole("Depo"))
        {
            return new()
            {
                Link("Ürünler", "Urunler"),
                Link("Kategoriler", "Kategoriler"),
                Link("Tedarikçiler", "Tedarikciler"),
                Link("Stok Hareketleri", "StokHareketleri", "Index", "warning")
            };
        }

        if (User.IsInRole("Muhasebe"))
        {
            return new()
            {
                Link("Finans Hareketleri", "FinansHareketleri", "Index", "warning"),
                Link("Satışlar", "Satislar"),
                Link("Faturalar", "Faturalar"),
                Link("Raporlar", "Raporlar", "Index", "info")
            };
        }

        if (User.IsInRole("InsanKaynaklari"))
        {
            return new()
            {
                Link("Personeller", "Personeller")
            };
        }

        return new();
    }

    private static DashboardQuickActionViewModel Link(
        string baslik,
        string controller,
        string action = "Index",
        string stil = "primary")
    {
        return new DashboardQuickActionViewModel
        {
            Baslik = baslik,
            Controller = controller,
            Action = action,
            Stil = stil
        };
    }
}