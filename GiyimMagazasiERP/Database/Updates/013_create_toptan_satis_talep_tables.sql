USE GiyimMagazasiERP;
GO

SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.ToptanSatisTalepleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ToptanSatisTalepleri
        (
            Id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_ToptanSatisTalepleri PRIMARY KEY,

            TalepNo NVARCHAR(30) NOT NULL,
            MusteriId INT NOT NULL,
            TalepEdenPersonelId INT NULL,
            TalepEdenKullaniciId INT NULL,

            OdemeTipi NVARCHAR(30) NOT NULL,
            Aciklama NVARCHAR(500) NULL,

            Durum NVARCHAR(40) NOT NULL
                CONSTRAINT DF_ToptanSatisTalepleri_Durum
                DEFAULT N'YoneticiOnayiBekliyor',

            TalepTarihi DATETIME2 NOT NULL
                CONSTRAINT DF_ToptanSatisTalepleri_TalepTarihi
                DEFAULT GETDATE(),

            YoneticiOnaylayanKullaniciId INT NULL,
            YoneticiOnayTarihi DATETIME2 NULL,

            MuhasebeOnaylayanKullaniciId INT NULL,
            MuhasebeOnayTarihi DATETIME2 NULL,

            ReddedenKullaniciId INT NULL,
            RedTarihi DATETIME2 NULL,
            RedNedeni NVARCHAR(500) NULL,

            ToplamTutar DECIMAL(18,2) NOT NULL,
            IndirimTutari DECIMAL(18,2) NOT NULL,
            NetTutar DECIMAL(18,2) NOT NULL,
            ToplamKdvTutari DECIMAL(18,2) NOT NULL,
            VergiHaricToplam DECIMAL(18,2) NOT NULL,
            VergiDahilToplam DECIMAL(18,2) NOT NULL,

            SatisId INT NULL,
            SatisaDonusturulmeTarihi DATETIME2 NULL,

            RowVersion ROWVERSION NOT NULL
        );
    END;

    IF OBJECT_ID(N'dbo.ToptanSatisTalepDetaylari', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ToptanSatisTalepDetaylari
        (
            Id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_ToptanSatisTalepDetaylari PRIMARY KEY,

            ToptanSatisTalebiId INT NOT NULL,
            UrunId INT NOT NULL,
            Adet INT NOT NULL,

            BirimFiyat DECIMAL(18,2) NOT NULL,
            SatirAraToplam DECIMAL(18,2) NOT NULL,
            SatirIndirimTutari DECIMAL(18,2) NOT NULL,
            KdvOrani DECIMAL(5,2) NOT NULL,
            KdvTutari DECIMAL(18,2) NOT NULL,
            VergiHaricTutar DECIMAL(18,2) NOT NULL,
            VergiDahilTutar DECIMAL(18,2) NOT NULL,

            UrunAdiSnapshot NVARCHAR(150) NOT NULL,
            BarkodSnapshot NVARCHAR(50) NOT NULL,
            BedenSnapshot NVARCHAR(20) NOT NULL,
            RenkSnapshot NVARCHAR(50) NOT NULL
        );
    END;

    IF OBJECT_ID(N'dbo.ToptanSatisTalepHareketleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ToptanSatisTalepHareketleri
        (
            Id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_ToptanSatisTalepHareketleri PRIMARY KEY,

            ToptanSatisTalebiId INT NOT NULL,
            KullaniciId INT NULL,

            OncekiDurum NVARCHAR(40) NULL,
            YeniDurum NVARCHAR(40) NOT NULL,
            IslemTarihi DATETIME2 NOT NULL
                CONSTRAINT DF_ToptanSatisTalepHareketleri_IslemTarihi
                DEFAULT GETDATE(),

            Aciklama NVARCHAR(500) NULL
        );
    END;

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepleri_Musteriler'
    )
        ALTER TABLE dbo.ToptanSatisTalepleri
        ADD CONSTRAINT FK_ToptanSatisTalepleri_Musteriler
        FOREIGN KEY (MusteriId) REFERENCES dbo.Musteriler(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepleri_TalepEdenPersonel'
    )
        ALTER TABLE dbo.ToptanSatisTalepleri
        ADD CONSTRAINT FK_ToptanSatisTalepleri_TalepEdenPersonel
        FOREIGN KEY (TalepEdenPersonelId) REFERENCES dbo.Personeller(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepleri_TalepEdenKullanici'
    )
        ALTER TABLE dbo.ToptanSatisTalepleri
        ADD CONSTRAINT FK_ToptanSatisTalepleri_TalepEdenKullanici
        FOREIGN KEY (TalepEdenKullaniciId) REFERENCES dbo.Kullanicilar(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepleri_YoneticiOnaylayan'
    )
        ALTER TABLE dbo.ToptanSatisTalepleri
        ADD CONSTRAINT FK_ToptanSatisTalepleri_YoneticiOnaylayan
        FOREIGN KEY (YoneticiOnaylayanKullaniciId) REFERENCES dbo.Kullanicilar(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepleri_MuhasebeOnaylayan'
    )
        ALTER TABLE dbo.ToptanSatisTalepleri
        ADD CONSTRAINT FK_ToptanSatisTalepleri_MuhasebeOnaylayan
        FOREIGN KEY (MuhasebeOnaylayanKullaniciId) REFERENCES dbo.Kullanicilar(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepleri_ReddedenKullanici'
    )
        ALTER TABLE dbo.ToptanSatisTalepleri
        ADD CONSTRAINT FK_ToptanSatisTalepleri_ReddedenKullanici
        FOREIGN KEY (ReddedenKullaniciId) REFERENCES dbo.Kullanicilar(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepleri_Satislar'
    )
        ALTER TABLE dbo.ToptanSatisTalepleri
        ADD CONSTRAINT FK_ToptanSatisTalepleri_Satislar
        FOREIGN KEY (SatisId) REFERENCES dbo.Satislar(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepDetaylari_Talepler'
    )
        ALTER TABLE dbo.ToptanSatisTalepDetaylari
        ADD CONSTRAINT FK_ToptanSatisTalepDetaylari_Talepler
        FOREIGN KEY (ToptanSatisTalebiId)
        REFERENCES dbo.ToptanSatisTalepleri(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepDetaylari_Urunler'
    )
        ALTER TABLE dbo.ToptanSatisTalepDetaylari
        ADD CONSTRAINT FK_ToptanSatisTalepDetaylari_Urunler
        FOREIGN KEY (UrunId) REFERENCES dbo.Urunler(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepHareketleri_Talepler'
    )
        ALTER TABLE dbo.ToptanSatisTalepHareketleri
        ADD CONSTRAINT FK_ToptanSatisTalepHareketleri_Talepler
        FOREIGN KEY (ToptanSatisTalebiId)
        REFERENCES dbo.ToptanSatisTalepleri(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_ToptanSatisTalepHareketleri_Kullanicilar'
    )
        ALTER TABLE dbo.ToptanSatisTalepHareketleri
        ADD CONSTRAINT FK_ToptanSatisTalepHareketleri_Kullanicilar
        FOREIGN KEY (KullaniciId) REFERENCES dbo.Kullanicilar(Id);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'UX_ToptanSatisTalepleri_TalepNo'
          AND object_id = OBJECT_ID(N'dbo.ToptanSatisTalepleri')
    )
        CREATE UNIQUE INDEX UX_ToptanSatisTalepleri_TalepNo
        ON dbo.ToptanSatisTalepleri(TalepNo);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'UX_ToptanSatisTalepleri_SatisId'
          AND object_id = OBJECT_ID(N'dbo.ToptanSatisTalepleri')
    )
        CREATE UNIQUE INDEX UX_ToptanSatisTalepleri_SatisId
        ON dbo.ToptanSatisTalepleri(SatisId)
        WHERE SatisId IS NOT NULL;

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_ToptanSatisTalepleri_Durum_TalepTarihi'
          AND object_id = OBJECT_ID(N'dbo.ToptanSatisTalepleri')
    )
        CREATE INDEX IX_ToptanSatisTalepleri_Durum_TalepTarihi
        ON dbo.ToptanSatisTalepleri(Durum, TalepTarihi);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_ToptanSatisTalepDetaylari_TalepId'
          AND object_id = OBJECT_ID(N'dbo.ToptanSatisTalepDetaylari')
    )
        CREATE INDEX IX_ToptanSatisTalepDetaylari_TalepId
        ON dbo.ToptanSatisTalepDetaylari(ToptanSatisTalebiId);

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_ToptanSatisTalepHareketleri_TalepId'
          AND object_id = OBJECT_ID(N'dbo.ToptanSatisTalepHareketleri')
    )
        CREATE INDEX IX_ToptanSatisTalepHareketleri_TalepId
        ON dbo.ToptanSatisTalepHareketleri
        (ToptanSatisTalebiId, IslemTarihi);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

SELECT
    t.name AS TabloAdi
FROM sys.tables t
WHERE t.name IN
(
    N'ToptanSatisTalepleri',
    N'ToptanSatisTalepDetaylari',
    N'ToptanSatisTalepHareketleri'
)
ORDER BY t.name;
GO