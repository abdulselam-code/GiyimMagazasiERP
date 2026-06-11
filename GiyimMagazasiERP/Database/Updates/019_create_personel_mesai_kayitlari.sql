USE GiyimMagazasiERP;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.PersonelMesaiKayitlari', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.PersonelMesaiKayitlari
        (
            Id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_PersonelMesaiKayitlari PRIMARY KEY,
            PersonelId INT NOT NULL,
            KullaniciId INT NOT NULL,
            Tarih DATE NOT NULL,
            VardiyaBaslangic TIME(0) NOT NULL,
            VardiyaBitis TIME(0) NOT NULL,
            GercekGiris TIME(0) NULL,
            GercekCikis TIME(0) NULL,
            PlanlananSaat DECIMAL(5,2) NOT NULL
                CONSTRAINT DF_PersonelMesaiKayitlari_PlanlananSaat DEFAULT (0),
            GerceklesenSaat DECIMAL(5,2) NOT NULL
                CONSTRAINT DF_PersonelMesaiKayitlari_GerceklesenSaat DEFAULT (0),
            FazlaMesaiSaati DECIMAL(5,2) NOT NULL
                CONSTRAINT DF_PersonelMesaiKayitlari_FazlaMesaiSaati DEFAULT (0),
            MesaiTuru NVARCHAR(50) NOT NULL,
            Durum NVARCHAR(30) NOT NULL
                CONSTRAINT DF_PersonelMesaiKayitlari_Durum DEFAULT (N'OnayBekliyor'),
            Aciklama NVARCHAR(500) NULL,
            OnaylayanKullaniciId INT NULL,
            OnayTarihi DATETIME2 NULL,
            RedNedeni NVARCHAR(500) NULL,
            IptalTarihi DATETIME2 NULL,
            OlusturmaTarihi DATETIME2 NOT NULL
                CONSTRAINT DF_PersonelMesaiKayitlari_OlusturmaTarihi DEFAULT (GETDATE()),
            GuncellemeTarihi DATETIME2 NULL,
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT FK_PersonelMesaiKayitlari_Personeller
                FOREIGN KEY (PersonelId) REFERENCES dbo.Personeller(Id),
            CONSTRAINT FK_PersonelMesaiKayitlari_Kullanicilar
                FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(Id),
            CONSTRAINT FK_PersonelMesaiKayitlari_OnaylayanKullanicilar
                FOREIGN KEY (OnaylayanKullaniciId) REFERENCES dbo.Kullanicilar(Id)
        );
    END;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelMesaiKayitlari_PersonelId'
          AND object_id = OBJECT_ID(N'dbo.PersonelMesaiKayitlari'))
        CREATE INDEX IX_PersonelMesaiKayitlari_PersonelId
            ON dbo.PersonelMesaiKayitlari(PersonelId);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelMesaiKayitlari_KullaniciId'
          AND object_id = OBJECT_ID(N'dbo.PersonelMesaiKayitlari'))
        CREATE INDEX IX_PersonelMesaiKayitlari_KullaniciId
            ON dbo.PersonelMesaiKayitlari(KullaniciId);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelMesaiKayitlari_Tarih'
          AND object_id = OBJECT_ID(N'dbo.PersonelMesaiKayitlari'))
        CREATE INDEX IX_PersonelMesaiKayitlari_Tarih
            ON dbo.PersonelMesaiKayitlari(Tarih);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelMesaiKayitlari_Durum'
          AND object_id = OBJECT_ID(N'dbo.PersonelMesaiKayitlari'))
        CREATE INDEX IX_PersonelMesaiKayitlari_Durum
            ON dbo.PersonelMesaiKayitlari(Durum);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelMesaiKayitlari_MesaiTuru'
          AND object_id = OBJECT_ID(N'dbo.PersonelMesaiKayitlari'))
        CREATE INDEX IX_PersonelMesaiKayitlari_MesaiTuru
            ON dbo.PersonelMesaiKayitlari(MesaiTuru);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelMesaiKayitlari_OlusturmaTarihi'
          AND object_id = OBJECT_ID(N'dbo.PersonelMesaiKayitlari'))
        CREATE INDEX IX_PersonelMesaiKayitlari_OlusturmaTarihi
            ON dbo.PersonelMesaiKayitlari(OlusturmaTarihi);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_PersonelMesaiKayitlari_PersonelId_Tarih'
          AND object_id = OBJECT_ID(N'dbo.PersonelMesaiKayitlari'))
        CREATE INDEX IX_PersonelMesaiKayitlari_PersonelId_Tarih
            ON dbo.PersonelMesaiKayitlari(PersonelId, Tarih);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @HataMesaji NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Personel mesai tablosu oluşturulamadı: %s', 16, 1, @HataMesaji);
END CATCH;
GO

SELECT
    OBJECT_ID(N'dbo.PersonelMesaiKayitlari', N'U') AS PersonelMesaiKayitlariObjectId,
    COUNT(*) AS KayitSayisi
FROM dbo.PersonelMesaiKayitlari;
GO
