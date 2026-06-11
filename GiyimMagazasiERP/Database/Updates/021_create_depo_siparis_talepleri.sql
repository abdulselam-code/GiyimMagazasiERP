USE GiyimMagazasiERP;
GO
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.DepoSiparisTalepleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DepoSiparisTalepleri
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DepoSiparisTalepleri PRIMARY KEY,
            TalepNo NVARCHAR(30) NOT NULL,
            TalepEdenKullaniciId INT NOT NULL,
            TalepEdenPersonelId INT NULL,
            TalepTarihi DATETIME2 NOT NULL,
            Durum NVARCHAR(30) NOT NULL,
            Oncelik NVARCHAR(20) NOT NULL,
            Aciklama NVARCHAR(500) NULL,
            OnaylayanKullaniciId INT NULL,
            OnayTarihi DATETIME2 NULL,
            RedNedeni NVARCHAR(500) NULL,
            TeslimAlanKullaniciId INT NULL,
            TeslimAlmaTarihi DATETIME2 NULL,
            IptalTarihi DATETIME2 NULL,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_DepoSiparisTalepleri_Olusturma DEFAULT GETDATE(),
            GuncellemeTarihi DATETIME2 NULL,
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT FK_DepoSiparisTalepleri_TalepKullanici FOREIGN KEY (TalepEdenKullaniciId) REFERENCES dbo.Kullanicilar(Id),
            CONSTRAINT FK_DepoSiparisTalepleri_TalepPersonel FOREIGN KEY (TalepEdenPersonelId) REFERENCES dbo.Personeller(Id),
            CONSTRAINT FK_DepoSiparisTalepleri_OnayKullanici FOREIGN KEY (OnaylayanKullaniciId) REFERENCES dbo.Kullanicilar(Id),
            CONSTRAINT FK_DepoSiparisTalepleri_TeslimKullanici FOREIGN KEY (TeslimAlanKullaniciId) REFERENCES dbo.Kullanicilar(Id)
        );
    END;

    IF OBJECT_ID(N'dbo.DepoSiparisTalepKalemleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.DepoSiparisTalepKalemleri
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DepoSiparisTalepKalemleri PRIMARY KEY,
            DepoSiparisTalebiId INT NOT NULL,
            UrunId INT NOT NULL,
            TedarikciId INT NULL,
            MevcutStok INT NOT NULL,
            MinimumStok INT NOT NULL,
            TalepAdedi INT NOT NULL,
            OnaylananAdet INT NOT NULL CONSTRAINT DF_DepoSiparisKalem_Onaylanan DEFAULT 0,
            TeslimAlinanAdet INT NOT NULL CONSTRAINT DF_DepoSiparisKalem_Teslim DEFAULT 0,
            TahminiBirimMaliyet DECIMAL(18,2) NULL,
            Aciklama NVARCHAR(300) NULL,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_DepoSiparisKalem_Olusturma DEFAULT GETDATE(),
            CONSTRAINT FK_DepoSiparisKalem_Talep FOREIGN KEY (DepoSiparisTalebiId) REFERENCES dbo.DepoSiparisTalepleri(Id),
            CONSTRAINT FK_DepoSiparisKalem_Urun FOREIGN KEY (UrunId) REFERENCES dbo.Urunler(Id),
            CONSTRAINT FK_DepoSiparisKalem_Tedarikci FOREIGN KEY (TedarikciId) REFERENCES dbo.Tedarikciler(Id),
            CONSTRAINT UQ_DepoSiparisKalem_TalepUrun UNIQUE (DepoSiparisTalebiId, UrunId),
            CONSTRAINT CK_DepoSiparisKalem_TalepAdedi CHECK (TalepAdedi > 0),
            CONSTRAINT CK_DepoSiparisKalem_Onaylanan CHECK (OnaylananAdet >= 0),
            CONSTRAINT CK_DepoSiparisKalem_Teslim CHECK (TeslimAlinanAdet >= 0 AND TeslimAlinanAdet <= OnaylananAdet)
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_DepoSiparisTalepleri_TalepNo' AND object_id=OBJECT_ID(N'dbo.DepoSiparisTalepleri'))
        CREATE UNIQUE INDEX UX_DepoSiparisTalepleri_TalepNo ON dbo.DepoSiparisTalepleri(TalepNo);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_DepoSiparisTalepleri_Durum' AND object_id=OBJECT_ID(N'dbo.DepoSiparisTalepleri'))
        CREATE INDEX IX_DepoSiparisTalepleri_Durum ON dbo.DepoSiparisTalepleri(Durum);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_DepoSiparisTalepleri_TalepTarihi' AND object_id=OBJECT_ID(N'dbo.DepoSiparisTalepleri'))
        CREATE INDEX IX_DepoSiparisTalepleri_TalepTarihi ON dbo.DepoSiparisTalepleri(TalepTarihi);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_DepoSiparisTalepleri_TalepEdenKullaniciId' AND object_id=OBJECT_ID(N'dbo.DepoSiparisTalepleri'))
        CREATE INDEX IX_DepoSiparisTalepleri_TalepEdenKullaniciId ON dbo.DepoSiparisTalepleri(TalepEdenKullaniciId);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_DepoSiparisTalepleri_Oncelik' AND object_id=OBJECT_ID(N'dbo.DepoSiparisTalepleri'))
        CREATE INDEX IX_DepoSiparisTalepleri_Oncelik ON dbo.DepoSiparisTalepleri(Oncelik);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'IX_DepoSiparisTalepKalemleri_UrunId' AND object_id=OBJECT_ID(N'dbo.DepoSiparisTalepKalemleri'))
        CREATE INDEX IX_DepoSiparisTalepKalemleri_UrunId ON dbo.DepoSiparisTalepKalemleri(UrunId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @Hata NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Depo sipariş talebi tabloları oluşturulamadı: %s', 16, 1, @Hata);
END CATCH;
GO

SELECT OBJECT_ID(N'dbo.DepoSiparisTalepleri', N'U') AS TalepTablosu,
       OBJECT_ID(N'dbo.DepoSiparisTalepKalemleri', N'U') AS KalemTablosu;
GO
