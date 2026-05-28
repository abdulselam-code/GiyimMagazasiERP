USE GiyimMagazasiERP;
GO

/*
    Dosya:
    Database/Updates/010_fix_last_remaining_alt_kategori_eslestirme.sql

    Amaç:
    - 009 scriptinden sonra boş kalan son ürünleri güvenli şekilde eşleştirir.
    - Sadece AltKategoriId NULL olan ürünleri günceller.
    - Dolu AltKategoriId değerlerine dokunmaz.
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
    -- Eksik olma ihtimaline karşı gerekli alt kategorileri ekle
    ------------------------------------------------------------

    -- Çocuk Giyim > Sweatshirt
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
        N'Sweatshirt',
        N'Son kalan ürün eşleştirme scripti ile eklendi',
        1,
        SYSDATETIME()
    FROM dbo.Kategoriler k
    WHERE k.KategoriAdi COLLATE Turkish_CI_AI = N'Çocuk Giyim' COLLATE Turkish_CI_AI
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.AltKategoriler ak
          WHERE ak.KategoriId = k.Id
            AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Sweatshirt' COLLATE Turkish_CI_AI
      );

    -- İç Giyim > Pijama
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
        N'Pijama',
        N'Son kalan ürün eşleştirme scripti ile eklendi',
        1,
        SYSDATETIME()
    FROM dbo.Kategoriler k
    WHERE k.KategoriAdi COLLATE Turkish_CI_AI = N'İç Giyim' COLLATE Turkish_CI_AI
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.AltKategoriler ak
          WHERE ak.KategoriId = k.Id
            AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Pijama' COLLATE Turkish_CI_AI
      );

    ------------------------------------------------------------
    -- Güncellenecek ürünleri rapor tablosuna al
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
        CASE
            WHEN k.KategoriAdi COLLATE Turkish_CI_AI = N'Çocuk Giyim' COLLATE Turkish_CI_AI
                 AND u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%sweatshirt%' COLLATE Turkish_CI_AI
                THEN N'Çocuk Giyim ürün adında sweatshirt geçtiği için Sweatshirt eşleştirildi.'
            WHEN k.KategoriAdi COLLATE Turkish_CI_AI = N'İç Giyim' COLLATE Turkish_CI_AI
                 AND u.UrunAdi COLLATE Turkish_CI_AI LIKE N'%pijama%' COLLATE Turkish_CI_AI
                THEN N'İç Giyim ürün adında pijama geçtiği için Pijama eşleştirildi.'
            ELSE N'Eşleşme'
        END
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
      );

    ------------------------------------------------------------
    -- Güvenli update
    ------------------------------------------------------------
    UPDATE u
    SET u.AltKategoriId = ak.Id
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
      );

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
-- Kontrol 3: Toplam durum
------------------------------------------------------------
SELECT
    COUNT(*) AS ToplamUrunSayisi,
    SUM(CASE WHEN AltKategoriId IS NOT NULL THEN 1 ELSE 0 END) AS AltKategoriDoluUrunSayisi,
    SUM(CASE WHEN AltKategoriId IS NULL THEN 1 ELSE 0 END) AS AltKategoriBosUrunSayisi
FROM dbo.Urunler;
GO