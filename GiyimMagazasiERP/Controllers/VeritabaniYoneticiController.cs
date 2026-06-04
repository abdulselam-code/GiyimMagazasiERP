using System.Data;
using System.Text.RegularExpressions;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin")]
public class VeritabaniYoneticiController : Controller
{
    private readonly AppDbContext _context;

    

    public VeritabaniYoneticiController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
     string? raporKod,
     string? tablo,
     string? limit,
     string? panel)
    {
        var model = await ModelOlustur();

        model.AktifPanel = string.IsNullOrWhiteSpace(panel)
            ? "schemaPanel"
            : panel;

        model.TabloTarayiciLimit = LimitDegeriniGetir(limit);

        if (!string.IsNullOrWhiteSpace(raporKod))
        {
            model.AktifPanel = "queriesPanel";

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
            model.AktifPanel = "tableBrowserPanel";

            if (!model.TabloTarayiciTablolari.Contains(tablo))
            {
                model.HataMesaji = "Seçilen tablo görüntülenemez.";
                return View(model);
            }

            var gecerliLimit = LimitDegeriniGetir(limit);
            var tabloNesneAdi = await TabloNesneAdiniGetir(tablo);

            model.SeciliTablo = tablo;
            model.TabloTarayiciLimit = gecerliLimit;

            var toplamKayitSayisi = await TabloKayitSayisiniGetir(tabloNesneAdi);
            var sql = TabloTarayiciSqlOlustur(tabloNesneAdi, gecerliLimit);

            model.SonucTablosu = await SqlSonucuGetir(
                sql,
                $"{tablo} Tablo Tarayıcı",
                "Bu tabloda kayıt bulunamadı.",
                LimitSatirSayisiniGetir(gecerliLimit));

            model.SonucTablosu.ToplamKayitSayisi = toplamKayitSayisi;
            model.SonucTablosu.KayitBilgisi = TabloKayitBilgisiOlustur(
                toplamKayitSayisi,
                model.SonucTablosu.KayitSayisi,
                gecerliLimit);

            model.BasariMesaji =
                $"{tablo} tablosu görüntülendi. {model.SonucTablosu.KayitSayisi} kayıt getirildi.";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CalistirSql(string sqlSorgusu)
    {
        var model = await ModelOlustur();
        model.AktifPanel = "sqlPanel";
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
        var semaTablolari = await SemaTablolariGetir();
        var tabloAdlari = semaTablolari
            .Select(x => x.TabloAdi)
            .OrderBy(x => x)
            .ToList();

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
                ToplamTabloSayisi = tabloAdlari.Count,

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

            SemaTablolari = semaTablolari,

            KayitliSorgular = KayitliSqlSorgulari()
                .Select(x => new KayitliSorguViewModel
                {
                    Kod = x.Kod,
                    Baslik = x.Baslik,
                    Aciklama = x.Aciklama,
                    Kategori = x.Kategori
                })
                .ToList(),

            TabloTarayiciTablolari = tabloAdlari,
            TabloTarayiciLimit = "50"
        };
    }

    private static string LimitDegeriniGetir(string? limit)
    {
        return limit switch
        {
            "10" => "10",
            "25" => "25",
            "50" => "50",
            "100" => "100",
            "all" => "all",
            _ => "50"
        };
    }

    private static int LimitSatirSayisiniGetir(string limit)
    {
        return limit switch
        {
            "10" => 10,
            "25" => 25,
            "50" => 50,
            "100" => 100,
            "all" => 1000,
            _ => 50
        };
    }

    private static string TabloTarayiciSqlOlustur(string tabloNesneAdi, string limit)
    {
        var satirSayisi = LimitSatirSayisiniGetir(limit);

        return $"SELECT TOP ({satirSayisi}) * FROM {tabloNesneAdi}";
    }

    private async Task<int> TabloKayitSayisiniGetir(string tabloNesneAdi)
    {
        var sql = $"SELECT COUNT(*) FROM {tabloNesneAdi}";

        var connection = _context.Database.GetDbConnection();
        var baglantiAcildiMi = connection.State != ConnectionState.Open;

        if (baglantiAcildiMi)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 15;

            var sonuc = await command.ExecuteScalarAsync();

            return sonuc is null ? 0 : Convert.ToInt32(sonuc);
        }
        finally
        {
            if (baglantiAcildiMi)
                await connection.CloseAsync();
        }
    }

    private static string TabloKayitBilgisiOlustur(
        int toplamKayitSayisi,
        int gosterilenKayitSayisi,
        string limit)
    {
        if (limit == "all")
        {
            if (toplamKayitSayisi > 1000)
                return $"Performans için en fazla 1000 kayıt gösterilir. Toplam kayıt: {toplamKayitSayisi}.";

            return $"Tüm {toplamKayitSayisi} kayıt gösteriliyor.";
        }

        return $"Toplam {toplamKayitSayisi} kayıttan {gosterilenKayitSayisi} kayıt gösteriliyor.";
    }

    private async Task<DinamikSonucTablosuViewModel> SqlSonucuGetir(
        string sql,
        string baslik,
        string bosKayitMesaji,
        int? maksimumSatirSayisi = 200)
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

            while (await reader.ReadAsync())
            {
                if (maksimumSatirSayisi.HasValue &&
                    sonuc.Satirlar.Count >= maksimumSatirSayisi.Value)
                {
                    break;
                }

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

    private async Task<List<SemaTabloViewModel>> SemaTablolariGetir()
    {
        var tablolar = new Dictionary<string, SemaTabloViewModel>();

        const string sql = """
        SELECT
            s.name AS SchemaName,
            t.name AS TableName,
            c.name AS ColumnName,
            ty.name AS TypeName,
            c.max_length AS MaxLength,
            c.precision AS PrecisionValue,
            c.scale AS ScaleValue,
            c.is_nullable AS IsNullable,
            CASE WHEN ic.column_id IS NULL THEN 0 ELSE 1 END AS IsPrimaryKey,
            CASE WHEN fkc.parent_column_id IS NULL THEN 0 ELSE 1 END AS IsForeignKey
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        INNER JOIN sys.columns c ON c.object_id = t.object_id
        INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
        LEFT JOIN sys.indexes i
            ON i.object_id = t.object_id
           AND i.is_primary_key = 1
        LEFT JOIN sys.index_columns ic
            ON ic.object_id = t.object_id
           AND ic.index_id = i.index_id
           AND ic.column_id = c.column_id
        LEFT JOIN sys.foreign_key_columns fkc
            ON fkc.parent_object_id = t.object_id
           AND fkc.parent_column_id = c.column_id
        WHERE t.is_ms_shipped = 0
        ORDER BY t.name, c.column_id
        """;

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

            while (await reader.ReadAsync())
            {
                var tabloAdi = reader["TableName"].ToString() ?? "";
                var kolonAdi = reader["ColumnName"].ToString() ?? "";
                var tipAdi = reader["TypeName"].ToString() ?? "";

                if (!tablolar.ContainsKey(tabloAdi))
                {
                    tablolar[tabloAdi] = new SemaTabloViewModel
                    {
                        TabloAdi = tabloAdi
                    };
                }

                tablolar[tabloAdi].Alanlar.Add(new SemaAlanViewModel
                {
                    AlanAdi = kolonAdi,
                    VeriTipi = VeriTipiniYazdir(
                        tipAdi,
                        Convert.ToInt16(reader["MaxLength"]),
                        Convert.ToByte(reader["PrecisionValue"]),
                        Convert.ToByte(reader["ScaleValue"])),
                    PrimaryKeyMi = Convert.ToInt32(reader["IsPrimaryKey"]) == 1,
                    ForeignKeyMi = Convert.ToInt32(reader["IsForeignKey"]) == 1,
                    NotNullMi = !Convert.ToBoolean(reader["IsNullable"])
                });
            }
        }
        finally
        {
            if (baglantiAcildiMi)
                await connection.CloseAsync();
        }

        return tablolar.Values
            .OrderBy(x => x.TabloAdi)
            .ToList();
    }

    private async Task<string> TabloNesneAdiniGetir(string tablo)
    {
        const string sql = """
        SELECT TOP (1)
            s.name AS SchemaName,
            t.name AS TableName
        FROM sys.tables t
        INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
        WHERE t.is_ms_shipped = 0
          AND t.name = @tablo
        """;

        var connection = _context.Database.GetDbConnection();
        var baglantiAcildiMi = connection.State != ConnectionState.Open;

        if (baglantiAcildiMi)
            await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 15;

            var parameter = command.CreateParameter();
            parameter.ParameterName = "@tablo";
            parameter.Value = tablo;
            command.Parameters.Add(parameter);

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var schemaName = reader["SchemaName"].ToString() ?? "dbo";
                var tableName = reader["TableName"].ToString() ?? tablo;

                return $"[{schemaName.Replace("]", "]]")}].[{tableName.Replace("]", "]]")}]";
            }
        }
        finally
        {
            if (baglantiAcildiMi)
                await connection.CloseAsync();
        }

        return $"[dbo].[{tablo.Replace("]", "]]")}]";
    }

    private static string VeriTipiniYazdir(
        string tipAdi,
        short maxLength,
        byte precision,
        byte scale)
    {
        if (tipAdi is "nvarchar" or "nchar")
        {
            if (maxLength == -1)
                return $"{tipAdi}(MAX)";

            return $"{tipAdi}({maxLength / 2})";
        }

        if (tipAdi is "varchar" or "char" or "varbinary" or "binary")
        {
            if (maxLength == -1)
                return $"{tipAdi}(MAX)";

            return $"{tipAdi}({maxLength})";
        }

        if (tipAdi is "decimal" or "numeric")
            return $"{tipAdi}({precision},{scale})";

        return tipAdi.ToUpperInvariant();
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