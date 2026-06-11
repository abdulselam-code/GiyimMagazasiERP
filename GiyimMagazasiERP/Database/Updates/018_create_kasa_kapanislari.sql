USE GiyimMagazasiERP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.KasaKapanislari', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.KasaKapanislari
        (
            Id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_KasaKapanislari PRIMARY KEY,
            KapanisNo NVARCHAR(30) NOT NULL,
            KasaPersonelId INT NOT NULL,
            KasaKullaniciId INT NOT NULL,
            Tarih DATE NOT NULL,
            BeklenenNakit DECIMAL(18,2) NOT NULL,
            BeklenenKrediKarti DECIMAL(18,2) NOT NULL,
            BeklenenHavale DECIMAL(18,2) NOT NULL,
            BeklenenToplam DECIMAL(18,2) NOT NULL,
            SayilanNakit DECIMAL(18,2) NOT NULL,
            SayilanKrediKarti DECIMAL(18,2) NOT NULL,
            SayilanHavale DECIMAL(18,2) NOT NULL,
            SayilanToplam DECIMAL(18,2) NOT NULL,
            FarkNakit DECIMAL(18,2) NOT NULL,
            FarkKrediKarti DECIMAL(18,2) NOT NULL,
            FarkHavale DECIMAL(18,2) NOT NULL,
            FarkToplam DECIMAL(18,2) NOT NULL,
            SatisSayisi INT NOT NULL,
            IadeSayisi INT NOT NULL,
            IadeToplami DECIMAL(18,2) NOT NULL,
            Durum NVARCHAR(30) NOT NULL,
            Aciklama NVARCHAR(500) NULL,
            OnaylayanKullaniciId INT NULL,
            OnayTarihi DATETIME2 NULL,
            RedNedeni NVARCHAR(500) NULL,
            OlusturmaTarihi DATETIME2 NOT NULL
                CONSTRAINT DF_KasaKapanislari_OlusturmaTarihi DEFAULT (GETDATE()),
            GuncellemeTarihi DATETIME2 NULL,
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT FK_KasaKapanislari_Personeller
                FOREIGN KEY (KasaPersonelId) REFERENCES dbo.Personeller(Id),
            CONSTRAINT FK_KasaKapanislari_KasaKullanicilar
                FOREIGN KEY (KasaKullaniciId) REFERENCES dbo.Kullanicilar(Id),
            CONSTRAINT FK_KasaKapanislari_OnaylayanKullanicilar
                FOREIGN KEY (OnaylayanKullaniciId) REFERENCES dbo.Kullanicilar(Id)
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KasaKapanislari') AND name = N'UX_KasaKapanislari_KapanisNo')
        CREATE UNIQUE INDEX UX_KasaKapanislari_KapanisNo ON dbo.KasaKapanislari(KapanisNo);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KasaKapanislari') AND name = N'UX_KasaKapanislari_KasaPersonelId_Tarih')
        CREATE UNIQUE INDEX UX_KasaKapanislari_KasaPersonelId_Tarih ON dbo.KasaKapanislari(KasaPersonelId, Tarih);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KasaKapanislari') AND name = N'IX_KasaKapanislari_Tarih')
        CREATE INDEX IX_KasaKapanislari_Tarih ON dbo.KasaKapanislari(Tarih);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KasaKapanislari') AND name = N'IX_KasaKapanislari_KasaPersonelId')
        CREATE INDEX IX_KasaKapanislari_KasaPersonelId ON dbo.KasaKapanislari(KasaPersonelId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KasaKapanislari') AND name = N'IX_KasaKapanislari_KasaKullaniciId')
        CREATE INDEX IX_KasaKapanislari_KasaKullaniciId ON dbo.KasaKapanislari(KasaKullaniciId);

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.KasaKapanislari') AND name = N'IX_KasaKapanislari_Durum')
        CREATE INDEX IX_KasaKapanislari_Durum ON dbo.KasaKapanislari(Durum);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @HataMesaji NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Kasa kapanışları tablosu oluşturulamadı: %s', 16, 1, @HataMesaji);
END CATCH;
GO

SELECT
    Id,
    KapanisNo,
    KasaPersonelId,
    Tarih,
    BeklenenToplam,
    SayilanToplam,
    FarkToplam,
    Durum
FROM dbo.KasaKapanislari
ORDER BY Tarih DESC, Id DESC;
GO
