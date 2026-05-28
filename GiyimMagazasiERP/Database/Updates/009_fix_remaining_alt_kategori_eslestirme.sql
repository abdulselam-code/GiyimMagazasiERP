USE GiyimMagazasiERP;
GO

/*
    Dosya:
    Database/Updates/009_fix_remaining_alt_kategori_eslestirme.sql

    Amaç:
    - 008 scriptinden sonra boş kalan birkaç ürünü güvenli şekilde eşleştirir.
    - Sadece AltKategoriId NULL olan ürünleri günceller.
    - Mevcut dolu AltKategoriId değerlerine dokunmaz.
    - İç Giyim altına eksikse "Çorap" alt kategorisi ekler.
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
    -- Güvenlik kontrolleri
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
    -- İç Giyim altında Çorap alt kategorisi yoksa ekle
    ------------------------------------------------------------
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
        N'Çorap',
        N'Kalan ürün eşleştirme scripti ile eklendi',
        1,
        SYSDATETIME()
    FROM dbo.Kategoriler k
    WHERE k.KategoriAdi COLLATE Turkish_CI_AI = N'İç Giyim' COLLATE Turkish_CI_AI
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.AltKategoriler ak
          WHERE ak.KategoriId = k.Id
            AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Çorap' COLLATE Turkish_CI_AI
      );

    ------------------------------------------------------------
    -- Aday eşleşmeler
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
        CASE
            WHEN u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%sweatshirt%' COLLATE Turkish_CI_AI
                THEN N'Çocuk Giyim ürün adında sweatshirt geçtiği için Sweatshirt eşleştirildi.'
            WHEN u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%pijama%' COLLATE Turkish_CI_AI
                THEN N'İç Giyim ürün adında pijama geçtiği için Pijama eşleştirildi.'
            WHEN u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%çorap%' COLLATE Turkish_CI_AI
              OR u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%corap%' COLLATE Turkish_CI_AI
                THEN N'İç Giyim ürün adında çorap geçtiği için Çorap eşleştirildi.'
            ELSE N'Eşleşme'
        END AS EslesmeNotu,
        CASE
            WHEN u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%sweatshirt%' COLLATE Turkish_CI_AI THEN 10
            WHEN u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%pijama%' COLLATE Turkish_CI_AI THEN 20
            WHEN u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%çorap%' COLLATE Turkish_CI_AI
              OR u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%corap%' COLLATE Turkish_CI_AI THEN 30
            ELSE 999
        END AS Oncelik
    FROM dbo.Urunler u
    INNER JOIN dbo.Kategoriler k
        ON k.Id = u.KategoriId
    INNER JOIN dbo.AltKategoriler ak
        ON ak.KategoriId = u.KategoriId
    WHERE u.AltKategoriId IS NULL
      AND
      (
          (
              k.KategoriAdi COLLATE Turkish_CI_AI = N'Çocuk Giyim' COLLATE Turkish_CI_AI
              AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Sweatshirt' COLLATE Turkish_CI_AI
              AND u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%sweatshirt%' COLLATE Turkish_CI_AI
          )
          OR
          (
              k.KategoriAdi COLLATE Turkish_CI_AI = N'İç Giyim' COLLATE Turkish_CI_AI
              AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Pijama' COLLATE Turkish_CI_AI
              AND u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%pijama%' COLLATE Turkish_CI_AI
          )
          OR
          (
              k.KategoriAdi COLLATE Turkish_CI_AI = N'İç Giyim' COLLATE Turkish_CI_AI
              AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Çorap' COLLATE Turkish_CI_AI
              AND
              (
                  u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%çorap%' COLLATE Turkish_CI_AI
                  OR u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%corap%' COLLATE Turkish_CI_AI
              )
          )
      );

    ------------------------------------------------------------
    -- Aynı ürün için birden fazla eşleşme çıkarsa en öncelikli olanı seç
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
    -- Raporlama için güncellenecek ürünleri kaydet
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
    INNER JOIN dbo.Kategoriler k
        ON k.Id = u.KategoriId
    INNER JOIN dbo.AltKategoriler ak
        ON ak.Id = s.YeniAltKategoriId
    WHERE u.AltKategoriId IS NULL;

    ------------------------------------------------------------
    -- Güvenli update
    ------------------------------------------------------------
    UPDATE u
    SET u.AltKategoriId = s.YeniAltKategoriId
    FROM dbo.Urunler u
    INNER JOIN @SecilenEslesmeler s
        ON s.UrunId = u.Id
    INNER JOIN dbo.AltKategoriler ak
        ON ak.Id = s.YeniAltKategoriId
       AND ak.KategoriId = u.KategoriId
    WHERE u.AltKategoriId IS NULL;

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
-- Kontrol 1: Bu script ile güncellenen ürünler
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
-- Kontrol 2: Hâlâ AltKategoriId boş kalan ürünler
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
-- Kontrol 3: Alt kategori dağılımı
------------------------------------------------------------
SELECT
    k.KategoriAdi AS AnaKategori,
    ak.AltKategoriAdi AS AltKategori,
    COUNT(u.Id) AS UrunSayisi
FROM dbo.Urunler u
INNER JOIN dbo.Kategoriler k
    ON k.Id = u.KategoriId
LEFT JOIN dbo.AltKategoriler ak
    ON ak.Id = u.AltKategoriId
GROUP BY
    k.KategoriAdi,
    ak.AltKategoriAdi
ORDER BY
    k.KategoriAdi,
    ak.AltKategoriAdi;

------------------------------------------------------------
-- Kontrol 4: Toplam durum
------------------------------------------------------------
SELECT
    COUNT(*) AS ToplamUrunSayisi,
    SUM(CASE WHEN AltKategoriId IS NOT NULL THEN 1 ELSE 0 END) AS AltKategoriDoluUrunSayisi,
    SUM(CASE WHEN AltKategoriId IS NULL THEN 1 ELSE 0 END) AS AltKategoriBosUrunSayisi
FROM dbo.Urunler;
GO