USE GiyimMagazasiERP;
GO

/*
    Dosya:
    Database/Updates/011_fix_final_two_alt_kategori_eslestirme.sql

    Amaç:
    - Son kalan 2 ürünü güvenli şekilde alt kategoriye bağlar.
    - Sadece AltKategoriId NULL olan ürünlere dokunur.
    - Ürün Id bazlı çalışır.
*/

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    ------------------------------------------------------------
    -- UrunId 3: Çocuk Sweatshirt
    -- Ürünün kendi ana kategorisi altında Sweatshirt yoksa ekle
    ------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM dbo.Urunler
        WHERE Id = 3
          AND AltKategoriId IS NULL
    )
    BEGIN
        INSERT INTO dbo.AltKategoriler
        (
            KategoriId,
            AltKategoriAdi,
            Aciklama,
            AktifMi,
            OlusturmaTarihi
        )
        SELECT
            u.KategoriId,
            N'Sweatshirt',
            N'Son iki ürün eşleştirme scripti ile eklendi',
            1,
            SYSDATETIME()
        FROM dbo.Urunler u
        WHERE u.Id = 3
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.AltKategoriler ak
              WHERE ak.KategoriId = u.KategoriId
                AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Sweatshirt' COLLATE Turkish_CI_AI
          );

        UPDATE u
        SET AltKategoriId = ak.Id
        FROM dbo.Urunler u
        INNER JOIN dbo.AltKategoriler ak
            ON ak.KategoriId = u.KategoriId
           AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Sweatshirt' COLLATE Turkish_CI_AI
        WHERE u.Id = 3
          AND u.AltKategoriId IS NULL;
    END

    ------------------------------------------------------------
    -- UrunId 4: Kadın Pijama Takımı
    -- Ürünün kendi ana kategorisi altında Pijama yoksa ekle
    ------------------------------------------------------------
    IF EXISTS (
        SELECT 1
        FROM dbo.Urunler
        WHERE Id = 4
          AND AltKategoriId IS NULL
    )
    BEGIN
        INSERT INTO dbo.AltKategoriler
        (
            KategoriId,
            AltKategoriAdi,
            Aciklama,
            AktifMi,
            OlusturmaTarihi
        )
        SELECT
            u.KategoriId,
            N'Pijama',
            N'Son iki ürün eşleştirme scripti ile eklendi',
            1,
            SYSDATETIME()
        FROM dbo.Urunler u
        WHERE u.Id = 4
          AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.AltKategoriler ak
              WHERE ak.KategoriId = u.KategoriId
                AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Pijama' COLLATE Turkish_CI_AI
          );

        UPDATE u
        SET AltKategoriId = ak.Id
        FROM dbo.Urunler u
        INNER JOIN dbo.AltKategoriler ak
            ON ak.KategoriId = u.KategoriId
           AND ak.AltKategoriAdi COLLATE Turkish_CI_AI = N'Pijama' COLLATE Turkish_CI_AI
        WHERE u.Id = 4
          AND u.AltKategoriId IS NULL;
    END

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
-- Kontrol 1: Bu iki ürünün son durumu
------------------------------------------------------------
SELECT
    u.Id AS UrunId,
    u.UrunAdi,
    k.KategoriAdi AS AnaKategori,
    ak.AltKategoriAdi AS AltKategori
FROM dbo.Urunler u
INNER JOIN dbo.Kategoriler k ON k.Id = u.KategoriId
LEFT JOIN dbo.AltKategoriler ak ON ak.Id = u.AltKategoriId
WHERE u.Id IN (3, 4)
ORDER BY u.Id;

------------------------------------------------------------
-- Kontrol 2: Hâlâ AltKategoriId boş kalan ürünler
------------------------------------------------------------
SELECT
    u.Id AS UrunId,
    u.UrunAdi,
    k.KategoriAdi AS AnaKategori
FROM dbo.Urunler u
LEFT JOIN dbo.Kategoriler k ON k.Id = u.KategoriId
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