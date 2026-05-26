USE GiyimMagazasiERP;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.MagazaBilgileri', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.MagazaBilgileri
        (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            MagazaAdi NVARCHAR(150) NOT NULL,
            Adres NVARCHAR(300) NULL,
            Telefon NVARCHAR(30) NULL,
            Email NVARCHAR(120) NULL,
            VergiDairesi NVARCHAR(100) NULL,
            VergiNo NVARCHAR(30) NULL,
            KurulusTarihi DATE NULL,
            AktifMi BIT NOT NULL CONSTRAINT DF_MagazaBilgileri_AktifMi DEFAULT 1
        );
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.MagazaBilgileri WHERE AktifMi = 1)
    BEGIN
        INSERT INTO dbo.MagazaBilgileri
        (
            MagazaAdi,
            Adres,
            Telefon,
            Email,
            VergiDairesi,
            VergiNo,
            KurulusTarihi,
            AktifMi
        )
        VALUES
        (
            N'Giyim Mağazası ERP',
            N'Merkez / Türkiye',
            N'0555 000 00 00',
            N'info@giyimerp.local',
            N'Merkez',
            N'0000000000',
            '2026-01-01',
            1
        );
    END

    IF COL_LENGTH('dbo.Satislar', 'SatisTuru') IS NULL
    BEGIN
        ALTER TABLE dbo.Satislar
        ADD SatisTuru NVARCHAR(20) NULL;
    END

    UPDATE dbo.Satislar
    SET SatisTuru = N'Perakende'
    WHERE SatisTuru IS NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT ERROR_MESSAGE() AS HataMesaji;
END CATCH;
GO