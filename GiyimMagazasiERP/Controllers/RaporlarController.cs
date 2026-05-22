using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class RaporlarController : Controller
{
    private readonly AppDbContext _context;

    public RaporlarController(AppDbContext context)
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

        var baslangicSermayesi = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x =>
                x.HareketTipi == "Gelir" &&
                x.Kategori == "Baslangic Sermayesi")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var gunlukSatisOzetleri = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => x.SatisTarihi.Date)
            .Select(grup => new GunlukSatisOzetiViewModel
            {
                Gun = grup.Key,
                SatisSayisi = grup.Count(),
                ToplamNetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Gun)
            .ToListAsync();

        var aylikSatisOzetleri = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.SatisTarihi.Year,
                x.SatisTarihi.Month
            })
            .Select(grup => new AylikSatisOzetiViewModel
            {
                Yil = grup.Key.Year,
                Ay = grup.Key.Month,
                SatisSayisi = grup.Count(),
                ToplamSatisTutari = grup.Sum(x => x.ToplamTutar),
                ToplamIndirim = grup.Sum(x => x.IndirimTutari),
                NetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Yil)
            .ThenByDescending(x => x.Ay)
            .ToListAsync();

        var yillikSatisOzetleri = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => x.SatisTarihi.Year)
            .Select(grup => new YillikSatisOzetiViewModel
            {
                Yil = grup.Key,
                SatisSayisi = grup.Count(),
                ToplamSatisTutari = grup.Sum(x => x.ToplamTutar),
                ToplamIndirim = grup.Sum(x => x.IndirimTutari),
                NetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Yil)
            .ToListAsync();

        var aylikGelirGiderRaporu = await _context.FinansHareketleri
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.Tarih.Year,
                x.Tarih.Month
            })
            .Select(grup => new AylikGelirGiderRaporuViewModel
            {
                Yil = grup.Key.Year,
                Ay = grup.Key.Month,
                ToplamGelir = grup
                    .Where(x => x.HareketTipi == "Gelir")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                ToplamGider = grup
                    .Where(x => x.HareketTipi == "Gider")
                    .Sum(x => (decimal?)x.Tutar) ?? 0
            })
            .OrderByDescending(x => x.Yil)
            .ThenByDescending(x => x.Ay)
            .ToListAsync();

        var yillikGelirGiderRaporu = await _context.FinansHareketleri
            .AsNoTracking()
            .GroupBy(x => x.Tarih.Year)
            .Select(grup => new YillikGelirGiderRaporuViewModel
            {
                Yil = grup.Key,
                ToplamGelir = grup
                    .Where(x => x.HareketTipi == "Gelir")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                ToplamGider = grup
                    .Where(x => x.HareketTipi == "Gider")
                    .Sum(x => (decimal?)x.Tutar) ?? 0
            })
            .OrderByDescending(x => x.Yil)
            .ToListAsync();

        var giderKategorileriRaporu = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .GroupBy(x => x.Kategori)
            .Select(grup => new GiderKategorisiRaporuViewModel
            {
                GiderKategorisi = grup.Key,
                ToplamTutar = grup.Sum(x => x.Tutar),
                HareketSayisi = grup.Count()
            })
            .OrderByDescending(x => x.ToplamTutar)
            .ThenBy(x => x.GiderKategorisi)
            .ToListAsync();

        var enYuksekGiderler = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .OrderByDescending(x => x.Tutar)
            .ThenByDescending(x => x.Tarih)
            .Select(x => new EnYuksekGiderViewModel
            {
                Tarih = x.Tarih,
                Kategori = x.Kategori,
                Aciklama = x.Aciklama,
                Tutar = x.Tutar
            })
            .Take(10)
            .ToListAsync();

        var enCokSatilanUrunler = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.UrunId,
                x.Urun.UrunAdi
            })
            .Select(grup => new EnCokSatilanUrunViewModel
            {
                UrunAdi = grup.Key.UrunAdi,
                ToplamSatilanAdet = grup.Sum(x => x.Adet),
                ToplamSatisTutari = grup.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.ToplamSatilanAdet)
            .ThenByDescending(x => x.ToplamSatisTutari)
            .Take(10)
            .ToListAsync();

        var hicSatilmayanUrunler = await _context.Urunler
            .AsNoTracking()
            .Where(x => !x.SatisDetaylari.Any())
            .Select(x => new HicSatilmayanUrunViewModel
            {
                UrunAdi = x.UrunAdi,
                Barkod = x.Barkod,
                StokMiktari = x.StokMiktari
            })
            .OrderBy(x => x.UrunAdi)
            .ToListAsync();

        var kritikStokRaporu = await _context.Urunler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Where(x => x.AktifMi && x.StokMiktari <= x.MinimumStok)
            .Select(x => new KritikStokRaporuViewModel
            {
                UrunAdi = x.UrunAdi,
                StokMiktari = x.StokMiktari,
                MinimumStok = x.MinimumStok,
                KategoriAdi = x.Kategori.KategoriAdi
            })
            .OrderBy(x => x.StokMiktari)
            .ToListAsync();

        var enCokAlisverisYapanMusteriler = await _context.Musteriler
            .AsNoTracking()
            .OrderByDescending(x => x.ToplamHarcama)
            .Select(x => new EnCokAlisverisYapanMusteriViewModel
            {
                MusteriAdi = x.AdSoyad,
                ToplamHarcama = x.ToplamHarcama,
                SadakatPuani = x.SadakatPuani,
                IndirimOrani = x.IndirimOrani
            })
            .Take(10)
            .ToListAsync();

        var musteriUrunAnalizi = await _context.SatisDetaylari
            .AsNoTracking()
            .Include(x => x.Satis)
                .ThenInclude(x => x.Musteri)
            .Include(x => x.Urun)
            .Where(x => x.Satis.MusteriId != null)
            .GroupBy(x => new
            {
                MusteriId = x.Satis.MusteriId,
                MusteriAdi = x.Satis.Musteri!.AdSoyad,
                x.UrunId,
                x.Urun.UrunAdi
            })
            .Select(grup => new MusteriUrunAnaliziViewModel
            {
                MusteriAdi = grup.Key.MusteriAdi,
                UrunAdi = grup.Key.UrunAdi,
                ToplamAdet = grup.Sum(x => x.Adet)
            })
            .OrderByDescending(x => x.ToplamAdet)
            .ThenBy(x => x.MusteriAdi)
            .Take(10)
            .ToListAsync();

        var personelSatisPerformansi = await _context.Personeller
            .AsNoTracking()
            .Select(x => new PersonelSatisPerformansiViewModel
            {
                PersonelAdi = x.AdSoyad,
                SatisSayisi = x.Satislar.Count(),
                ToplamSatisTutari = x.Satislar.Sum(s => (decimal?)s.NetTutar) ?? 0,
                PrimOrani = x.PrimOrani
            })
            .OrderByDescending(x => x.ToplamSatisTutari)
            .ToListAsync();

        var tedarikciUrunIndirimRaporu = await _context.Tedarikciler
            .AsNoTracking()
            .Select(x => new TedarikciUrunIndirimRaporuViewModel
            {
                FirmaAdi = x.FirmaAdi,
                UrunSayisi = x.Urunler.Count(),
                IndirimOrani = x.IndirimOrani
            })
            .OrderByDescending(x => x.UrunSayisi)
            .ThenBy(x => x.FirmaAdi)
            .ToListAsync();

        var kategoriBazliSatisRaporu = await _context.SatisDetaylari
            .AsNoTracking()
            .Include(x => x.Urun)
                .ThenInclude(x => x.Kategori)
            .GroupBy(x => new
            {
                x.Urun.KategoriId,
                x.Urun.Kategori.KategoriAdi
            })
            .Select(grup => new KategoriBazliSatisRaporuViewModel
            {
                KategoriAdi = grup.Key.KategoriAdi,
                ToplamSatilanAdet = grup.Sum(x => x.Adet),
                ToplamSatisTutari = grup.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.ToplamSatisTutari)
            .ToListAsync();

        var viewModel = new RaporlarIndexViewModel
        {
            GenelFinansOzeti = new GenelFinansOzetiViewModel
            {
                ToplamGelir = toplamGelir,
                ToplamGider = toplamGider,
                NetKazanc = toplamGelir - toplamGider,
                BaslangicSermayesi = baslangicSermayesi
            },

            GunlukSatisOzetleri = gunlukSatisOzetleri,
            AylikSatisOzetleri = aylikSatisOzetleri,
            YillikSatisOzetleri = yillikSatisOzetleri,
            AylikGelirGiderRaporu = aylikGelirGiderRaporu,
            YillikGelirGiderRaporu = yillikGelirGiderRaporu,
            GiderKategorileriRaporu = giderKategorileriRaporu,
            EnYuksekGiderler = enYuksekGiderler,

            EnCokSatilanUrunler = enCokSatilanUrunler,
            HicSatilmayanUrunler = hicSatilmayanUrunler,
            KritikStokRaporu = kritikStokRaporu,
            EnCokAlisverisYapanMusteriler = enCokAlisverisYapanMusteriler,
            MusteriUrunAnalizi = musteriUrunAnalizi,
            PersonelSatisPerformansi = personelSatisPerformansi,
            TedarikciUrunIndirimRaporu = tedarikciUrunIndirimRaporu,
            KategoriBazliSatisRaporu = kategoriBazliSatisRaporu
        };

        return View(viewModel);
    }
}