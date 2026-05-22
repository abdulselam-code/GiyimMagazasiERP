using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var toplamGelir = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gelir")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var toplamGider = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var gunlukSatislar = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => x.SatisTarihi.Date)
            .Select(grup => new
            {
                Tarih = grup.Key,
                ToplamNetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderBy(x => x.Tarih)
            .ToListAsync();

        var kategoriBazliSatislar = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.Urun.KategoriId,
                x.Urun.Kategori.KategoriAdi
            })
            .Select(grup => new
            {
                KategoriAdi = grup.Key.KategoriAdi,
                ToplamSatisTutari = grup.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.ToplamSatisTutari)
            .ToListAsync();

        var enCokSatilanUrunler = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.UrunId,
                x.Urun.UrunAdi
            })
            .Select(grup => new
            {
                UrunAdi = grup.Key.UrunAdi,
                ToplamAdet = grup.Sum(x => x.Adet)
            })
            .OrderByDescending(x => x.ToplamAdet)
            .Take(10)
            .ToListAsync();

        var kritikStokUrunleri = await _context.Urunler
            .AsNoTracking()
            .Where(x => x.AktifMi && x.StokMiktari <= x.MinimumStok)
            .OrderBy(x => x.StokMiktari)
            .Take(10)
            .Select(x => new
            {
                x.UrunAdi,
                x.StokMiktari
            })
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            ToplamUrunSayisi = await _context.Urunler.CountAsync(),
            ToplamMusteriSayisi = await _context.Musteriler.CountAsync(),
            ToplamPersonelSayisi = await _context.Personeller.CountAsync(),
            ToplamSatisSayisi = await _context.Satislar.CountAsync(),
            ToplamGelir = toplamGelir,
            ToplamGider = toplamGider,
            KritikStokSayisi = await _context.Urunler
                .CountAsync(x => x.AktifMi && x.StokMiktari <= x.MinimumStok),

            GunlukSatisEtiketleri = gunlukSatislar
                .Select(x => x.Tarih.ToString("dd/MM/yyyy"))
                .ToList(),

            GunlukSatisTutarlar = gunlukSatislar
                .Select(x => x.ToplamNetSatis)
                .ToList(),

            GelirGiderEtiketleri = new List<string>
            {
                "Toplam Gelir",
                "Toplam Gider"
            },

            GelirGiderTutarlar = new List<decimal>
            {
                toplamGelir,
                toplamGider
            },

            KategoriSatisEtiketleri = kategoriBazliSatislar
                .Select(x => x.KategoriAdi)
                .ToList(),

            KategoriSatisTutarlar = kategoriBazliSatislar
                .Select(x => x.ToplamSatisTutari)
                .ToList(),

            EnCokSatilanUrunEtiketleri = enCokSatilanUrunler
                .Select(x => x.UrunAdi)
                .ToList(),

            EnCokSatilanUrunAdetleri = enCokSatilanUrunler
                .Select(x => x.ToplamAdet)
                .ToList(),

            KritikStokEtiketleri = kritikStokUrunleri
                .Select(x => x.UrunAdi)
                .ToList(),

            KritikStokMiktarlari = kritikStokUrunleri
                .Select(x => x.StokMiktari)
                .ToList()
        };

        return View(viewModel);
    }
}