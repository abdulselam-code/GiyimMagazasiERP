USE GiyimMagazasiERP;
GO
/*
    Dosya:
    Database/Updates/008_fix_alt_kategori_eslestirme.sql

    Amaç:
    - Bayan Giyim uyumluluğunu tamamlar.
    - Kazak / Hırka alt kategorilerini ekler.
    - Sadece AltKategoriId NULL olan ürünleri güvenli şekilde eşleştirir.
    - Mevcut dolu AltKategoriId değerlerine dokunmaz.
*/

SET NOCOUNT ON;

DECLARE @Guncellenen TABLE
(
    UrunId INT,
    UrunAdi NVARCHAR(200),
    AnaKategori NVARCHAR(100),
    AltKategori NVARCHAR(100),
    EslesmeNotu NVARCHAR(250)
);

BEGIN TRY
    BEGIN TRANSACTION;

    ------------------------------------------------------------
    -- 1. Gerekli tablo/kolon kontrolleri
    ------------------------------------------------------------
    IF OBJECT_ID('dbo.Kategoriler', 'U') IS NULL
        THROW 50001, 'Kategoriler tablosu bulunamadı.', 1;

    IF OBJECT_ID('dbo.AltKategoriler', 'U') IS NULL
        THROW 50002, 'AltKategoriler tablosu bulunamadı.', 1;

    IF OBJECT_ID('dbo.Urunler', 'U') IS NULL
        THROW 50003, 'Urunler tablosu bulunamadı.', 1;

    IF COL_LENGTH('dbo.Urunler', 'AltKategoriId') IS NULL
        THROW 50004, 'Urunler.AltKategoriId kolonu bulunamadı.', 1;


    ------------------------------------------------------------
    -- 2. Eksik alt kategorileri güvenli şekilde ekle
    ------------------------------------------------------------
    DECLARE @EklenecekAltKategoriler TABLE
    (
        KategoriAdi NVARCHAR(100),
        AltKategoriAdi NVARCHAR(100)
    );

    INSERT INTO @EklenecekAltKategoriler (KategoriAdi, AltKategoriAdi)
    VALUES
        ('Bay Giyim', 'Kazak'),
        ('Bay Giyim', 'Hırka'),

        ('Bayan Giyim', 'Elbise'),
        ('Bayan Giyim', 'Bluz'),
        ('Bayan Giyim', 'Gömlek'),
        ('Bayan Giyim', 'Tişört'),
        ('Bayan Giyim', 'Pantolon'),
        ('Bayan Giyim', 'Etek'),
        ('Bayan Giyim', 'Ceket'),
        ('Bayan Giyim', 'Mont'),
        ('Bayan Giyim', 'Tunik'),
        ('Bayan Giyim', 'Eşofman'),
        ('Bayan Giyim', 'Kazak'),
        ('Bayan Giyim', 'Hırka'),

        ('Kadın Giyim', 'Kazak'),
        ('Kadın Giyim', 'Hırka'),

        ('Çocuk Giyim', 'Kazak'),
        ('Çocuk Giyim', 'Hırka');

    INSERT INTO dbo.AltKategoriler
    (
        KategoriId,
        AltKategoriAdi,
        Aciklama,
        AktifMi,
        OlusturmaTarihi
    )
    SELECT
        k.Id,
        e.AltKategoriAdi,
        N'Düzeltme scripti ile eklendi',
        1,
        SYSDATETIME()
    FROM @EklenecekAltKategoriler e
    INNER JOIN dbo.Kategoriler k
        ON k.KategoriAdi COLLATE Turkish_CI_AI = e.KategoriAdi COLLATE Turkish_CI_AI
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.AltKategoriler ak
        WHERE ak.KategoriId = k.Id
          AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = e.AltKategoriAdi COLLATE Turkish_CI_AI
    );


    ------------------------------------------------------------
    -- 3. Ürün adı kurallarını hazırla
    -- Oncelik küçük olan daha güçlü/eşleşmesi daha özel kabul edilir.
    ------------------------------------------------------------
    DECLARE @Kurallar TABLE
    (
        AltKategoriAdi NVARCHAR(100),
        LikePattern NVARCHAR(100),
        EslesmeNotu NVARCHAR(250),
        Oncelik INT
    );

    INSERT INTO @Kurallar
    (
        AltKategoriAdi,
        LikePattern,
        EslesmeNotu,
        Oncelik
    )
    VALUES
        ('Hırka', '%hırka%', 'Ürün adında hırka geçtiği için Hırka eşleştirildi.', 10),
        ('Hırka', '%hirka%', 'Ürün adında hirka geçtiği için Hırka eşleştirildi.', 11),

        ('Kazak', '%kazak%', 'Ürün adında kazak geçtiği için Kazak eşleştirildi.', 20),

        ('Gömlek', '%gömlek%', 'Ürün adında gömlek geçtiği için Gömlek eşleştirildi.', 30),
        ('Gömlek', '%gomlek%', 'Ürün adında gomlek geçtiği için Gömlek eşleştirildi.', 31),

        ('Bluz', '%bluz%', 'Ürün adında bluz geçtiği için Bluz eşleştirildi.', 40),

        ('Pantolon', '%pantolon%', 'Ürün adında pantolon geçtiği için Pantolon eşleştirildi.', 50),
        ('Pantolon', '%jean%', 'Ürün adında jean geçtiği için Pantolon eşleştirildi.', 51),
        ('Pantolon', '%kot%', 'Ürün adında kot geçtiği için Pantolon eşleştirildi.', 52),

        ('Tişört', '%tişört%', 'Ürün adında tişört geçtiği için Tişört eşleştirildi.', 60),
        ('Tişört', '%tisort%', 'Ürün adında tisort geçtiği için Tişört eşleştirildi.', 61),
        ('Tişört', '%tshirt%', 'Ürün adında tshirt geçtiği için Tişört eşleştirildi.', 62),
        ('Tişört', '%t-shirt%', 'Ürün adında t-shirt geçtiği için Tişört eşleştirildi.', 63),

        ('Etek', '%etek%', 'Ürün adında etek geçtiği için Etek eşleştirildi.', 70),

        ('Elbise', '%elbise%', 'Ürün adında elbise geçtiği için Elbise eşleştirildi.', 80),

        ('Tunik', '%tunik%', 'Ürün adında tunik geçtiği için Tunik eşleştirildi.', 90),

        ('Eşofman', '%eşofman%', 'Ürün adında eşofman geçtiği için Eşofman eşleştirildi.', 100),
        ('Eşofman', '%esofman%', 'Ürün adında esofman geçtiği için Eşofman eşleştirildi.', 101),

        ('Ceket', '%ceket%', 'Ürün adında ceket geçtiği için Ceket eşleştirildi.', 110),

        ('Mont', '%mont%', 'Ürün adında mont geçtiği için Mont eşleştirildi.', 120),
        ('Mont', '%kaban%', 'Ürün adında kaban geçtiği için Mont eşleştirildi.', 121);


    ------------------------------------------------------------
    -- 4. Aday eşleşmeleri çıkar
    ------------------------------------------------------------
    DECLARE @AdayEslesmeler TABLE
    (
        UrunId INT,
        YeniAltKategoriId INT,
        EslesmeNotu NVARCHAR(250),
        Oncelik INT
    );

    INSERT INTO @AdayEslesmeler
    (
        UrunId,
        YeniAltKategoriId,
        EslesmeNotu,
        Oncelik
    )
    SELECT
        u.Id,
        ak.Id,
        r.EslesmeNotu,
        r.Oncelik
    FROM dbo.Urunler u
    INNER JOIN dbo.AltKategoriler ak
        ON ak.KategoriId = u.KategoriId
    INNER JOIN @Kurallar r
        ON ak.AltKategoriAdi COLLATE Turkish_CI_AI = r.AltKategoriAdi COLLATE Turkish_CI_AI
       AND u.UrunAdi COLLATE Turkish_CI_AI LIKE r.LikePattern COLLATE Turkish_CI_AI
    WHERE u.AltKategoriId IS NULL;


    ------------------------------------------------------------
    -- 5. Aynı ürün birden fazla kurala uyarsa en öncelikli olanı seç
    ------------------------------------------------------------
    DECLARE @SecilenEslesmeler TABLE
    (
        UrunId INT PRIMARY KEY,
        YeniAltKategoriId INT,
        EslesmeNotu NVARCHAR(250)
    );

    INSERT INTO @SecilenEslesmeler
    (
        UrunId,
        YeniAltKategoriId,
        EslesmeNotu
    )
    SELECT
        x.UrunId,
        x.YeniAltKategoriId,
        x.EslesmeNotu
    FROM
    (
        SELECT
            a.UrunId,
            a.YeniAltKategoriId,
            a.EslesmeNotu,
            ROW_NUMBER() OVER
            (
                PARTITION BY a.UrunId
                ORDER BY a.Oncelik ASC, a.YeniAltKategoriId ASC
            ) AS Sira
        FROM @AdayEslesmeler a
    ) x
    WHERE x.Sira = 1;


    ------------------------------------------------------------
    -- 6. Sadece AltKategoriId NULL olan ürünleri güncelle
    ------------------------------------------------------------
    UPDATE u
    SET u.AltKategoriId = s.YeniAltKategoriId
    FROM dbo.Urunler u
    INNER JOIN @SecilenEslesmeler s
        ON s.UrunId = u.Id
    WHERE u.AltKategoriId IS NULL;


    ------------------------------------------------------------
    -- 7. Bu script ile güncellenen ürünleri raporlamak için kaydet
    ------------------------------------------------------------
    INSERT INTO @Guncellenen
    (
        UrunId,
        UrunAdi,
        AnaKategori,
        AltKategori,
        EslesmeNotu
    )
    SELECT
        u.Id,
        u.UrunAdi,
        k.KategoriAdi,
        ak.AltKategoriAdi,
        s.EslesmeNotu
    FROM @SecilenEslesmeler s
    INNER JOIN dbo.Urunler u
        ON u.Id = s.UrunId
       AND u.AltKategoriId = s.YeniAltKategoriId
    INNER JOIN dbo.Kategoriler k
        ON k.Id = u.KategoriId
    INNER JOIN dbo.AltKategoriler ak
        ON ak.Id = u.AltKategoriId;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT
        ERROR_NUMBER() AS HataNo,
        ERROR_MESSAGE() AS HataMesaji;

    RETURN;
END CATCH;


------------------------------------------------------------
-- KONTROL 1:
-- Bu script ile güncellenen ürünler
------------------------------------------------------------
SELECT
    UrunId,
    UrunAdi,
    AnaKategori,
    AltKategori,
    EslesmeNotu
FROM @Guncellenen
ORDER BY AnaKategori, AltKategori, UrunAdi;


------------------------------------------------------------
-- KONTROL 2:
-- Hâlâ AltKategoriId boş kalan ürünler
------------------------------------------------------------
SELECT
    u.Id AS UrunId,
    u.UrunAdi,
    k.KategoriAdi AS AnaKategori
FROM dbo.Urunler u
LEFT JOIN dbo.Kategoriler k
    ON k.Id = u.KategoriId
WHERE u.AltKategoriId IS NULL
ORDER BY k.KategoriAdi, u.UrunAdi;


------------------------------------------------------------
-- KONTROL 3:
-- Alt kategori dağılımı
------------------------------------------------------------
SELECT
    k.KategoriAdi AS AnaKategori,
    ak.AltKategoriAdi AS AltKategori,
    COUNT(u.Id) AS UrunSayisi
FROM dbo.Urunler u
INNER JOIN dbo.Kategoriler k
    ON k.Id = u.KategoriId
INNER JOIN dbo.AltKategoriler ak
    ON ak.Id = u.AltKategoriId
GROUP BY
    k.KategoriAdi,
    ak.AltKategoriAdi
ORDER BY
    k.KategoriAdi,
    ak.AltKategoriAdi;


------------------------------------------------------------
-- KONTROL 4:
-- Toplam durum
------------------------------------------------------------
SELECT
    COUNT(*) AS ToplamUrunSayisi,
    SUM(CASE WHEN AltKategoriId IS NOT NULL THEN 1 ELSE 0 END) AS AltKategoriDoluUrunSayisi,
    SUM(CASE WHEN AltKategoriId IS NULL THEN 1 ELSE 0 END) AS AltKategoriBosUrunSayisi
FROM dbo.Urunler;