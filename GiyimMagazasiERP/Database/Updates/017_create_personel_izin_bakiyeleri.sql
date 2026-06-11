USE GiyimMagazasiERP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.PersonelIzinBakiyeleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PersonelIzinBakiyeleri
        (
            Id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PersonelIzinBakiyeleri PRIMARY KEY,
            PersonelId INT NOT NULL,
            Yil INT NOT NULL,
            YillikIzinHakki DECIMAL(5,2) NOT NULL
                CONSTRAINT DF_PersonelIzinBakiyeleri_YillikIzinHakki DEFAULT (14),
            DevredenIzinGunu DECIMAL(5,2) NOT NULL
                CONSTRAINT DF_PersonelIzinBakiyeleri_DevredenIzinGunu DEFAULT (0),
            OlusturmaTarihi DATETIME2 NOT NULL
                CONSTRAINT DF_PersonelIzinBakiyeleri_OlusturmaTarihi DEFAULT (GETDATE()),
            GuncellemeTarihi DATETIME2 NULL,
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT FK_PersonelIzinBakiyeleri_Personeller
                FOREIGN KEY (PersonelId) REFERENCES dbo.Personeller(Id),
            CONSTRAINT CK_PersonelIzinBakiyeleri_Yil
                CHECK (Yil BETWEEN 1900 AND 2200),
            CONSTRAINT CK_PersonelIzinBakiyeleri_Hak
                CHECK (YillikIzinHakki >= 0),
            CONSTRAINT CK_PersonelIzinBakiyeleri_Devreden
                CHECK (DevredenIzinGunu >= 0)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.PersonelIzinBakiyeleri')
          AND name = N'UX_PersonelIzinBakiyeleri_PersonelId_Yil'
    )
    BEGIN
        CREATE UNIQUE INDEX UX_PersonelIzinBakiyeleri_PersonelId_Yil
            ON dbo.PersonelIzinBakiyeleri(PersonelId, Yil);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.PersonelIzinBakiyeleri')
          AND name = N'IX_PersonelIzinBakiyeleri_PersonelId'
    )
    BEGIN
        CREATE INDEX IX_PersonelIzinBakiyeleri_PersonelId
            ON dbo.PersonelIzinBakiyeleri(PersonelId);
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE object_id = OBJECT_ID(N'dbo.PersonelIzinBakiyeleri')
          AND name = N'IX_PersonelIzinBakiyeleri_Yil'
    )
    BEGIN
        CREATE INDEX IX_PersonelIzinBakiyeleri_Yil
            ON dbo.PersonelIzinBakiyeleri(Yil);
    END;

    DECLARE @Yil INT = YEAR(GETDATE());

    INSERT INTO dbo.PersonelIzinBakiyeleri
    (
        PersonelId,
        Yil,
        YillikIzinHakki,
        DevredenIzinGunu,
        OlusturmaTarihi
    )
    SELECT
        p.Id,
        @Yil,
        CAST(14 AS DECIMAL(5,2)),
        CAST(0 AS DECIMAL(5,2)),
        GETDATE()
    FROM dbo.Personeller AS p
    WHERE p.AktifMi = 1
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.PersonelIzinBakiyeleri AS b
          WHERE b.PersonelId = p.Id
            AND b.Yil = @Yil
      );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @HataMesaji NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Personel izin bakiyesi kurulamadı: %s', 16, 1, @HataMesaji);
END CATCH;
GO

DECLARE @KontrolYili INT = YEAR(GETDATE());
DECLARE @YilBaslangici DATE = DATEFROMPARTS(@KontrolYili, 1, 1);
DECLARE @SonrakiYilBaslangici DATE = DATEFROMPARTS(@KontrolYili + 1, 1, 1);

SELECT
    p.AdSoyad AS Personel,
    b.Yil,
    b.YillikIzinHakki,
    b.DevredenIzinGunu,
    CAST(COALESCE(k.KullanilanIzinGunu, 0) AS DECIMAL(5,2)) AS KullanilanIzinGunu,
    CAST(
        b.YillikIzinHakki + b.DevredenIzinGunu -
        COALESCE(k.KullanilanIzinGunu, 0)
        AS DECIMAL(6,2)
    ) AS KalanIzinGunu
FROM dbo.PersonelIzinBakiyeleri AS b
INNER JOIN dbo.Personeller AS p
    ON p.Id = b.PersonelId
OUTER APPLY
(
    SELECT SUM(
        DATEDIFF(
            DAY,
            CASE
                WHEN i.BaslangicTarihi < @YilBaslangici
                    THEN @YilBaslangici
                ELSE CAST(i.BaslangicTarihi AS DATE)
            END,
            CASE
                WHEN i.BitisTarihi >= @SonrakiYilBaslangici
                    THEN DATEADD(DAY, -1, @SonrakiYilBaslangici)
                ELSE CAST(i.BitisTarihi AS DATE)
            END
        ) + 1
    ) AS KullanilanIzinGunu
    FROM dbo.PersonelIzinleri AS i
    WHERE i.PersonelId = b.PersonelId
      AND i.IzinTuru = N'Yıllık İzin'
      AND i.Durum = N'Onaylandi'
      AND i.BaslangicTarihi < @SonrakiYilBaslangici
      AND i.BitisTarihi >= @YilBaslangici
) AS k
WHERE b.Yil = @KontrolYili
ORDER BY p.AdSoyad;
GO
