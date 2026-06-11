USE GiyimMagazasiERP;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/*
    Demo ürün-tedarikçi karşılaştırma verileri
    -------------------------------------------
    - Mevcut ürün, tedarikçi veya ürün-tedarikçi kayıtlarını silmez.
    - Mevcut fiyat/indirim bilgilerini ezmez.
    - Yalnız TeslimSuresiGun = 0 olan mevcut bağlantılara teslim süresi verir.
    - Seçili ürünlerde aktif tedarikçi sayısını en fazla 3'e tamamlar.
    - Aynı UrunId + TedarikciId kaydını tekrar oluşturmaz.
    - Tekrar çalıştırılabilir.
*/

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.UrunTedarikcileri', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Urunler', N'U') IS NULL
       OR OBJECT_ID(N'dbo.Tedarikciler', N'U') IS NULL
    BEGIN
        RAISERROR(
            N'Gerekli tablolar bulunamadı. Önce 022_create_urun_tedarikcileri.sql scriptini çalıştırın.',
            16,
            1
        );
    END;

    /* Karşılaştırma havuzunda kullanılacak yedek tekstil tedarikçisi. */
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.Tedarikciler
        WHERE FirmaAdi COLLATE Turkish_100_CI_AI = N'Şanlıurfa Toptan Tekstil'
    )
    BEGIN
        INSERT INTO dbo.Tedarikciler
        (
            FirmaAdi,
            Telefon,
            Email,
            Adres,
            IndirimOrani,
            AktifMi
        )
        VALUES
        (
            N'Şanlıurfa Toptan Tekstil',
            N'0414 555 23 23',
            N'satis@sanliurfatoptantekstil.demo',
            N'Şanlıurfa Organize Sanayi Bölgesi',
            12.00,
            1
        );
    END;

    /*
      Yalnız teslim süresi girilmemiş mevcut kayıtlar güncellenir.
      Fiyat, indirim, net maliyet, minimum sipariş ve varsayılan bilgisi korunur.
    */
    UPDATE ut
    SET
        ut.TeslimSuresiGun =
            CASE
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Bursa Tekstil%' THEN 3
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Anadolu Tekstil%' THEN 2
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%ModaTekstil%' THEN 4
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Trend Kumaş%' THEN 5
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Minik Stil%' THEN 3
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Renkli Giyim%' THEN 2
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Nova Ayakkabı%' THEN 6
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Elit Aksesuar%' THEN 2
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%İstanbul Hazır Giyim%' THEN 4
                WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Şanlıurfa Toptan Tekstil%' THEN 6
                ELSE 4
            END,
        ut.GuncellemeTarihi = GETDATE()
    FROM dbo.UrunTedarikcileri ut
    INNER JOIN dbo.Tedarikciler t ON t.Id = ut.TedarikciId
    WHERE ISNULL(ut.TeslimSuresiGun, 0) = 0;

    DECLARE @HedefUrunler TABLE
    (
        UrunId INT NOT NULL PRIMARY KEY,
        UrunAdi NVARCHAR(150) NOT NULL,
        AlisFiyati DECIMAL(18,2) NOT NULL,
        MevcutAktifTedarikciSayisi INT NOT NULL
    );

    INSERT INTO @HedefUrunler
    (
        UrunId,
        UrunAdi,
        AlisFiyati,
        MevcutAktifTedarikciSayisi
    )
    SELECT
        u.Id,
        u.UrunAdi,
        u.AlisFiyati,
        (
            SELECT COUNT(*)
            FROM dbo.UrunTedarikcileri mevcut
            WHERE mevcut.UrunId = u.Id
              AND mevcut.AktifMi = 1
        )
    FROM dbo.Urunler u
    WHERE u.AktifMi = 1
      AND u.AlisFiyati > 0
      AND
      (
          u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Kadın Hırka%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Kadın Triko Kazak%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Kadın Uzun Hırka%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Erkek Spor Ceket%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Erkek Sweatshirt%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Basic Atlet%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Çocuk Sweatshirt%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Bay Kot Pantolon%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Gömlek%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Kazak%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Basic Erkek Tişört%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Basic Şapka%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Çocuk Eşofman Takımı%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Sweatshirt Classic%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Mont Classic%'
          OR u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Pantolon Classic%'
      );

    /*
      Her hedef ürün için henüz bağlı olmayan aktif tedarikçiler sıralanır.
      Sıra 1 hızlı alternatif, sıra 2 düşük net maliyetli alternatif olarak
      hazırlanır. Ürün zaten iki tedarikçiliyse yalnız bir kayıt eklenir.
    */
    ;WITH AdayTedarikciler AS
    (
        SELECT
            h.UrunId,
            h.UrunAdi,
            h.AlisFiyati,
            h.MevcutAktifTedarikciSayisi,
            t.Id AS TedarikciId,
            t.FirmaAdi,
            ROW_NUMBER() OVER
            (
                PARTITION BY h.UrunId
                ORDER BY
                    CASE
                        /* Aksesuar ürünleri */
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Şapka%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Elit Aksesuar%' THEN 1
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Şapka%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%İstanbul Hazır Giyim%' THEN 2
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Şapka%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Şanlıurfa Toptan Tekstil%' THEN 3

                        /* Çocuk ürünleri */
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Çocuk%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Renkli Giyim%' THEN 1
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Çocuk%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Minik Stil%' THEN 2
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Çocuk%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Şanlıurfa Toptan Tekstil%' THEN 3

                        /* İç giyim */
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Atlet%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Anadolu Tekstil%' THEN 1
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Atlet%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Trend Kumaş%' THEN 2
                        WHEN h.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Atlet%'
                             AND t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Şanlıurfa Toptan Tekstil%' THEN 3

                        /* Genel tekstil: hızlı Anadolu, ucuz Şanlıurfa, dengeli Bursa */
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Anadolu Tekstil%' THEN 10
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Şanlıurfa Toptan Tekstil%' THEN 11
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Bursa Tekstil%' THEN 12
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%ModaTekstil%' THEN 13
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%İstanbul Hazır Giyim%' THEN 14
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Trend Kumaş%' THEN 15
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Renkli Giyim%' THEN 16
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Minik Stil%' THEN 17
                        WHEN t.FirmaAdi COLLATE Turkish_100_CI_AI LIKE N'%Elit Aksesuar%' THEN 18
                        ELSE 100
                    END,
                    t.Id
            ) AS AdaySirasi
        FROM @HedefUrunler h
        CROSS JOIN dbo.Tedarikciler t
        WHERE t.AktifMi = 1
          AND h.MevcutAktifTedarikciSayisi < 3
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.UrunTedarikcileri mevcut
              WHERE mevcut.UrunId = h.UrunId
                AND mevcut.TedarikciId = t.Id
          )
    ),
    Eklenecekler AS
    (
        SELECT *
        FROM AdayTedarikciler
        WHERE AdaySirasi <= 3 - MevcutAktifTedarikciSayisi
    )
    INSERT INTO dbo.UrunTedarikcileri
    (
        UrunId,
        TedarikciId,
        TedarikciUrunKodu,
        BirimMaliyet,
        IndirimOrani,
        NetBirimMaliyet,
        MinimumSiparisAdedi,
        TeslimSuresiGun,
        VarsayilanMi,
        AktifMi,
        Aciklama,
        OlusturmaTarihi
    )
    SELECT
        e.UrunId,
        e.TedarikciId,
        N'DEMO-' + RIGHT(N'000000' + CAST(e.UrunId AS NVARCHAR(6)), 6)
            + N'-' + RIGHT(N'00' + CAST(e.AdaySirasi AS NVARCHAR(2)), 2),
        CAST(
            ROUND(
                e.AlisFiyati *
                CASE
                    WHEN e.AdaySirasi = 1 THEN 1.00
                    WHEN e.AdaySirasi = 2 THEN 1.05
                    ELSE 1.02
                END,
                2
            ) AS DECIMAL(18,2)
        ) AS BirimMaliyet,
        CAST(
            CASE
                WHEN e.AdaySirasi = 1 THEN 2.00
                WHEN e.AdaySirasi = 2 THEN 12.00
                ELSE 7.00
            END AS DECIMAL(5,2)
        ) AS IndirimOrani,
        CAST(
            ROUND(
                (
                    e.AlisFiyati *
                    CASE
                        WHEN e.AdaySirasi = 1 THEN 1.00
                        WHEN e.AdaySirasi = 2 THEN 1.05
                        ELSE 1.02
                    END
                )
                *
                (
                    1 -
                    (
                        CASE
                            WHEN e.AdaySirasi = 1 THEN 2.00
                            WHEN e.AdaySirasi = 2 THEN 12.00
                            ELSE 7.00
                        END / 100.00
                    )
                ),
                2
            ) AS DECIMAL(18,2)
        ) AS NetBirimMaliyet,
        CASE
            WHEN e.AdaySirasi = 1 THEN 5
            WHEN e.AdaySirasi = 2 THEN 10
            ELSE 3
        END AS MinimumSiparisAdedi,
        CASE
            WHEN e.AdaySirasi = 1 THEN 1
            WHEN e.AdaySirasi = 2 THEN 6
            ELSE 3
        END AS TeslimSuresiGun,
        0 AS VarsayilanMi,
        1 AS AktifMi,
        CASE
            WHEN e.AdaySirasi = 1
                THEN N'023 demo seed: hızlı teslimat alternatifi.'
            WHEN e.AdaySirasi = 2
                THEN N'023 demo seed: düşük net maliyet alternatifi.'
            ELSE N'023 demo seed: dengeli tedarik alternatifi.'
        END,
        GETDATE()
    FROM Eklenecekler e
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.UrunTedarikcileri tekrar
        WHERE tekrar.UrunId = e.UrunId
          AND tekrar.TedarikciId = e.TedarikciId
    );

    /*
      Aynı üründe birden fazla varsayılan işaretlenmişse en eski aktif kayıt
      korunur. Hiç varsayılanı olmayan hedef ürünlerde en eski aktif kayıt
      varsayılan yapılır. Fiyat ve tedarik koşulları değiştirilmez.
    */
    ;WITH VarsayilanSirasi AS
    (
        SELECT
            ut.Id,
            ROW_NUMBER() OVER
            (
                PARTITION BY ut.UrunId
                ORDER BY ut.Id
            ) AS Sira
        FROM dbo.UrunTedarikcileri ut
        WHERE ut.AktifMi = 1
          AND ut.VarsayilanMi = 1
    )
    UPDATE ut
    SET
        ut.VarsayilanMi = 0,
        ut.GuncellemeTarihi = GETDATE()
    FROM dbo.UrunTedarikcileri ut
    INNER JOIN VarsayilanSirasi s ON s.Id = ut.Id
    WHERE s.Sira > 1;

    ;WITH VarsayilansizUrunler AS
    (
        SELECT
            ut.UrunId,
            MIN(ut.Id) AS VarsayilanId
        FROM dbo.UrunTedarikcileri ut
        INNER JOIN @HedefUrunler h ON h.UrunId = ut.UrunId
        WHERE ut.AktifMi = 1
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.UrunTedarikcileri v
              WHERE v.UrunId = ut.UrunId
                AND v.AktifMi = 1
                AND v.VarsayilanMi = 1
          )
        GROUP BY ut.UrunId
    )
    UPDATE ut
    SET
        ut.VarsayilanMi = 1,
        ut.GuncellemeTarihi = GETDATE()
    FROM dbo.UrunTedarikcileri ut
    INNER JOIN VarsayilansizUrunler v ON v.VarsayilanId = ut.Id;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @HataMesaji NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(
        N'Alternatif ürün-tedarikçi demo verileri oluşturulamadı: %s',
        16,
        1,
        @HataMesaji
    );
END CATCH;
GO

/* ================================================================
   KONTROL SORGULARI
   ================================================================ */

SELECT COUNT(*) AS ToplamUrunTedarikciKaydi
FROM dbo.UrunTedarikcileri;

SELECT COUNT(*) AS TeslimSuresiSifirKalanKayit
FROM dbo.UrunTedarikcileri
WHERE ISNULL(TeslimSuresiGun, 0) = 0;

SELECT COUNT(*) AS BirdenFazlaAktifTedarikcisiOlanUrunSayisi
FROM
(
    SELECT UrunId
    FROM dbo.UrunTedarikcileri
    WHERE AktifMi = 1
    GROUP BY UrunId
    HAVING COUNT(*) > 1
) coklu;

SELECT
    u.Id AS UrunId,
    u.UrunAdi,
    t.FirmaAdi,
    ut.BirimMaliyet,
    ut.IndirimOrani,
    ut.NetBirimMaliyet,
    ut.MinimumSiparisAdedi,
    ut.TeslimSuresiGun,
    ut.VarsayilanMi,
    ut.AktifMi
FROM dbo.UrunTedarikcileri ut
INNER JOIN dbo.Urunler u ON u.Id = ut.UrunId
INNER JOIN dbo.Tedarikciler t ON t.Id = ut.TedarikciId
WHERE u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Kadın Hırka%'
ORDER BY u.UrunAdi, ut.NetBirimMaliyet, ut.TeslimSuresiGun;

SELECT
    u.Id AS UrunId,
    u.UrunAdi,
    t.FirmaAdi,
    ut.BirimMaliyet,
    ut.IndirimOrani,
    ut.NetBirimMaliyet,
    ut.MinimumSiparisAdedi,
    ut.TeslimSuresiGun,
    ut.VarsayilanMi,
    ut.AktifMi
FROM dbo.UrunTedarikcileri ut
INNER JOIN dbo.Urunler u ON u.Id = ut.UrunId
INNER JOIN dbo.Tedarikciler t ON t.Id = ut.TedarikciId
WHERE u.UrunAdi COLLATE Turkish_100_CI_AI LIKE N'%Erkek Spor Ceket%'
ORDER BY u.UrunAdi, ut.NetBirimMaliyet, ut.TeslimSuresiGun;

SELECT COUNT(*) AS AlternatifTedarikcisiOlmayanAktifUrunSayisi
FROM dbo.Urunler u
WHERE u.AktifMi = 1
  AND
  (
      SELECT COUNT(*)
      FROM dbo.UrunTedarikcileri ut
      WHERE ut.UrunId = u.Id
        AND ut.AktifMi = 1
  ) < 2;
GO
