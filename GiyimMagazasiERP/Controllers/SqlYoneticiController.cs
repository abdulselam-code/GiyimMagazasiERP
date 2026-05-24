using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin")]
public class SqlYoneticiController : Controller
{
    private readonly AppDbContext _context;

    public SqlYoneticiController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? raporKodu)
    {
        var viewModel = new SqlYoneticiViewModel
        {
            SeciliRaporKodu = raporKodu,
            HazirRaporlar = HazirRaporlariGetir()
        };

        if (!string.IsNullOrWhiteSpace(raporKodu))
        {
            viewModel.SonucTablosu = await RaporuCalistir(raporKodu);
        }

        return View(viewModel);
    }

    private static List<HazirRaporSecenegiViewModel> HazirRaporlariGetir()
    {
        return new List<HazirRaporSecenegiViewModel>
        {
            new()
            {
                Kod = "kritik-stok",
                Baslik = "Kritik Stoktaki Ürünler",
                Aciklama = "Stok miktarı minimum stok seviyesine düşen ürünleri listeler."
            },
            new()
            {
                Kod = "en-cok-satilan-urunler",
                Baslik = "En Çok Satılan Ürünler",
                Aciklama = "Satılan adet ve satış tutarına göre ürün performansını gösterir."
            },
            new()
            {
                Kod = "hic-satilmayan-urunler",
                Baslik = "Hiç Satılmayan Ürünler",
                Aciklama = "Satış detaylarında yer almayan ürünleri listeler."
            },
            new()
            {
                Kod = "gunluk-satis-ozeti",
                Baslik = "Günlük Satış Özeti",
                Aciklama = "Gün bazında satış sayısını ve net satış tutarını gösterir."
            },
            new()
            {
                Kod = "aylik-satis-ozeti",
                Baslik = "Aylık Satış Özeti",
                Aciklama = "Ay bazında satış sayısı, toplam tutar, indirim ve net satışı gösterir."
            },
            new()
            {
                Kod = "yillik-satis-ozeti",
                Baslik = "Yıllık Satış Özeti",
                Aciklama = "Yıl bazında satış sayısı, toplam tutar, indirim ve net satışı gösterir."
            },
            new()
            {
                Kod = "gelir-gider-ozeti",
                Baslik = "Gelir-Gider Özeti",
                Aciklama = "Finans hareketlerini gün bazında gelir ve gider olarak özetler."
            },
            new()
            {
                Kod = "aylik-gelir-gider-ozeti",
                Baslik = "Aylık Gelir-Gider Özeti",
                Aciklama = "Ay bazında gelir, gider ve net kazancı gösterir."
            },
            new()
            {
                Kod = "yillik-gelir-gider-ozeti",
                Baslik = "Yıllık Gelir-Gider Özeti",
                Aciklama = "Yıl bazında gelir, gider ve net kazancı gösterir."
            },
            new()
            {
                Kod = "gider-kategorileri",
                Baslik = "Gider Kategorileri Raporu",
                Aciklama = "Giderleri kategori bazında toplam tutar ve hareket sayısı ile özetler."
            },
            new()
            {
                Kod = "en-yuksek-giderler",
                Baslik = "En Yüksek Giderler",
                Aciklama = "Tutarı en yüksek ilk 10 gider hareketini listeler."
            },
            new()
            {
                Kod = "net-kazanc-ozeti",
                Baslik = "Net Kazanç Özeti",
                Aciklama = "Toplam gelir, toplam gider ve net kazanç bilgisini gösterir."
            },
            new()
            {
                Kod = "en-cok-alisveris-yapan-musteriler",
                Baslik = "En Çok Alışveriş Yapan Müşteriler",
                Aciklama = "Toplam harcaması en yüksek müşterileri listeler."
            },
            new()
            {
                Kod = "musteri-urun-analizi",
                Baslik = "Müşteri Hangi Ürünü En Çok Alıyor",
                Aciklama = "Müşteri ve ürün bazında satın alınan toplam adedi gösterir."
            },
            new()
            {
                Kod = "personel-satis-performansi",
                Baslik = "Personel Satış Performansı",
                Aciklama = "Personellerin satış sayısı ve satış tutarı performansını gösterir."
            },
            new()
            {
                Kod = "tedarikci-urun-indirim",
                Baslik = "Tedarikçi Ürün ve İndirim Raporu",
                Aciklama = "Tedarikçiye bağlı ürün sayısını ve indirim oranını gösterir."
            },
            new()
            {
                Kod = "kategori-bazli-satis",
                Baslik = "Kategori Bazlı Satış Raporu",
                Aciklama = "Kategoriye göre satılan adet ve satış tutarını gösterir."
            },
            new()
            {
                Kod = "beden-bazli-satis",
                Baslik = "Beden Bazlı Satış Raporu",
                Aciklama = "Bedenlere göre satış adetlerini ve tutarlarını gösterir."
            },
            new()
            {
                Kod = "renk-bazli-satis",
                Baslik = "Renk Bazlı Satış Raporu",
                Aciklama = "Renklere göre satış adetlerini ve tutarlarını gösterir."
            },
            new()
            {
                Kod = "baslangic-sermayesi",
                Baslik = "Başlangıç Sermayesi Raporu",
                Aciklama = "Finans hareketlerindeki başlangıç sermayesi kayıtlarını gösterir."
            }
        };
    }

    private async Task<DinamikRaporTablosuViewModel> RaporuCalistir(string raporKodu)
    {
        return raporKodu switch
        {
            "kritik-stok" => await KritikStokRaporuGetir(),
            "en-cok-satilan-urunler" => await EnCokSatilanUrunlerRaporuGetir(),
            "hic-satilmayan-urunler" => await HicSatilmayanUrunlerRaporuGetir(),

            "gunluk-satis-ozeti" => await GunlukSatisOzetiGetir(),
            "aylik-satis-ozeti" => await AylikSatisOzetiGetir(),
            "yillik-satis-ozeti" => await YillikSatisOzetiGetir(),

            "gelir-gider-ozeti" => await GelirGiderOzetiGetir(),
            "aylik-gelir-gider-ozeti" => await AylikGelirGiderOzetiGetir(),
            "yillik-gelir-gider-ozeti" => await YillikGelirGiderOzetiGetir(),
            "gider-kategorileri" => await GiderKategorileriRaporuGetir(),
            "en-yuksek-giderler" => await EnYuksekGiderlerRaporuGetir(),
            "net-kazanc-ozeti" => await NetKazancOzetiGetir(),

            "en-cok-alisveris-yapan-musteriler" => await EnCokAlisverisYapanMusterilerGetir(),
            "musteri-urun-analizi" => await MusteriUrunAnaliziGetir(),
            "personel-satis-performansi" => await PersonelSatisPerformansiGetir(),
            "tedarikci-urun-indirim" => await TedarikciUrunIndirimRaporuGetir(),
            "kategori-bazli-satis" => await KategoriBazliSatisRaporuGetir(),
            "beden-bazli-satis" => await BedenBazliSatisRaporuGetir(),
            "renk-bazli-satis" => await RenkBazliSatisRaporuGetir(),
            "baslangic-sermayesi" => await BaslangicSermayesiRaporuGetir(),

            _ => GecersizRaporGetir()
        };
    }

    private async Task<DinamikRaporTablosuViewModel> KritikStokRaporuGetir()
    {
        var veriler = await _context.Urunler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Where(x => x.AktifMi && x.StokMiktari <= x.MinimumStok)
            .OrderBy(x => x.StokMiktari)
            .Select(x => new
            {
                x.UrunAdi,
                x.Barkod,
                KategoriAdi = x.Kategori.KategoriAdi,
                x.StokMiktari,
                x.MinimumStok
            })
            .ToListAsync();

        var tablo = TabloOlustur(
            "Kritik Stoktaki Ürünler",
            "Minimum stok seviyesine düşen aktif ürünler.",
            "Ürün Adı", "Barkod", "Kategori", "Stok Miktarı", "Minimum Stok");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Ürün Adı", x.UrunAdi),
                ("Barkod", x.Barkod),
                ("Kategori", x.KategoriAdi),
                ("Stok Miktarı", x.StokMiktari.ToString()),
                ("Minimum Stok", x.MinimumStok.ToString())));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> EnCokSatilanUrunlerRaporuGetir()
    {
        var veriler = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.UrunId,
                x.Urun.UrunAdi
            })
            .Select(grup => new
            {
                UrunAdi = grup.Key.UrunAdi,
                ToplamAdet = grup.Sum(x => x.Adet),
                ToplamTutar = grup.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.ToplamAdet)
            .ThenByDescending(x => x.ToplamTutar)
            .Take(10)
            .ToListAsync();

        var tablo = TabloOlustur(
            "En Çok Satılan Ürünler",
            "Satış detaylarına göre en fazla satılan ürünler.",
            "Ürün Adı", "Toplam Satılan Adet", "Toplam Satış Tutarı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Ürün Adı", x.UrunAdi),
                ("Toplam Satılan Adet", x.ToplamAdet.ToString()),
                ("Toplam Satış Tutarı", ParaYaz(x.ToplamTutar))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> HicSatilmayanUrunlerRaporuGetir()
    {
        var veriler = await _context.Urunler
            .AsNoTracking()
            .Where(x => !x.SatisDetaylari.Any())
            .OrderBy(x => x.UrunAdi)
            .Select(x => new
            {
                x.UrunAdi,
                x.Barkod,
                x.StokMiktari
            })
            .ToListAsync();

        var tablo = TabloOlustur(
            "Hiç Satılmayan Ürünler",
            "Henüz satış detaylarında bulunmayan ürünler.",
            "Ürün Adı", "Barkod", "Stok Miktarı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Ürün Adı", x.UrunAdi),
                ("Barkod", x.Barkod),
                ("Stok Miktarı", x.StokMiktari.ToString())));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> GunlukSatisOzetiGetir()
    {
        var veriler = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => x.SatisTarihi.Date)
            .Select(grup => new
            {
                Gun = grup.Key,
                SatisSayisi = grup.Count(),
                ToplamNetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Gun)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Günlük Satış Özeti",
            "Gün bazında satış sayısı ve net satış tutarı.",
            "Tarih", "Satış Sayısı", "Toplam Net Satış");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Tarih", TarihYaz(x.Gun)),
                ("Satış Sayısı", x.SatisSayisi.ToString()),
                ("Toplam Net Satış", ParaYaz(x.ToplamNetSatis))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> AylikSatisOzetiGetir()
    {
        var veriler = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.SatisTarihi.Year,
                x.SatisTarihi.Month
            })
            .Select(grup => new
            {
                grup.Key.Year,
                grup.Key.Month,
                SatisSayisi = grup.Count(),
                ToplamSatisTutari = grup.Sum(x => x.ToplamTutar),
                ToplamIndirim = grup.Sum(x => x.IndirimTutari),
                NetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Aylık Satış Özeti",
            "Ay bazında satış sayısı, toplam tutar, indirim ve net satış.",
            "Ay", "Satış Sayısı", "Toplam Satış Tutarı", "Toplam İndirim", "Net Satış");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Ay", $"{x.Year}-{x.Month:D2}"),
                ("Satış Sayısı", x.SatisSayisi.ToString()),
                ("Toplam Satış Tutarı", ParaYaz(x.ToplamSatisTutari)),
                ("Toplam İndirim", ParaYaz(x.ToplamIndirim)),
                ("Net Satış", ParaYaz(x.NetSatis))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> YillikSatisOzetiGetir()
    {
        var veriler = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => x.SatisTarihi.Year)
            .Select(grup => new
            {
                Yil = grup.Key,
                SatisSayisi = grup.Count(),
                ToplamSatisTutari = grup.Sum(x => x.ToplamTutar),
                ToplamIndirim = grup.Sum(x => x.IndirimTutari),
                NetSatis = grup.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Yil)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Yıllık Satış Özeti",
            "Yıl bazında satış sayısı, toplam tutar, indirim ve net satış.",
            "Yıl", "Satış Sayısı", "Toplam Satış Tutarı", "Toplam İndirim", "Net Satış");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Yıl", x.Yil.ToString()),
                ("Satış Sayısı", x.SatisSayisi.ToString()),
                ("Toplam Satış Tutarı", ParaYaz(x.ToplamSatisTutari)),
                ("Toplam İndirim", ParaYaz(x.ToplamIndirim)),
                ("Net Satış", ParaYaz(x.NetSatis))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> GelirGiderOzetiGetir()
    {
        var veriler = await _context.FinansHareketleri
            .AsNoTracking()
            .GroupBy(x => x.Tarih.Date)
            .Select(grup => new
            {
                Gun = grup.Key,
                Gelir = grup
                    .Where(x => x.HareketTipi == "Gelir")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                Gider = grup
                    .Where(x => x.HareketTipi == "Gider")
                    .Sum(x => (decimal?)x.Tutar) ?? 0
            })
            .OrderByDescending(x => x.Gun)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Gelir-Gider Özeti",
            "Gün bazında finans hareketleri özeti.",
            "Tarih", "Gelir", "Gider", "Net");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Tarih", TarihYaz(x.Gun)),
                ("Gelir", ParaYaz(x.Gelir)),
                ("Gider", ParaYaz(x.Gider)),
                ("Net", ParaYaz(x.Gelir - x.Gider))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> AylikGelirGiderOzetiGetir()
    {
        var veriler = await _context.FinansHareketleri
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.Tarih.Year,
                x.Tarih.Month
            })
            .Select(grup => new
            {
                grup.Key.Year,
                grup.Key.Month,
                Gelir = grup
                    .Where(x => x.HareketTipi == "Gelir")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                Gider = grup
                    .Where(x => x.HareketTipi == "Gider")
                    .Sum(x => (decimal?)x.Tutar) ?? 0
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Aylık Gelir-Gider Özeti",
            "Ay bazında gelir, gider ve net kazanç.",
            "Ay", "Toplam Gelir", "Toplam Gider", "Net Kazanç");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Ay", $"{x.Year}-{x.Month:D2}"),
                ("Toplam Gelir", ParaYaz(x.Gelir)),
                ("Toplam Gider", ParaYaz(x.Gider)),
                ("Net Kazanç", ParaYaz(x.Gelir - x.Gider))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> YillikGelirGiderOzetiGetir()
    {
        var veriler = await _context.FinansHareketleri
            .AsNoTracking()
            .GroupBy(x => x.Tarih.Year)
            .Select(grup => new
            {
                Yil = grup.Key,
                Gelir = grup
                    .Where(x => x.HareketTipi == "Gelir")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                Gider = grup
                    .Where(x => x.HareketTipi == "Gider")
                    .Sum(x => (decimal?)x.Tutar) ?? 0
            })
            .OrderByDescending(x => x.Yil)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Yıllık Gelir-Gider Özeti",
            "Yıl bazında gelir, gider ve net kazanç.",
            "Yıl", "Toplam Gelir", "Toplam Gider", "Net Kazanç");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Yıl", x.Yil.ToString()),
                ("Toplam Gelir", ParaYaz(x.Gelir)),
                ("Toplam Gider", ParaYaz(x.Gider)),
                ("Net Kazanç", ParaYaz(x.Gelir - x.Gider))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> GiderKategorileriRaporuGetir()
    {
        var veriler = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .GroupBy(x => x.Kategori)
            .Select(grup => new
            {
                GiderKategorisi = grup.Key,
                ToplamTutar = grup.Sum(x => x.Tutar),
                HareketSayisi = grup.Count()
            })
            .OrderByDescending(x => x.ToplamTutar)
            .ThenBy(x => x.GiderKategorisi)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Gider Kategorileri Raporu",
            "Giderlerin kategori bazında toplam tutarı ve hareket sayısı.",
            "Gider Kategorisi", "Toplam Tutar", "Hareket Sayısı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Gider Kategorisi", x.GiderKategorisi),
                ("Toplam Tutar", ParaYaz(x.ToplamTutar)),
                ("Hareket Sayısı", x.HareketSayisi.ToString())));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> EnYuksekGiderlerRaporuGetir()
    {
        var veriler = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .OrderByDescending(x => x.Tutar)
            .ThenByDescending(x => x.Tarih)
            .Select(x => new
            {
                x.Tarih,
                x.Kategori,
                x.Aciklama,
                x.Tutar
            })
            .Take(10)
            .ToListAsync();

        var tablo = TabloOlustur(
            "En Yüksek Giderler",
            "Tutarı en yüksek ilk 10 gider hareketi.",
            "Tarih", "Kategori", "Açıklama", "Tutar");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Tarih", TarihYaz(x.Tarih)),
                ("Kategori", x.Kategori),
                ("Açıklama", x.Aciklama ?? "-"),
                ("Tutar", ParaYaz(x.Tutar))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> NetKazancOzetiGetir()
    {
        var toplamGelir = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gelir")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var toplamGider = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var tablo = TabloOlustur(
            "Net Kazanç Özeti",
            "Toplam gelir ve gider farkı.",
            "Toplam Gelir", "Toplam Gider", "Net Kazanç");

        tablo.Satirlar.Add(SatirOlustur(
            ("Toplam Gelir", ParaYaz(toplamGelir)),
            ("Toplam Gider", ParaYaz(toplamGider)),
            ("Net Kazanç", ParaYaz(toplamGelir - toplamGider))));

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> EnCokAlisverisYapanMusterilerGetir()
    {
        var veriler = await _context.Musteriler
            .AsNoTracking()
            .OrderByDescending(x => x.ToplamHarcama)
            .Take(10)
            .Select(x => new
            {
                x.AdSoyad,
                x.ToplamHarcama,
                x.SadakatPuani,
                x.IndirimOrani
            })
            .ToListAsync();

        var tablo = TabloOlustur(
            "En Çok Alışveriş Yapan Müşteriler",
            "Toplam harcaması en yüksek müşteriler.",
            "Müşteri Adı", "Toplam Harcama", "Sadakat Puanı", "İndirim Oranı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Müşteri Adı", x.AdSoyad),
                ("Toplam Harcama", ParaYaz(x.ToplamHarcama)),
                ("Sadakat Puanı", x.SadakatPuani.ToString()),
                ("İndirim Oranı", OranYaz(x.IndirimOrani))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> MusteriUrunAnaliziGetir()
    {
        var veriler = await _context.SatisDetaylari
            .AsNoTracking()
            .Where(x => x.Satis.MusteriId != null)
            .GroupBy(x => new
            {
                MusteriAdi = x.Satis.Musteri!.AdSoyad,
                x.Urun.UrunAdi
            })
            .Select(grup => new
            {
                grup.Key.MusteriAdi,
                grup.Key.UrunAdi,
                ToplamAdet = grup.Sum(x => x.Adet)
            })
            .OrderByDescending(x => x.ToplamAdet)
            .ThenBy(x => x.MusteriAdi)
            .Take(15)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Müşteri Hangi Ürünü En Çok Alıyor",
            "Müşteri ve ürün bazında satın alma adetleri.",
            "Müşteri Adı", "Ürün Adı", "Toplam Adet");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Müşteri Adı", x.MusteriAdi),
                ("Ürün Adı", x.UrunAdi),
                ("Toplam Adet", x.ToplamAdet.ToString())));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> PersonelSatisPerformansiGetir()
    {
        var veriler = await _context.Personeller
            .AsNoTracking()
            .Select(x => new
            {
                x.AdSoyad,
                SatisSayisi = x.Satislar.Count(),
                ToplamSatisTutari = x.Satislar.Sum(s => (decimal?)s.NetTutar) ?? 0,
                x.PrimOrani
            })
            .OrderByDescending(x => x.ToplamSatisTutari)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Personel Satış Performansı",
            "Personellerin satış sayısı ve satış tutarı performansı.",
            "Personel Adı", "Satış Sayısı", "Toplam Satış Tutarı", "Prim Oranı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Personel Adı", x.AdSoyad),
                ("Satış Sayısı", x.SatisSayisi.ToString()),
                ("Toplam Satış Tutarı", ParaYaz(x.ToplamSatisTutari)),
                ("Prim Oranı", OranYaz(x.PrimOrani))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> TedarikciUrunIndirimRaporuGetir()
    {
        var veriler = await _context.Tedarikciler
            .AsNoTracking()
            .Select(x => new
            {
                x.FirmaAdi,
                UrunSayisi = x.Urunler.Count(),
                x.IndirimOrani
            })
            .OrderByDescending(x => x.UrunSayisi)
            .ThenBy(x => x.FirmaAdi)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Tedarikçi Ürün ve İndirim Raporu",
            "Tedarikçi ürün sayıları ve indirim oranları.",
            "Tedarikçi Firma Adı", "Ürün Sayısı", "İndirim Oranı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Tedarikçi Firma Adı", x.FirmaAdi),
                ("Ürün Sayısı", x.UrunSayisi.ToString()),
                ("İndirim Oranı", OranYaz(x.IndirimOrani))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> KategoriBazliSatisRaporuGetir()
    {
        var veriler = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => new
            {
                x.Urun.KategoriId,
                x.Urun.Kategori.KategoriAdi
            })
            .Select(grup => new
            {
                KategoriAdi = grup.Key.KategoriAdi,
                ToplamAdet = grup.Sum(x => x.Adet),
                ToplamTutar = grup.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.ToplamTutar)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Kategori Bazlı Satış Raporu",
            "Kategori bazında satış adetleri ve tutarları.",
            "Kategori Adı", "Toplam Satılan Adet", "Toplam Satış Tutarı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Kategori Adı", x.KategoriAdi),
                ("Toplam Satılan Adet", x.ToplamAdet.ToString()),
                ("Toplam Satış Tutarı", ParaYaz(x.ToplamTutar))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> BedenBazliSatisRaporuGetir()
    {
        var veriler = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => x.Urun.Beden)
            .Select(grup => new
            {
                Beden = grup.Key,
                ToplamAdet = grup.Sum(x => x.Adet),
                ToplamTutar = grup.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.ToplamAdet)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Beden Bazlı Satış Raporu",
            "Ürün bedenlerine göre satış dağılımı.",
            "Beden", "Toplam Satılan Adet", "Toplam Satış Tutarı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Beden", x.Beden),
                ("Toplam Satılan Adet", x.ToplamAdet.ToString()),
                ("Toplam Satış Tutarı", ParaYaz(x.ToplamTutar))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> RenkBazliSatisRaporuGetir()
    {
        var veriler = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => x.Urun.Renk)
            .Select(grup => new
            {
                Renk = grup.Key,
                ToplamAdet = grup.Sum(x => x.Adet),
                ToplamTutar = grup.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.ToplamAdet)
            .ToListAsync();

        var tablo = TabloOlustur(
            "Renk Bazlı Satış Raporu",
            "Ürün renklerine göre satış dağılımı.",
            "Renk", "Toplam Satılan Adet", "Toplam Satış Tutarı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Renk", x.Renk),
                ("Toplam Satılan Adet", x.ToplamAdet.ToString()),
                ("Toplam Satış Tutarı", ParaYaz(x.ToplamTutar))));
        }

        return tablo;
    }

    private async Task<DinamikRaporTablosuViewModel> BaslangicSermayesiRaporuGetir()
    {
        var veriler = await _context.FinansHareketleri
            .AsNoTracking()
            .Include(x => x.Kullanici)
            .Where(x =>
                x.HareketTipi == "Gelir" &&
                x.Kategori == "Baslangic Sermayesi")
            .OrderByDescending(x => x.Tarih)
            .Select(x => new
            {
                x.Tarih,
                x.Kategori,
                x.Tutar,
                x.Aciklama,
                KullaniciAdi = x.Kullanici.KullaniciAdi
            })
            .ToListAsync();

        var tablo = TabloOlustur(
            "Başlangıç Sermayesi Raporu",
            "Başlangıç sermayesi olarak girilen finans hareketleri.",
            "Tarih", "Kategori", "Tutar", "Açıklama", "Kullanıcı");

        foreach (var x in veriler)
        {
            tablo.Satirlar.Add(SatirOlustur(
                ("Tarih", TarihYaz(x.Tarih)),
                ("Kategori", x.Kategori),
                ("Tutar", ParaYaz(x.Tutar)),
                ("Açıklama", x.Aciklama ?? "-"),
                ("Kullanıcı", x.KullaniciAdi)));
        }

        return tablo;
    }

    private static DinamikRaporTablosuViewModel GecersizRaporGetir()
    {
        return TabloOlustur(
            "Geçersiz Rapor Seçimi",
            "Sadece hazır rapor listesinde bulunan sorgular çalıştırılabilir.",
            "Bilgi");
    }

    private static DinamikRaporTablosuViewModel TabloOlustur(
        string baslik,
        string aciklama,
        params string[] sutunlar)
    {
        return new DinamikRaporTablosuViewModel
        {
            Baslik = baslik,
            Aciklama = aciklama,
            Sutunlar = sutunlar.ToList()
        };
    }

    private static Dictionary<string, string> SatirOlustur(
        params (string Sutun, string Deger)[] hucreler)
    {
        return hucreler.ToDictionary(x => x.Sutun, x => x.Deger);
    }

    private static string ParaYaz(decimal tutar)
    {
        return $"{tutar:N2} TL";
    }

    private static string OranYaz(decimal oran)
    {
        return $"% {oran:N2}";
    }

    private static string TarihYaz(DateTime tarih)
    {
        return tarih.ToString("dd/MM/yyyy");
    }
}