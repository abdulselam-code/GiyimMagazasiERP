using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Muhasebe")]
public class RaporlarController : Controller
{
    private readonly AppDbContext _context;

    public RaporlarController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? baslangic, DateTime? bitis, string? donem)
    {
        donem = string.IsNullOrWhiteSpace(donem) ? "BuAy" : donem;

        var tarihAraligi = TarihAraliginiGetir(baslangic, bitis, donem);
        var baslangicTarihi = tarihAraligi.Baslangic;
        var bitisTarihi = tarihAraligi.Bitis;
        var bitisExclusive = bitisTarihi.Date.AddDays(1);

        var donemSatislari = _context.Satislar
            .AsNoTracking()
            .Where(x => x.SatisTarihi >= baslangicTarihi && x.SatisTarihi < bitisExclusive);

        var donemFinansHareketleri = _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.Tarih >= baslangicTarihi && x.Tarih < bitisExclusive);

        // Genel finans özeti tüm zamanı gösterir.
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

        var donemToplamSatisSayisi = await donemSatislari.CountAsync();

        var donemToplamSatisTutari = await donemSatislari
            .SumAsync(x => (decimal?)x.ToplamTutar) ?? 0;

        var donemToplamIndirim = await donemSatislari
            .SumAsync(x => (decimal?)x.IndirimTutari) ?? 0;

        var donemToplamNetSatis = await donemSatislari
            .SumAsync(x => (decimal?)x.NetTutar) ?? 0;

        var donemOrtalamaSatisTutari = donemToplamSatisSayisi > 0
            ? donemToplamNetSatis / donemToplamSatisSayisi
            : 0;

        var donemGelir = await donemFinansHareketleri
            .Where(x => x.HareketTipi == "Gelir")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var donemGider = await donemFinansHareketleri
            .Where(x => x.HareketTipi == "Gider")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var gunlukSatisOzetleri = await donemSatislari
            .GroupBy(x => x.SatisTarihi.Date)
            .Select(grup => new GunlukSatisOzetiViewModel
            {
                Gun = grup.Key,
                SatisSayisi = grup.Count(),
                ToplamNetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Gun)
            .ToListAsync();

        var aylikSatisOzetleri = await donemSatislari
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

        var yillikSatisOzetleri = await donemSatislari
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

        var aylikGelirGiderRaporu = await donemFinansHareketleri
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

        var yillikGelirGiderRaporu = await donemFinansHareketleri
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

        var giderKategorileriRaporu = await donemFinansHareketleri
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

        var enYuksekGiderler = await donemFinansHareketleri
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
            .Where(x => x.Satis.SatisTarihi >= baslangicTarihi && x.Satis.SatisTarihi < bitisExclusive)
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

        // Güncel durum raporu olduğu için tarih filtresi uygulanmıyor.
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

        // Güncel durum raporu olduğu için tarih filtresi uygulanmıyor.
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

        var enCokAlisverisYapanMusteriler = await donemSatislari
            .Where(x => x.Musteri != null)
            .GroupBy(x => new
            {
                x.MusteriId,
                x.Musteri!.AdSoyad,
                x.Musteri.ToplamHarcama,
                x.Musteri.SadakatPuani,
                x.Musteri.IndirimOrani
            })
            .Select(grup => new EnCokAlisverisYapanMusteriViewModel
            {
                MusteriAdi = grup.Key.AdSoyad,
                ToplamHarcama = grup.Sum(x => x.NetTutar),
                SadakatPuani = grup.Key.SadakatPuani,
                IndirimOrani = grup.Key.IndirimOrani
            })
            .OrderByDescending(x => x.ToplamHarcama)
            .Take(10)
            .ToListAsync();

        var musteriUrunAnalizi = await _context.SatisDetaylari
            .AsNoTracking()
            .Include(x => x.Satis)
                .ThenInclude(x => x.Musteri)
            .Include(x => x.Urun)
            .Where(x =>
                x.Satis.MusteriId != null &&
                x.Satis.SatisTarihi >= baslangicTarihi &&
                x.Satis.SatisTarihi < bitisExclusive)
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
                SatisSayisi = x.Satislar.Count(s =>
                    s.SatisTarihi >= baslangicTarihi &&
                    s.SatisTarihi < bitisExclusive),
                ToplamSatisTutari = x.Satislar
                    .Where(s =>
                        s.SatisTarihi >= baslangicTarihi &&
                        s.SatisTarihi < bitisExclusive)
                    .Sum(s => (decimal?)s.NetTutar) ?? 0,
                PrimOrani = x.PrimOrani
            })
            .OrderByDescending(x => x.ToplamSatisTutari)
            .ToListAsync();

        // Güncel durum raporu olduğu için tarih filtresi uygulanmıyor.
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
            .Where(x => x.Satis.SatisTarihi >= baslangicTarihi && x.Satis.SatisTarihi < bitisExclusive)
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

        var odemeTipineGoreGelirler = await donemSatislari
            .GroupBy(x => x.OdemeTipi)
            .Select(grup => new OdemeTipiGelirRaporuViewModel
            {
                OdemeTipi = grup.Key ?? "-",
                SatisSayisi = grup.Count(),
                ToplamGelir = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.ToplamGelir)
            .ToListAsync();

        var viewModel = new RaporlarIndexViewModel
        {
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi,
            Donem = donem,

            DonemToplamSatisSayisi = donemToplamSatisSayisi,
            DonemToplamSatisTutari = donemToplamSatisTutari,
            DonemToplamIndirim = donemToplamIndirim,
            DonemToplamNetSatis = donemToplamNetSatis,
            DonemOrtalamaSatisTutari = donemOrtalamaSatisTutari,
            DonemNetKarZarar = donemGelir - donemGider,

            OdemeTipineGoreGelirler = odemeTipineGoreGelirler,

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

    public async Task<IActionResult> HicSatilmayanUrunler(
        string? arama,
        int sayfa = 1,
        int kayitSayisi = 20)
    {
        var izinliKayitSayilari = new[] { 10, 20, 50 };

        if (!izinliKayitSayilari.Contains(kayitSayisi))
            kayitSayisi = 20;

        if (sayfa < 1)
            sayfa = 1;

        arama = string.IsNullOrWhiteSpace(arama)
            ? null
            : arama.Trim();

        var sorgu = _context.Urunler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Include(x => x.Tedarikci)
            .Where(x => !x.SatisDetaylari.Any());

        if (!string.IsNullOrWhiteSpace(arama))
        {
            sorgu = sorgu.Where(x =>
                x.UrunAdi.Contains(arama) ||
                x.Barkod.Contains(arama) ||
                (x.Kategori != null && x.Kategori.KategoriAdi.Contains(arama)) ||
                (x.Tedarikci != null && x.Tedarikci.FirmaAdi.Contains(arama)));
        }

        var toplamKayit = await sorgu.CountAsync();

        var toplamSayfa = toplamKayit == 0
            ? 1
            : (int)Math.Ceiling(toplamKayit / (double)kayitSayisi);

        if (sayfa > toplamSayfa)
            sayfa = toplamSayfa;

        var urunler = await sorgu
            .OrderBy(x => x.UrunAdi)
            .Skip((sayfa - 1) * kayitSayisi)
            .Take(kayitSayisi)
            .Select(x => new HicSatilmayanUrunDetayViewModel
            {
                UrunAdi = x.UrunAdi,
                Barkod = x.Barkod,
                KategoriAdi = x.Kategori != null ? x.Kategori.KategoriAdi : "-",
                TedarikciAdi = x.Tedarikci != null ? x.Tedarikci.FirmaAdi : "-",
                StokMiktari = x.StokMiktari,
                MinimumStok = x.MinimumStok,
                SatisFiyati = x.SatisFiyati,
                AktifMi = x.AktifMi
            })
            .ToListAsync();

        var viewModel = new HicSatilmayanUrunlerViewModel
        {
            Arama = arama,
            Sayfa = sayfa,
            KayitSayisi = kayitSayisi,
            ToplamKayit = toplamKayit,
            ToplamSayfa = toplamSayfa,
            Urunler = urunler
        };

        return View(viewModel);
    }

    private static (DateTime Baslangic, DateTime Bitis) TarihAraliginiGetir(
        DateTime? baslangic,
        DateTime? bitis,
        string? donem)
    {
        var bugun = DateTime.Today;
        donem = string.IsNullOrWhiteSpace(donem) ? "BuAy" : donem;

        if (baslangic.HasValue && bitis.HasValue)
            return (baslangic.Value.Date, bitis.Value.Date);

        return donem switch
        {
            "Bugun" => (bugun, bugun),

            "BuHafta" => (
                bugun.AddDays(-((int)bugun.DayOfWeek == 0 ? 6 : (int)bugun.DayOfWeek - 1)),
                bugun
            ),

            "BuYil" => (new DateTime(bugun.Year, 1, 1), bugun),

            "TumZamanlar" => (new DateTime(2000, 1, 1), bugun),

            _ => (new DateTime(bugun.Year, bugun.Month, 1), bugun)
        };
    }
}