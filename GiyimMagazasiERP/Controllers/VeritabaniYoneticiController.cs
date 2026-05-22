using System.Data;
using System.Text.RegularExpressions;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class VeritabaniYoneticiController : Controller
{
    private readonly AppDbContext _context;

    private static readonly string[] TabloAdlari =
    {
        "Kullanicilar",
        "Personeller",
        "Musteriler",
        "Kategoriler",
        "Tedarikciler",
        "Urunler",
        "Satislar",
        "SatisDetaylari",
        "StokHareketleri",
        "FinansHareketleri"
    };

    public VeritabaniYoneticiController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? raporKod, string? tablo)
    {
        var model = await ModelOlustur();

        if (!string.IsNullOrWhiteSpace(raporKod))
        {
            var kayitliSorgu = KayitliSqlSorgulari()
                .FirstOrDefault(x => x.Kod == raporKod);

            if (kayitliSorgu is null)
            {
                model.HataMesaji = "Seçilen kayıtlı sorgu bulunamadı.";
                return View(model);
            }

            model.SonucTablosu = await SqlSonucuGetir(
                kayitliSorgu.Sql,
                kayitliSorgu.Baslik,
                "Bu rapor için kayıt bulunamadı.");

            model.BasariMesaji =
                $"{kayitliSorgu.Baslik} raporu çalıştırıldı. {model.SonucTablosu.KayitSayisi} kayıt getirildi.";
        }

        if (!string.IsNullOrWhiteSpace(tablo))
        {
            if (!TabloAdlari.Contains(tablo))
            {
                model.HataMesaji = "Seçilen tablo görüntülenemez.";
                return View(model);
            }

            var sql = $"SELECT TOP (50) * FROM [{tablo}] ORDER BY Id DESC";

            model.SonucTablosu = await SqlSonucuGetir(
                sql,
                $"{tablo} Tablo Tarayıcı",
                "Bu tabloda kayıt bulunamadı.");

            model.BasariMesaji =
                $"{tablo} tablosundan ilk 50 kayıt görüntülendi. {model.SonucTablosu.KayitSayisi} kayıt getirildi.";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalistirSql(string sqlSorgusu)
    {
        var model = await ModelOlustur();
        model.SqlSorgusu = sqlSorgusu ?? "";

        if (!SerbestSqlGuvenliMi(model.SqlSorgusu, out var hata))
        {
            model.HataMesaji = hata;
            return View("Index", model);
        }

        try
        {
            model.SonucTablosu = await SqlSonucuGetir(
                model.SqlSorgusu,
                "SQL Editörü Sonucu",
                "Bu sorgu için kayıt bulunamadı.");

            model.BasariMesaji =
                $"Sorgu başarıyla çalıştı. {model.SonucTablosu.KayitSayisi} kayıt getirildi.";
        }
        catch
        {
            model.HataMesaji =
                "Sorgu çalıştırılamadı. SELECT sözdizimini ve tablo/kolon adlarını kontrol et.";
        }

        return View("Index", model);
    }

    private async Task<VeritabaniYoneticiViewModel> ModelOlustur()
    {
        var toplamGelir = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gelir")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var toplamGider = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        return new VeritabaniYoneticiViewModel
        {
            Istatistikler = new VeritabaniIstatistikViewModel
            {
                ToplamTabloSayisi = 10,

                ToplamUrunSayisi = await _context.Urunler.CountAsync(),

                ToplamStokAdedi = await _context.Urunler
            .SumAsync(x => (int?)x.StokMiktari) ?? 0,

                ToplamMusteriSayisi = await _context.Musteriler.CountAsync(),
                ToplamPersonelSayisi = await _context.Personeller.CountAsync(),
                ToplamSatisSayisi = await _context.Satislar.CountAsync(),

                KritikStokSayisi = await _context.Urunler
            .CountAsync(x => x.AktifMi && x.StokMiktari <= x.MinimumStok),

                ToplamGelir = toplamGelir,
                ToplamGider = toplamGider,
                NetKazanc = toplamGelir - toplamGider
            },

            SemaTablolari = SemaTablolariGetir(),

            KayitliSorgular = KayitliSqlSorgulari()
        .Select(x => new KayitliSorguViewModel
        {
            Kod = x.Kod,
            Baslik = x.Baslik,
            Aciklama = x.Aciklama,
            Kategori = x.Kategori
        })
        .ToList(),

            TabloTarayiciTablolari = TabloAdlari.ToList()
        };
    }

    private async Task<DinamikSonucTablosuViewModel> SqlSonucuGetir(
        string sql,
        string baslik,
        string bosKayitMesaji)
    {
        var sonuc = new DinamikSonucTablosuViewModel
        {
            Baslik = baslik,
            BosKayitMesaji = bosKayitMesaji
        };

        var connection = _context.Database.GetDbConnection();
        var baglantiAcildiMi = connection.State != ConnectionState.Open;

        if (baglantiAcildiMi)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 15;

            await using var reader = await command.ExecuteReaderAsync();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                sonuc.Sutunlar.Add(reader.GetName(i));
            }

            while (await reader.ReadAsync() && sonuc.Satirlar.Count < 200)
            {
                var satir = new Dictionary<string, string>();

                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var sutunAdi = reader.GetName(i);
                    var deger = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    satir[sutunAdi] = DegeriYazdir(sutunAdi, deger);
                }

                sonuc.Satirlar.Add(satir);
            }
        }
        finally
        {
            if (baglantiAcildiMi)
                await connection.CloseAsync();
        }

        return sonuc;
    }

    private static bool SerbestSqlGuvenliMi(string? sql, out string hata)
    {
        hata = "";

        if (string.IsNullOrWhiteSpace(sql))
        {
            hata = "SQL sorgusu boş bırakılamaz.";
            return false;
        }

        var temizSql = sql.Trim();

        if (!Regex.IsMatch(temizSql, @"^SELECT\b", RegexOptions.IgnoreCase))
        {
            hata = "Bu editörde yalnızca SELECT ile başlayan sorgular çalıştırılabilir.";
            return false;
        }

        if (temizSql.Contains(';'))
        {
            hata = "Tek sorgu çalıştırılabilir. Noktalı virgül kullanma.";
            return false;
        }

        var engelliKomutlar =
            @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|TRUNCATE|EXEC|EXECUTE|CREATE|MERGE|INTO)\b";

        if (Regex.IsMatch(temizSql, engelliKomutlar, RegexOptions.IgnoreCase))
        {
            hata = "Veri değiştiren veya çalıştırılabilir komutlar güvenlik nedeniyle engellendi.";
            return false;
        }

        if (Regex.IsMatch(temizSql, @"(--|/\*|\*/)", RegexOptions.IgnoreCase))
        {
            hata = "Yorum içeren SQL metinleri bu panelde çalıştırılamaz.";
            return false;
        }

        return true;
    }

    private static string DegeriYazdir(string sutunAdi, object? deger)
    {
        if (deger is null)
            return "-";

        if (deger is DateTime tarih)
            return tarih.ToString("dd/MM/yyyy");

        if (deger is decimal tutar && ParaKolonuMu(sutunAdi))
            return $"{tutar:N2} TL";

        if (deger is double doubleDeger && ParaKolonuMu(sutunAdi))
            return $"{doubleDeger:N2} TL";

        if (deger is bool boolDeger)
            return boolDeger ? "Evet" : "Hayır";

        return deger.ToString() ?? "-";
    }

    private static bool ParaKolonuMu(string sutunAdi)
    {
        var ad = sutunAdi.ToLowerInvariant();

        return ad.Contains("tutar")
            || ad.Contains("fiyat")
            || ad.Contains("maas")
            || ad.Contains("harcama")
            || ad.Contains("gelir")
            || ad.Contains("gider")
            || ad.Contains("kazanc")
            || ad.Contains("sermaye")
            || ad.Contains("toplam")
            || ad.Contains("net");
    }

    private static List<KayitliSqlSorgu> KayitliSqlSorgulari()
    {
        return new List<KayitliSqlSorgu>
        {
            new("kritik-stok", "Kritik Stoktaki Ürünler", "Minimum stok seviyesinde veya altında kalan ürünleri gösterir.", "Stok",
                """
                SELECT U.UrunAdi AS Urun,
                       U.Barkod,
                       U.StokMiktari,
                       U.MinimumStok,
                       K.KategoriAdi AS Kategori
                FROM Urunler U
                INNER JOIN Kategoriler K ON K.Id = U.KategoriId
                WHERE U.AktifMi = 1
                  AND U.StokMiktari <= U.MinimumStok
                ORDER BY U.StokMiktari ASC, U.UrunAdi ASC
                """),

            new("stok-cikis-hareketleri", "Stok Çıkış Hareketleri", "Satış nedeniyle oluşan stok çıkış hareketlerini listeler.", "Stok",
                """
                SELECT SH.Tarih,
                       U.UrunAdi AS Urun,
                       SH.HareketTipi,
                       SH.Miktar,
                       SH.Aciklama
                FROM StokHareketleri SH
                INNER JOIN Urunler U ON U.Id = SH.UrunId
                WHERE SH.HareketTipi = 'SatisCikis'
                ORDER BY SH.Tarih DESC, SH.Id DESC
                """),

            new("hic-satilmayan", "Hiç Satılmayan Ürünler", "Satış detayında bulunmayan ürünleri listeler.", "Stok",
                """
                SELECT U.UrunAdi,
                       U.Barkod,
                       U.StokMiktari
                FROM Urunler U
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM SatisDetaylari SD
                    WHERE SD.UrunId = U.Id
                )
                ORDER BY U.UrunAdi
                """),

            new("en-cok-satilan", "En Çok Satılan Ürünler", "Ürün bazında toplam satılan adet ve satış tutarını gösterir.", "Satış",
                """
                SELECT TOP (10)
                       U.UrunAdi AS Urun,
                       SUM(SD.Adet) AS ToplamAdet,
                       SUM(SD.ToplamTutar) AS ToplamSatisTutari
                FROM SatisDetaylari SD
                INNER JOIN Urunler U ON U.Id = SD.UrunId
                GROUP BY U.UrunAdi
                ORDER BY ToplamAdet DESC, ToplamSatisTutari DESC
                """),

            new("sepetli-satis-detaylari", "Sepetli Satış Detayları", "Bir satışın içinde hangi ürünlerden kaç adet satıldığını gösterir.", "Satış",
                """
                SELECT S.Id AS SatisId,
                       S.SatisTarihi,
                       ISNULL(M.AdSoyad, 'Kayıtsız Müşteri') AS Musteri,
                       P.AdSoyad AS Personel,
                       U.UrunAdi AS Urun,
                       SD.Adet,
                       SD.BirimFiyat,
                       SD.ToplamTutar AS SatirToplami,
                       S.OdemeTipi
                FROM Satislar S
                INNER JOIN SatisDetaylari SD ON SD.SatisId = S.Id
                INNER JOIN Urunler U ON U.Id = SD.UrunId
                INNER JOIN Personeller P ON P.Id = S.PersonelId
                LEFT JOIN Musteriler M ON M.Id = S.MusteriId
                ORDER BY S.SatisTarihi DESC, S.Id DESC, SD.Id ASC
                """),

            new("cok-urunlu-satislar", "Çok Ürünlü Satışlar", "Birden fazla ürün içeren satışları listeler.", "Satış",
                """
                SELECT S.Id AS SatisId,
                       S.SatisTarihi,
                       ISNULL(M.AdSoyad, 'Kayıtsız Müşteri') AS Musteri,
                       COUNT(SD.Id) AS UrunKalemSayisi,
                       SUM(SD.Adet) AS ToplamAdet,
                       S.NetTutar
                FROM Satislar S
                INNER JOIN SatisDetaylari SD ON SD.SatisId = S.Id
                LEFT JOIN Musteriler M ON M.Id = S.MusteriId
                GROUP BY S.Id,
                         S.SatisTarihi,
                         M.AdSoyad,
                         S.NetTutar
                HAVING COUNT(SD.Id) > 1
                ORDER BY S.SatisTarihi DESC, S.Id DESC
                """),

            new("gunluk-satis", "Günlük Satış Özeti", "Günlük satış sayısı ve tutar özetini gösterir.", "Satış",
                """
                SELECT CAST(SatisTarihi AS DATE) AS Tarih,
                       COUNT(*) AS SatisSayisi,
                       SUM(ToplamTutar) AS ToplamSatis,
                       SUM(IndirimTutari) AS ToplamIndirim,
                       SUM(NetTutar) AS NetSatis
                FROM Satislar
                GROUP BY CAST(SatisTarihi AS DATE)
                ORDER BY Tarih DESC
                """),

            new("aylik-satis", "Aylık Satış Özeti", "Ay bazında satış sayısı, toplam satış, indirim ve net satış tutarını gösterir.", "Satış",
                """
                SELECT YEAR(SatisTarihi) AS Yil,
                       MONTH(SatisTarihi) AS Ay,
                       COUNT(*) AS SatisSayisi,
                       SUM(ToplamTutar) AS ToplamSatis,
                       SUM(IndirimTutari) AS ToplamIndirim,
                       SUM(NetTutar) AS NetSatis
                FROM Satislar
                GROUP BY YEAR(SatisTarihi), MONTH(SatisTarihi)
                ORDER BY Yil DESC, Ay DESC
                """),

            new("yillik-satis", "Yıllık Satış Özeti", "Yıl bazında satış sayısı, toplam satış, indirim ve net satış tutarını gösterir.", "Satış",
                """
                SELECT YEAR(SatisTarihi) AS Yil,
                       COUNT(*) AS SatisSayisi,
                       SUM(ToplamTutar) AS ToplamSatis,
                       SUM(IndirimTutari) AS ToplamIndirim,
                       SUM(NetTutar) AS NetSatis
                FROM Satislar
                GROUP BY YEAR(SatisTarihi)
                ORDER BY Yil DESC
                """),

            new("kategori-satis", "Kategori Bazlı Satış", "Kategori bazında satış adet ve tutar analizini gösterir.", "Satış",
                """
                SELECT K.KategoriAdi,
                       SUM(SD.Adet) AS ToplamAdet,
                       SUM(SD.ToplamTutar) AS ToplamSatisTutari
                FROM SatisDetaylari SD
                INNER JOIN Urunler U ON U.Id = SD.UrunId
                INNER JOIN Kategoriler K ON K.Id = U.KategoriId
                GROUP BY K.KategoriAdi
                ORDER BY ToplamSatisTutari DESC
                """),

            new("beden-satis", "Beden Bazlı Satış", "Bedenlere göre satış dağılımını gösterir.", "Satış",
                """
                SELECT U.Beden,
                       SUM(SD.Adet) AS ToplamAdet,
                       SUM(SD.ToplamTutar) AS ToplamSatisTutari
                FROM SatisDetaylari SD
                INNER JOIN Urunler U ON U.Id = SD.UrunId
                GROUP BY U.Beden
                ORDER BY ToplamAdet DESC
                """),

            new("renk-satis", "Renk Bazlı Satış", "Renklere göre satış dağılımını gösterir.", "Satış",
                """
                SELECT U.Renk,
                       SUM(SD.Adet) AS ToplamAdet,
                       SUM(SD.ToplamTutar) AS ToplamSatisTutari
                FROM SatisDetaylari SD
                INNER JOIN Urunler U ON U.Id = SD.UrunId
                GROUP BY U.Renk
                ORDER BY ToplamAdet DESC
                """),

            new("gelir-gider", "Gelir-Gider Özeti", "Günlük finans özetini gösterir.", "Finans",
                """
                SELECT CAST(Tarih AS DATE) AS Tarih,
                       SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE 0 END) AS ToplamGelir,
                       SUM(CASE WHEN HareketTipi = 'Gider' THEN Tutar ELSE 0 END) AS ToplamGider,
                       SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE -Tutar END) AS NetKazanc
                FROM FinansHareketleri
                GROUP BY CAST(Tarih AS DATE)
                ORDER BY Tarih DESC
                """),

            new("aylik-gelir-gider", "Aylık Gelir-Gider Özeti", "Ay bazında gelir, gider ve net kazancı gösterir.", "Finans",
                """
                SELECT YEAR(Tarih) AS Yil,
                       MONTH(Tarih) AS Ay,
                       SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE 0 END) AS ToplamGelir,
                       SUM(CASE WHEN HareketTipi = 'Gider' THEN Tutar ELSE 0 END) AS ToplamGider,
                       SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE -Tutar END) AS NetKazanc
                FROM FinansHareketleri
                GROUP BY YEAR(Tarih), MONTH(Tarih)
                ORDER BY Yil DESC, Ay DESC
                """),

            new("yillik-gelir-gider", "Yıllık Gelir-Gider Özeti", "Yıl bazında gelir, gider ve net kazancı gösterir.", "Finans",
                """
                SELECT YEAR(Tarih) AS Yil,
                       SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE 0 END) AS ToplamGelir,
                       SUM(CASE WHEN HareketTipi = 'Gider' THEN Tutar ELSE 0 END) AS ToplamGider,
                       SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE -Tutar END) AS NetKazanc
                FROM FinansHareketleri
                GROUP BY YEAR(Tarih)
                ORDER BY Yil DESC
                """),

            new("net-kazanc", "Net Kazanç Özeti", "Toplam gelir, toplam gider ve net kazancı gösterir.", "Finans",
                """
                SELECT SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE 0 END) AS ToplamGelir,
                       SUM(CASE WHEN HareketTipi = 'Gider' THEN Tutar ELSE 0 END) AS ToplamGider,
                       SUM(CASE WHEN HareketTipi = 'Gelir' THEN Tutar ELSE -Tutar END) AS NetKazanc
                FROM FinansHareketleri
                """),

            new("gider-kategorileri", "Gider Kategorileri Raporu", "Giderleri kategori bazında gruplar.", "Finans",
                """
                SELECT Kategori AS GiderKategorisi,
                       COUNT(*) AS HareketSayisi,
                       SUM(Tutar) AS ToplamGider
                FROM FinansHareketleri
                WHERE HareketTipi = 'Gider'
                GROUP BY Kategori
                ORDER BY ToplamGider DESC, GiderKategorisi ASC
                """),

            new("en-yuksek-giderler", "En Yüksek Giderler", "Tutarı en yüksek gider hareketlerini listeler.", "Finans",
                """
                SELECT TOP (10)
                       Tarih,
                       Kategori,
                       Aciklama,
                       Tutar
                FROM FinansHareketleri
                WHERE HareketTipi = 'Gider'
                ORDER BY Tutar DESC, Tarih DESC
                """),

            new("personel-maasi-giderleri", "Personel Maaşı Giderleri", "Personel maaşı kategorisindeki giderleri listeler.", "Finans / Personel",
                """
                SELECT Tarih,
                       Aciklama,
                       Tutar
                FROM FinansHareketleri
                WHERE HareketTipi = 'Gider'
                  AND Kategori = 'Personel Maaşı'
                ORDER BY Tarih DESC, Tutar DESC
                """),

            new("isletme-sabit-giderleri", "İşletme Sabit Giderleri", "Düzenli işletme giderlerini listeler.", "Finans",
                """
                SELECT Tarih,
                       Kategori,
                       Aciklama,
                       Tutar
                FROM FinansHareketleri
                WHERE HareketTipi = 'Gider'
                  AND Kategori IN
                  (
                      'Kira',
                      'Kira Gideri',
                      'Elektrik Gideri',
                      'Su Gideri',
                      'İnternet Gideri',
                      'Internet Gideri',
                      'Temizlik Gideri'
                  )
                ORDER BY Tarih DESC, Tutar DESC
                """),

            new("baslangic-sermayesi", "Başlangıç Sermayesi Raporu", "Sermaye giriş kayıtlarını gösterir.", "Finans",
                """
                SELECT Tarih,
                       Kategori,
                       Tutar,
                       Aciklama
                FROM FinansHareketleri
                WHERE HareketTipi = 'Gelir'
                  AND Kategori = 'Baslangic Sermayesi'
                ORDER BY Tarih DESC
                """),

            new("en-cok-musteri", "En Çok Alışveriş Yapan Müşteriler", "Toplam harcaması yüksek müşterileri listeler.", "Müşteri",
                """
                SELECT TOP (10)
                       AdSoyad,
                       ToplamHarcama,
                       SadakatPuani,
                       IndirimOrani
                FROM Musteriler
                ORDER BY ToplamHarcama DESC
                """),

            new("musteri-alisveris-ozeti", "Müşteri Bazlı Alışveriş Özeti", "Müşteri satış ve harcama özetini gösterir.", "Müşteri",
                """
                SELECT M.AdSoyad AS Musteri,
                       COUNT(DISTINCT S.Id) AS SatisSayisi,
                       ISNULL(SUM(SD.Adet), 0) AS ToplamUrunAdedi,
                       M.ToplamHarcama
                FROM Musteriler M
                LEFT JOIN Satislar S ON S.MusteriId = M.Id
                LEFT JOIN SatisDetaylari SD ON SD.SatisId = S.Id
                GROUP BY M.Id,
                         M.AdSoyad,
                         M.ToplamHarcama
                ORDER BY M.ToplamHarcama DESC, ToplamUrunAdedi DESC
                """),

            new("musteri-urun", "Müşteri Hangi Ürünü En Çok Alıyor", "Müşteri ürün adet analizini gösterir.", "Müşteri",
                """
                SELECT TOP (15)
                       M.AdSoyad AS MusteriAdi,
                       U.UrunAdi,
                       SUM(SD.Adet) AS ToplamAdet
                FROM SatisDetaylari SD
                INNER JOIN Satislar S ON S.Id = SD.SatisId
                INNER JOIN Musteriler M ON M.Id = S.MusteriId
                INNER JOIN Urunler U ON U.Id = SD.UrunId
                GROUP BY M.AdSoyad, U.UrunAdi
                ORDER BY ToplamAdet DESC
                """),

            new("personel-performans", "Personel Satış Performansı", "Personel satış performansını gösterir.", "Personel",
                """
                SELECT P.AdSoyad AS PersonelAdi,
                       COUNT(DISTINCT S.Id) AS SatisSayisi,
                       ISNULL(SUM(S.NetTutar), 0) AS ToplamSatisTutari,
                       P.PrimOrani
                FROM Personeller P
                LEFT JOIN Satislar S ON S.PersonelId = P.Id
                GROUP BY P.AdSoyad, P.PrimOrani
                ORDER BY ToplamSatisTutari DESC
                """),

            new("tedarikci-urun", "Tedarikçi Ürün Raporu", "Tedarikçi ürün sayılarını gösterir.", "Tedarikçi",
                """
                SELECT T.FirmaAdi,
                       COUNT(U.Id) AS UrunSayisi
                FROM Tedarikciler T
                LEFT JOIN Urunler U ON U.TedarikciId = T.Id
                GROUP BY T.FirmaAdi
                ORDER BY UrunSayisi DESC
                """),

            new("tedarikci-indirim", "Tedarikçi İndirim Raporu", "Tedarikçi indirim oranlarını gösterir.", "Tedarikçi",
                """
                SELECT FirmaAdi,
                       IndirimOrani,
                       AktifMi
                FROM Tedarikciler
                ORDER BY IndirimOrani DESC
                """)
        };
    }

    private static List<SemaTabloViewModel> SemaTablolariGetir()
    {
        return new List<SemaTabloViewModel>
        {
            Tablo("Kullanicilar",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("KullaniciAdi", "NVARCHAR(50)"),
                Alan("Email", "NVARCHAR(100)"),
                Alan("SifreHash", "NVARCHAR(255)"),
                Alan("Rol", "NVARCHAR(30)"),
                Alan("AktifMi", "BIT"),
                Alan("OlusturmaTarihi", "DATETIME2")),

            Tablo("Personeller",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("AdSoyad", "NVARCHAR(100)"),
                Alan("Telefon", "NVARCHAR(20)", notNull: false),
                Alan("Email", "NVARCHAR(100)", notNull: false),
                Alan("Pozisyon", "NVARCHAR(50)"),
                Alan("Maas", "DECIMAL(18,2)"),
                Alan("PrimOrani", "DECIMAL(5,2)"),
                Alan("GirisSaati", "TIME", notNull: false),
                Alan("CikisSaati", "TIME", notNull: false),
                Alan("MesaiSaati", "DECIMAL(5,2)"),
                Alan("IzinGunu", "INT"),
                Alan("Departman", "NVARCHAR(50)"),
                Alan("AktifMi", "BIT"),
                Alan("IseBaslamaTarihi", "DATE")),

            Tablo("Musteriler",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("AdSoyad", "NVARCHAR(100)"),
                Alan("Telefon", "NVARCHAR(20)", notNull: false),
                Alan("Email", "NVARCHAR(100)", notNull: false),
                Alan("SadakatPuani", "INT"),
                Alan("IndirimOrani", "DECIMAL(5,2)"),
                Alan("ToplamHarcama", "DECIMAL(18,2)"),
                Alan("KayitTarihi", "DATETIME2")),

            Tablo("Kategoriler",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("KategoriAdi", "NVARCHAR(100)"),
                Alan("Aciklama", "NVARCHAR(250)", notNull: false)),

            Tablo("Tedarikciler",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("FirmaAdi", "NVARCHAR(150)"),
                Alan("Telefon", "NVARCHAR(20)", notNull: false),
                Alan("Email", "NVARCHAR(100)", notNull: false),
                Alan("Adres", "NVARCHAR(250)", notNull: false),
                Alan("IndirimOrani", "DECIMAL(5,2)"),
                Alan("AktifMi", "BIT")),

            Tablo("Urunler",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("UrunAdi", "NVARCHAR(150)"),
                Alan("Barkod", "NVARCHAR(50)"),
                Alan("KategoriId", "INT", fk: true),
                Alan("TedarikciId", "INT", fk: true),
                Alan("Beden", "NVARCHAR(20)"),
                Alan("Renk", "NVARCHAR(50)"),
                Alan("AlisFiyati", "DECIMAL(18,2)"),
                Alan("SatisFiyati", "DECIMAL(18,2)"),
                Alan("StokMiktari", "INT"),
                Alan("MinimumStok", "INT"),
                Alan("AktifMi", "BIT"),
                Alan("OlusturmaTarihi", "DATETIME2")),

            Tablo("Satislar",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("MusteriId", "INT", fk: true, notNull: false),
                Alan("PersonelId", "INT", fk: true),
                Alan("SatisTarihi", "DATETIME2"),
                Alan("ToplamTutar", "DECIMAL(18,2)"),
                Alan("IndirimTutari", "DECIMAL(18,2)"),
                Alan("NetTutar", "DECIMAL(18,2)"),
                Alan("OdemeTipi", "NVARCHAR(30)")),

            Tablo("SatisDetaylari",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("SatisId", "INT", fk: true),
                Alan("UrunId", "INT", fk: true),
                Alan("Adet", "INT"),
                Alan("BirimFiyat", "DECIMAL(18,2)"),
                Alan("ToplamTutar", "DECIMAL(18,2)")),

            Tablo("StokHareketleri",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("UrunId", "INT", fk: true),
                Alan("HareketTipi", "NVARCHAR(30)"),
                Alan("Miktar", "INT"),
                Alan("Tarih", "DATETIME2"),
                Alan("Aciklama", "NVARCHAR(250)", notNull: false)),

            Tablo("FinansHareketleri",
                Alan("Id", "INT IDENTITY", pk: true),
                Alan("SatisId", "INT", fk: true, notNull: false),
                Alan("KullaniciId", "INT", fk: true),
                Alan("HareketTipi", "NVARCHAR(20)"),
                Alan("Kategori", "NVARCHAR(100)"),
                Alan("Tutar", "DECIMAL(18,2)"),
                Alan("Tarih", "DATETIME2"),
                Alan("Aciklama", "NVARCHAR(250)", notNull: false))
        };
    }

    private static SemaTabloViewModel Tablo(string ad, params SemaAlanViewModel[] alanlar)
    {
        return new SemaTabloViewModel
        {
            TabloAdi = ad,
            Alanlar = alanlar.ToList()
        };
    }

    private static SemaAlanViewModel Alan(
        string ad,
        string tip,
        bool pk = false,
        bool fk = false,
        bool notNull = true)
    {
        return new SemaAlanViewModel
        {
            AlanAdi = ad,
            VeriTipi = tip,
            PrimaryKeyMi = pk,
            ForeignKeyMi = fk,
            NotNullMi = notNull
        };
    }

    private sealed record KayitliSqlSorgu(
        string Kod,
        string Baslik,
        string Aciklama,
        string Kategori,
        string Sql);
}