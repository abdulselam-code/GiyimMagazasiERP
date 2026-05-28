USE GiyimMagazasiERP;
GO

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.TedarikciAltKategoriler', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.TedarikciAltKategoriler
        (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            TedarikciId INT NOT NULL,
            AltKategoriId INT NOT NULL,
            AktifMi BIT NOT NULL CONSTRAINT DF_TedarikciAltKategoriler_AktifMi DEFAULT 1,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_TedarikciAltKategoriler_OlusturmaTarihi DEFAULT GETDATE()
        );
    END;

    IF OBJECT_ID('dbo.FK_TedarikciAltKategoriler_Tedarikciler', 'F') IS NULL
    BEGIN
        ALTER TABLE dbo.TedarikciAltKategoriler
        ADD CONSTRAINT FK_TedarikciAltKategoriler_Tedarikciler
        FOREIGN KEY (TedarikciId) REFERENCES dbo.Tedarikciler(Id);
    END;

    IF OBJECT_ID('dbo.FK_TedarikciAltKategoriler_AltKategoriler', 'F') IS NULL
    BEGIN
        ALTER TABLE dbo.TedarikciAltKategoriler
        ADD CONSTRAINT FK_TedarikciAltKategoriler_AltKategoriler
        FOREIGN KEY (AltKategoriId) REFERENCES dbo.AltKategoriler(Id);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = 'UX_TedarikciAltKategoriler_TedarikciId_AltKategoriId'
          AND object_id = OBJECT_ID('dbo.TedarikciAltKategoriler')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_TedarikciAltKategoriler_TedarikciId_AltKategoriId
        ON dbo.TedarikciAltKategoriler (TedarikciId, AltKategoriId);
    END;

    INSERT INTO dbo.TedarikciAltKategoriler
    (
        TedarikciId,
        AltKategoriId,
        AktifMi,
        OlusturmaTarihi
    )
    SELECT DISTINCT
        u.TedarikciId,
        u.AltKategoriId,
        1,
        GETDATE()
    FROM dbo.Urunler u
    WHERE u.TedarikciId IS NOT NULL
      AND u.AltKategoriId IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.TedarikciAltKategoriler tak
          WHERE tak.TedarikciId = u.TedarikciId
            AND tak.AltKategoriId = u.AltKategoriId
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

SELECT
    t.FirmaAdi,
    k.KategoriAdi,
    ak.AltKategoriAdi,
    tak.AktifMi,
    tak.OlusturmaTarihi
FROM dbo.TedarikciAltKategoriler tak
INNER JOIN dbo.Tedarikciler t ON t.Id = tak.TedarikciId
INNER JOIN dbo.AltKategoriler ak ON ak.Id = tak.AltKategoriId
INNER JOIN dbo.Kategoriler k ON k.Id = ak.KategoriId
ORDER BY t.FirmaAdi, k.KategoriAdi, ak.AltKategoriAdi;