USE GiyimMagazasiERP;
GO
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.UrunTedarikcileri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.UrunTedarikcileri
        (
            Id INT IDENTITY(1,1) NOT NULL
                CONSTRAINT PK_UrunTedarikcileri PRIMARY KEY,
            UrunId INT NOT NULL,
            TedarikciId INT NOT NULL,
            TedarikciUrunKodu NVARCHAR(100) NULL,
            BirimMaliyet DECIMAL(18,2) NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_BirimMaliyet DEFAULT 0,
            IndirimOrani DECIMAL(5,2) NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_IndirimOrani DEFAULT 0,
            NetBirimMaliyet DECIMAL(18,2) NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_NetBirimMaliyet DEFAULT 0,
            MinimumSiparisAdedi INT NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_MinimumSiparisAdedi DEFAULT 1,
            TeslimSuresiGun INT NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_TeslimSuresiGun DEFAULT 0,
            VarsayilanMi BIT NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_VarsayilanMi DEFAULT 0,
            AktifMi BIT NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_AktifMi DEFAULT 1,
            Aciklama NVARCHAR(500) NULL,
            OlusturmaTarihi DATETIME2 NOT NULL
                CONSTRAINT DF_UrunTedarikcileri_OlusturmaTarihi DEFAULT GETDATE(),
            GuncellemeTarihi DATETIME2 NULL,
            RowVersion ROWVERSION NOT NULL,
            CONSTRAINT FK_UrunTedarikcileri_Urunler
                FOREIGN KEY (UrunId) REFERENCES dbo.Urunler(Id),
            CONSTRAINT FK_UrunTedarikcileri_Tedarikciler
                FOREIGN KEY (TedarikciId) REFERENCES dbo.Tedarikciler(Id),
            CONSTRAINT CK_UrunTedarikcileri_BirimMaliyet
                CHECK (BirimMaliyet >= 0),
            CONSTRAINT CK_UrunTedarikcileri_IndirimOrani
                CHECK (IndirimOrani >= 0 AND IndirimOrani <= 100),
            CONSTRAINT CK_UrunTedarikcileri_NetBirimMaliyet
                CHECK (NetBirimMaliyet >= 0),
            CONSTRAINT CK_UrunTedarikcileri_MinimumSiparisAdedi
                CHECK (MinimumSiparisAdedi > 0),
            CONSTRAINT CK_UrunTedarikcileri_TeslimSuresiGun
                CHECK (TeslimSuresiGun >= 0)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.indexes
        WHERE name = N'UX_UrunTedarikcileri_UrunId_TedarikciId'
          AND object_id = OBJECT_ID(N'dbo.UrunTedarikcileri')
    )
        CREATE UNIQUE INDEX UX_UrunTedarikcileri_UrunId_TedarikciId
            ON dbo.UrunTedarikcileri(UrunId, TedarikciId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_UrunTedarikcileri_UrunId'
          AND object_id = OBJECT_ID(N'dbo.UrunTedarikcileri')
    )
        CREATE INDEX IX_UrunTedarikcileri_UrunId
            ON dbo.UrunTedarikcileri(UrunId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_UrunTedarikcileri_TedarikciId'
          AND object_id = OBJECT_ID(N'dbo.UrunTedarikcileri')
    )
        CREATE INDEX IX_UrunTedarikcileri_TedarikciId
            ON dbo.UrunTedarikcileri(TedarikciId);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_UrunTedarikcileri_AktifMi'
          AND object_id = OBJECT_ID(N'dbo.UrunTedarikcileri')
    )
        CREATE INDEX IX_UrunTedarikcileri_AktifMi
            ON dbo.UrunTedarikcileri(AktifMi);

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_UrunTedarikcileri_VarsayilanMi'
          AND object_id = OBJECT_ID(N'dbo.UrunTedarikcileri')
    )
        CREATE INDEX IX_UrunTedarikcileri_VarsayilanMi
            ON dbo.UrunTedarikcileri(VarsayilanMi);

    /*
      Mevcut ürün kartındaki güvenilir AlisFiyati kullanılır.
      Herhangi bir tahmini/demo maliyet üretilmez.
      Tedarikçinin genel indirim oranı başlangıç ilişkisine aktarılır.
    */
    INSERT INTO dbo.UrunTedarikcileri
    (
        UrunId,
        TedarikciId,
        BirimMaliyet,
        IndirimOrani,
        NetBirimMaliyet,
        MinimumSiparisAdedi,
        TeslimSuresiGun,
        VarsayilanMi,
        AktifMi,
        Aciklama,
        OlusturmaTarihi
    )
    SELECT
        u.Id,
        u.TedarikciId,
        ISNULL(u.AlisFiyati, 0),
        CASE
            WHEN ISNULL(t.IndirimOrani, 0) BETWEEN 0 AND 100
                THEN ISNULL(t.IndirimOrani, 0)
            ELSE 0
        END,
        ROUND(
            ISNULL(u.AlisFiyati, 0) *
            (1 - (
                CASE
                    WHEN ISNULL(t.IndirimOrani, 0) BETWEEN 0 AND 100
                        THEN ISNULL(t.IndirimOrani, 0)
                    ELSE 0
                END
            ) / 100.0),
            2
        ),
        1,
        0,
        1,
        1,
        N'Mevcut ürün kartındaki tedarikçi bilgisinden oluşturuldu.',
        GETDATE()
    FROM dbo.Urunler u
    INNER JOIN dbo.Tedarikciler t ON t.Id = u.TedarikciId
    WHERE u.TedarikciId IS NOT NULL
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.UrunTedarikcileri ut
          WHERE ut.UrunId = u.Id
            AND ut.TedarikciId = u.TedarikciId
      );

    IF OBJECT_ID(N'dbo.DepoSiparisTalepKalemleri', N'U') IS NOT NULL
    BEGIN
        IF COL_LENGTH(N'dbo.DepoSiparisTalepKalemleri', N'UrunTedarikciId') IS NULL
            ALTER TABLE dbo.DepoSiparisTalepKalemleri
                ADD UrunTedarikciId INT NULL;

        IF COL_LENGTH(N'dbo.DepoSiparisTalepKalemleri', N'TahminiIndirimOrani') IS NULL
            ALTER TABLE dbo.DepoSiparisTalepKalemleri
                ADD TahminiIndirimOrani DECIMAL(5,2) NULL;

        IF COL_LENGTH(N'dbo.DepoSiparisTalepKalemleri', N'TahminiTeslimSuresiGun') IS NULL
            ALTER TABLE dbo.DepoSiparisTalepKalemleri
                ADD TahminiTeslimSuresiGun INT NULL;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.foreign_keys
            WHERE name = N'FK_DepoSiparisKalem_UrunTedarikci'
        )
            ALTER TABLE dbo.DepoSiparisTalepKalemleri
                ADD CONSTRAINT FK_DepoSiparisKalem_UrunTedarikci
                FOREIGN KEY (UrunTedarikciId)
                REFERENCES dbo.UrunTedarikcileri(Id);

        IF NOT EXISTS
        (
            SELECT 1 FROM sys.indexes
            WHERE name = N'IX_DepoSiparisTalepKalemleri_UrunTedarikciId'
              AND object_id = OBJECT_ID(N'dbo.DepoSiparisTalepKalemleri')
        )
            CREATE INDEX IX_DepoSiparisTalepKalemleri_UrunTedarikciId
                ON dbo.DepoSiparisTalepKalemleri(UrunTedarikciId);
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @Hata NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Ürün-tedarikçi yapısı oluşturulamadı: %s', 16, 1, @Hata);
END CATCH;
GO

SELECT
    u.UrunAdi,
    t.FirmaAdi,
    ut.BirimMaliyet,
    ut.IndirimOrani,
    ut.NetBirimMaliyet,
    ut.MinimumSiparisAdedi,
    ut.TeslimSuresiGun,
    ut.VarsayilanMi,
    ut.AktifMi
FROM dbo.UrunTedarikcileri ut
INNER JOIN dbo.Urunler u ON u.Id = ut.UrunId
INNER JOIN dbo.Tedarikciler t ON t.Id = ut.TedarikciId
ORDER BY u.UrunAdi, ut.NetBirimMaliyet, t.FirmaAdi;
GO
