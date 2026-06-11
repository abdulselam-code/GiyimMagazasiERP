USE GiyimMagazasiERP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.PersonelIzinleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PersonelIzinleri
        (
            Id INT IDENTITY(1,1) NOT NULL,
            PersonelId INT NOT NULL,
            KullaniciId INT NOT NULL,
            IzinTuru NVARCHAR(50) NOT NULL,
            BaslangicTarihi DATE NOT NULL,
            BitisTarihi DATE NOT NULL,
            GunSayisi INT NOT NULL,
            Aciklama NVARCHAR(500) NULL,
            Durum NVARCHAR(30) NOT NULL
                CONSTRAINT DF_PersonelIzinleri_Durum DEFAULT (N'OnayBekliyor'),
            OnaylayanKullaniciId INT NULL,
            OnayTarihi DATETIME2 NULL,
            RedNedeni NVARCHAR(500) NULL,
            IptalTarihi DATETIME2 NULL,
            OlusturmaTarihi DATETIME2 NOT NULL
                CONSTRAINT DF_PersonelIzinleri_OlusturmaTarihi DEFAULT (GETDATE()),
            GuncellemeTarihi DATETIME2 NULL,
            RowVersion ROWVERSION NOT NULL,

            CONSTRAINT PK_PersonelIzinleri PRIMARY KEY (Id)
        );
    END;

    IF COL_LENGTH(N'dbo.PersonelIzinleri', N'GuncellemeTarihi') IS NULL
        ALTER TABLE dbo.PersonelIzinleri ADD GuncellemeTarihi DATETIME2 NULL;

    IF COL_LENGTH(N'dbo.PersonelIzinleri', N'IptalTarihi') IS NULL
        ALTER TABLE dbo.PersonelIzinleri ADD IptalTarihi DATETIME2 NULL;

    IF COL_LENGTH(N'dbo.PersonelIzinleri', N'RedNedeni') IS NULL
        ALTER TABLE dbo.PersonelIzinleri ADD RedNedeni NVARCHAR(500) NULL;

    IF COL_LENGTH(N'dbo.PersonelIzinleri', N'OnaylayanKullaniciId') IS NULL
        ALTER TABLE dbo.PersonelIzinleri ADD OnaylayanKullaniciId INT NULL;

    IF COL_LENGTH(N'dbo.PersonelIzinleri', N'OnayTarihi') IS NULL
        ALTER TABLE dbo.PersonelIzinleri ADD OnayTarihi DATETIME2 NULL;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_PersonelIzinleri_TarihAraligi'
    )
    BEGIN
        ALTER TABLE dbo.PersonelIzinleri WITH CHECK
        ADD CONSTRAINT CK_PersonelIzinleri_TarihAraligi
        CHECK (BaslangicTarihi <= BitisTarihi AND GunSayisi >= 1);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_PersonelIzinleri_Durum'
    )
    BEGIN
        ALTER TABLE dbo.PersonelIzinleri WITH CHECK
        ADD CONSTRAINT CK_PersonelIzinleri_Durum
        CHECK
        (
            Durum IN
            (
                N'OnayBekliyor',
                N'Onaylandi',
                N'Reddedildi',
                N'IptalEdildi'
            )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_PersonelIzinleri_Personeller_PersonelId'
    )
    BEGIN
        ALTER TABLE dbo.PersonelIzinleri WITH CHECK
        ADD CONSTRAINT FK_PersonelIzinleri_Personeller_PersonelId
        FOREIGN KEY (PersonelId) REFERENCES dbo.Personeller(Id);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_PersonelIzinleri_Kullanicilar_KullaniciId'
    )
    BEGIN
        ALTER TABLE dbo.PersonelIzinleri WITH CHECK
        ADD CONSTRAINT FK_PersonelIzinleri_Kullanicilar_KullaniciId
        FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(Id);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_PersonelIzinleri_Kullanicilar_OnaylayanKullaniciId'
    )
    BEGIN
        ALTER TABLE dbo.PersonelIzinleri WITH CHECK
        ADD CONSTRAINT FK_PersonelIzinleri_Kullanicilar_OnaylayanKullaniciId
        FOREIGN KEY (OnaylayanKullaniciId) REFERENCES dbo.Kullanicilar(Id);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelIzinleri_PersonelId'
          AND object_id = OBJECT_ID(N'dbo.PersonelIzinleri')
    )
        CREATE INDEX IX_PersonelIzinleri_PersonelId
            ON dbo.PersonelIzinleri(PersonelId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelIzinleri_KullaniciId'
          AND object_id = OBJECT_ID(N'dbo.PersonelIzinleri')
    )
        CREATE INDEX IX_PersonelIzinleri_KullaniciId
            ON dbo.PersonelIzinleri(KullaniciId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelIzinleri_Durum'
          AND object_id = OBJECT_ID(N'dbo.PersonelIzinleri')
    )
        CREATE INDEX IX_PersonelIzinleri_Durum
            ON dbo.PersonelIzinleri(Durum);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelIzinleri_TarihAraligi'
          AND object_id = OBJECT_ID(N'dbo.PersonelIzinleri')
    )
        CREATE INDEX IX_PersonelIzinleri_TarihAraligi
            ON dbo.PersonelIzinleri(BaslangicTarihi, BitisTarihi);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelIzinleri_OlusturmaTarihi'
          AND object_id = OBJECT_ID(N'dbo.PersonelIzinleri')
    )
        CREATE INDEX IX_PersonelIzinleri_OlusturmaTarihi
            ON dbo.PersonelIzinleri(OlusturmaTarihi DESC);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @HataMesaji NVARCHAR(2048) = ERROR_MESSAGE();
    RAISERROR(N'Personel izinleri scripti başarısız oldu: %s', 16, 1, @HataMesaji);
END CATCH;
GO

SELECT
    OBJECT_ID(N'dbo.PersonelIzinleri', N'U') AS PersonelIzinleriTabloId,
    (SELECT COUNT(*) FROM dbo.PersonelIzinleri) AS ToplamIzinKaydi;
GO
