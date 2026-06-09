USE GiyimMagazasiERP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* =========================================================
       1. IADE / DEGISIM TALEPLERI
       ========================================================= */
    IF OBJECT_ID(N'dbo.IadeDegisimTalepleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.IadeDegisimTalepleri
        (
            Id INT IDENTITY(1,1) NOT NULL,
            TalepNo NVARCHAR(30) NOT NULL,
            IadeBelgeNo NVARCHAR(30) NULL,

            SatisId INT NOT NULL,
            MusteriId INT NULL,
            TalepEdenKullaniciId INT NULL,
            TalepEdenPersonelId INT NULL,

            IslemTipi NVARCHAR(20) NOT NULL,
            Durum NVARCHAR(40) NOT NULL,
            TalepTarihi DATETIME2 NOT NULL,
            Aciklama NVARCHAR(500) NULL,

            YoneticiOnaylayanKullaniciId INT NULL,
            YoneticiOnayTarihi DATETIME2 NULL,

            MuhasebeOnaylayanKullaniciId INT NULL,
            MuhasebeOnayTarihi DATETIME2 NULL,

            ReddedenKullaniciId INT NULL,
            RedTarihi DATETIME2 NULL,
            RedNedeni NVARCHAR(500) NULL,

            IptalEdenKullaniciId INT NULL,
            IptalTarihi DATETIME2 NULL,
            IptalNedeni NVARCHAR(500) NULL,

            TamamlanmaTarihi DATETIME2 NULL,
            FinansHareketiId INT NULL,

            ToplamIadeTutari DECIMAL(18,2) NOT NULL,
            ToplamKdvTutari DECIMAL(18,2) NOT NULL,
            VergiHaricToplam DECIMAL(18,2) NOT NULL,
            VergiDahilToplam DECIMAL(18,2) NOT NULL,

            OdemeTipiSnapshot NVARCHAR(50) NULL,
            RowVersion ROWVERSION NOT NULL,

            CONSTRAINT PK_IadeDegisimTalepleri
                PRIMARY KEY (Id)
        );
    END;

    /* Ana tablo varsayılan değerleri */
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimTalepleri_ToplamIadeTutari'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri
        ADD CONSTRAINT DF_IadeDegisimTalepleri_ToplamIadeTutari
            DEFAULT (0) FOR ToplamIadeTutari;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimTalepleri_ToplamKdvTutari'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri
        ADD CONSTRAINT DF_IadeDegisimTalepleri_ToplamKdvTutari
            DEFAULT (0) FOR ToplamKdvTutari;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimTalepleri_VergiHaricToplam'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri
        ADD CONSTRAINT DF_IadeDegisimTalepleri_VergiHaricToplam
            DEFAULT (0) FOR VergiHaricToplam;
    END;

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimTalepleri_VergiDahilToplam'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri
        ADD CONSTRAINT DF_IadeDegisimTalepleri_VergiDahilToplam
            DEFAULT (0) FOR VergiDahilToplam;
    END;

    /* Ana tablo CHECK kısıtları */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimTalepleri_IslemTipi'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimTalepleri_IslemTipi
        CHECK (IslemTipi IN (N'Iade', N'Degisim'));
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimTalepleri_Durum'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimTalepleri_Durum
        CHECK
        (
            Durum IN
            (
                N'YoneticiOnayiBekliyor',
                N'MuhasebeOnayiBekliyor',
                N'Reddedildi',
                N'IptalEdildi',
                N'Tamamlandi'
            )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimTalepleri_Tutarlar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimTalepleri_Tutarlar
        CHECK
        (
            ToplamIadeTutari >= 0
            AND ToplamKdvTutari >= 0
            AND VergiHaricToplam >= 0
            AND VergiDahilToplam >= 0
        );
    END;

    /* Ana tablo indexleri */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'UX_IadeDegisimTalepleri_TalepNo'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepleri')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_IadeDegisimTalepleri_TalepNo
            ON dbo.IadeDegisimTalepleri(TalepNo);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'UX_IadeDegisimTalepleri_IadeBelgeNo'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepleri')
    )
    BEGIN
        CREATE UNIQUE INDEX UX_IadeDegisimTalepleri_IadeBelgeNo
            ON dbo.IadeDegisimTalepleri(IadeBelgeNo)
            WHERE IadeBelgeNo IS NOT NULL;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimTalepleri_SatisId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepleri')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimTalepleri_SatisId
            ON dbo.IadeDegisimTalepleri(SatisId);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimTalepleri_Durum'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepleri')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimTalepleri_Durum
            ON dbo.IadeDegisimTalepleri(Durum);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimTalepleri_IslemTipi'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepleri')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimTalepleri_IslemTipi
            ON dbo.IadeDegisimTalepleri(IslemTipi);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimTalepleri_TalepTarihi'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepleri')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimTalepleri_TalepTarihi
            ON dbo.IadeDegisimTalepleri(TalepTarihi);
    END;

    /* Ana tablo foreign key ilişkileri */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_Satislar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_Satislar
        FOREIGN KEY (SatisId)
        REFERENCES dbo.Satislar(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_Musteriler'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_Musteriler
        FOREIGN KEY (MusteriId)
        REFERENCES dbo.Musteriler(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_TalepEdenKullanici'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_TalepEdenKullanici
        FOREIGN KEY (TalepEdenKullaniciId)
        REFERENCES dbo.Kullanicilar(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_TalepEdenPersonel'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_TalepEdenPersonel
        FOREIGN KEY (TalepEdenPersonelId)
        REFERENCES dbo.Personeller(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_YoneticiOnaylayan'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_YoneticiOnaylayan
        FOREIGN KEY (YoneticiOnaylayanKullaniciId)
        REFERENCES dbo.Kullanicilar(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_MuhasebeOnaylayan'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_MuhasebeOnaylayan
        FOREIGN KEY (MuhasebeOnaylayanKullaniciId)
        REFERENCES dbo.Kullanicilar(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_ReddedenKullanici'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_ReddedenKullanici
        FOREIGN KEY (ReddedenKullaniciId)
        REFERENCES dbo.Kullanicilar(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_IptalEdenKullanici'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_IptalEdenKullanici
        FOREIGN KEY (IptalEdenKullaniciId)
        REFERENCES dbo.Kullanicilar(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimTalepleri_FinansHareketleri'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimTalepleri_FinansHareketleri
        FOREIGN KEY (FinansHareketiId)
        REFERENCES dbo.FinansHareketleri(Id)
        ON DELETE NO ACTION;
    END;

    /* =========================================================
       2. IADE / DEGISIM TALEP DETAYLARI
       ========================================================= */
    IF OBJECT_ID(N'dbo.IadeDegisimTalepDetaylari', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.IadeDegisimTalepDetaylari
        (
            Id INT IDENTITY(1,1) NOT NULL,
            IadeDegisimTalebiId INT NOT NULL,
            SatisDetayiId INT NOT NULL,
            UrunId INT NOT NULL,
            IadeAdedi INT NOT NULL,

            BirimFiyat DECIMAL(18,2) NOT NULL,
            KdvOrani DECIMAL(5,2) NOT NULL,
            SatirIndirimTutari DECIMAL(18,2) NOT NULL,
            KdvTutari DECIMAL(18,2) NOT NULL,
            VergiHaricTutar DECIMAL(18,2) NOT NULL,
            VergiDahilTutar DECIMAL(18,2) NOT NULL,

            IadeNedeni NVARCHAR(300) NULL,
            UrunDurumu NVARCHAR(40) NOT NULL,
            StogaGeriAlinsinMi BIT NOT NULL,

            UrunAdiSnapshot NVARCHAR(200) NOT NULL,
            BarkodSnapshot NVARCHAR(100) NULL,
            BedenSnapshot NVARCHAR(50) NULL,
            RenkSnapshot NVARCHAR(50) NULL,

            CONSTRAINT PK_IadeDegisimTalepDetaylari
                PRIMARY KEY (Id)
        );
    END;

    /* Detay varsayılan değerleri */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimDetay_SatirIndirimTutari'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari
        ADD CONSTRAINT DF_IadeDegisimDetay_SatirIndirimTutari
            DEFAULT (0) FOR SatirIndirimTutari;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimDetay_KdvTutari'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari
        ADD CONSTRAINT DF_IadeDegisimDetay_KdvTutari
            DEFAULT (0) FOR KdvTutari;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimDetay_VergiHaricTutar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari
        ADD CONSTRAINT DF_IadeDegisimDetay_VergiHaricTutar
            DEFAULT (0) FOR VergiHaricTutar;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimDetay_VergiDahilTutar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari
        ADD CONSTRAINT DF_IadeDegisimDetay_VergiDahilTutar
            DEFAULT (0) FOR VergiDahilTutar;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimDetay_StogaGeriAlinsinMi'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari
        ADD CONSTRAINT DF_IadeDegisimDetay_StogaGeriAlinsinMi
            DEFAULT (1) FOR StogaGeriAlinsinMi;
    END;

    /* Detay CHECK kısıtları */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimDetay_IadeAdedi'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimDetay_IadeAdedi
        CHECK (IadeAdedi > 0);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimDetay_UrunDurumu'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimDetay_UrunDurumu
        CHECK
        (
            UrunDurumu IN
            (
                N'Satilabilir',
                N'Hasarli',
                N'IncelemeGerekli'
            )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimDetay_Tutarlar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimDetay_Tutarlar
        CHECK
        (
            BirimFiyat >= 0
            AND KdvOrani >= 0
            AND SatirIndirimTutari >= 0
            AND KdvTutari >= 0
            AND VergiHaricTutar >= 0
            AND VergiDahilTutar >= 0
        );
    END;

    /* Detay indexleri */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimDetay_TalepId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepDetaylari')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimDetay_TalepId
            ON dbo.IadeDegisimTalepDetaylari(IadeDegisimTalebiId);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimDetay_SatisDetayiId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepDetaylari')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimDetay_SatisDetayiId
            ON dbo.IadeDegisimTalepDetaylari(SatisDetayiId);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimDetay_UrunId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepDetaylari')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimDetay_UrunId
            ON dbo.IadeDegisimTalepDetaylari(UrunId);
    END;

    /* Detay foreign key ilişkileri */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimDetay_Talepler'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimDetay_Talepler
        FOREIGN KEY (IadeDegisimTalebiId)
        REFERENCES dbo.IadeDegisimTalepleri(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimDetay_SatisDetaylari'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimDetay_SatisDetaylari
        FOREIGN KEY (SatisDetayiId)
        REFERENCES dbo.SatisDetaylari(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimDetay_Urunler'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepDetaylari WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimDetay_Urunler
        FOREIGN KEY (UrunId)
        REFERENCES dbo.Urunler(Id)
        ON DELETE NO ACTION;
    END;

    /* =========================================================
       3. IADE / DEGISIM TALEP HAREKETLERI
       ========================================================= */
    IF OBJECT_ID(N'dbo.IadeDegisimTalepHareketleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.IadeDegisimTalepHareketleri
        (
            Id INT IDENTITY(1,1) NOT NULL,
            IadeDegisimTalebiId INT NOT NULL,
            KullaniciId INT NULL,
            OncekiDurum NVARCHAR(40) NULL,
            YeniDurum NVARCHAR(40) NOT NULL,
            IslemTarihi DATETIME2 NOT NULL,
            Aciklama NVARCHAR(500) NULL,

            CONSTRAINT PK_IadeDegisimTalepHareketleri
                PRIMARY KEY (Id)
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimHareket_YeniDurum'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepHareketleri WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimHareket_YeniDurum
        CHECK
        (
            LEN(LTRIM(RTRIM(YeniDurum))) > 0
            AND YeniDurum IN
            (
                N'YoneticiOnayiBekliyor',
                N'MuhasebeOnayiBekliyor',
                N'Reddedildi',
                N'IptalEdildi',
                N'Tamamlandi'
            )
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimHareket_TalepId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepHareketleri')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimHareket_TalepId
            ON dbo.IadeDegisimTalepHareketleri(IadeDegisimTalebiId);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimHareket_KullaniciId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepHareketleri')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimHareket_KullaniciId
            ON dbo.IadeDegisimTalepHareketleri(KullaniciId);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimHareket_IslemTarihi'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimTalepHareketleri')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimHareket_IslemTarihi
            ON dbo.IadeDegisimTalepHareketleri(IslemTarihi);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimHareket_Talepler'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepHareketleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimHareket_Talepler
        FOREIGN KEY (IadeDegisimTalebiId)
        REFERENCES dbo.IadeDegisimTalepleri(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimHareket_Kullanicilar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimTalepHareketleri WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimHareket_Kullanicilar
        FOREIGN KEY (KullaniciId)
        REFERENCES dbo.Kullanicilar(Id)
        ON DELETE NO ACTION;
    END;

    /* =========================================================
       4. DEGISIM YENI URUN DETAYLARI
       ========================================================= */
    IF OBJECT_ID(N'dbo.IadeDegisimYeniUrunDetaylari', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.IadeDegisimYeniUrunDetaylari
        (
            Id INT IDENTITY(1,1) NOT NULL,
            IadeDegisimTalebiId INT NOT NULL,
            YeniUrunId INT NOT NULL,
            Adet INT NOT NULL,

            BirimFiyat DECIMAL(18,2) NOT NULL,
            KdvOrani DECIMAL(5,2) NOT NULL,
            KdvTutari DECIMAL(18,2) NOT NULL,
            VergiHaricTutar DECIMAL(18,2) NOT NULL,
            VergiDahilTutar DECIMAL(18,2) NOT NULL,

            UrunAdiSnapshot NVARCHAR(200) NOT NULL,
            BarkodSnapshot NVARCHAR(100) NULL,
            BedenSnapshot NVARCHAR(50) NULL,
            RenkSnapshot NVARCHAR(50) NULL,

            CONSTRAINT PK_IadeDegisimYeniUrunDetaylari
                PRIMARY KEY (Id)
        );
    END;

    /* Yeni ürün detay varsayılan değerleri */
    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimYeniUrun_KdvTutari'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimYeniUrunDetaylari
        ADD CONSTRAINT DF_IadeDegisimYeniUrun_KdvTutari
            DEFAULT (0) FOR KdvTutari;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimYeniUrun_VergiHaricTutar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimYeniUrunDetaylari
        ADD CONSTRAINT DF_IadeDegisimYeniUrun_VergiHaricTutar
            DEFAULT (0) FOR VergiHaricTutar;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.default_constraints
        WHERE name = N'DF_IadeDegisimYeniUrun_VergiDahilTutar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimYeniUrunDetaylari
        ADD CONSTRAINT DF_IadeDegisimYeniUrun_VergiDahilTutar
            DEFAULT (0) FOR VergiDahilTutar;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimYeniUrun_Adet'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimYeniUrunDetaylari WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimYeniUrun_Adet
        CHECK (Adet > 0);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.check_constraints
        WHERE name = N'CK_IadeDegisimYeniUrun_Tutarlar'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimYeniUrunDetaylari WITH CHECK
        ADD CONSTRAINT CK_IadeDegisimYeniUrun_Tutarlar
        CHECK
        (
            BirimFiyat >= 0
            AND KdvOrani >= 0
            AND KdvTutari >= 0
            AND VergiHaricTutar >= 0
            AND VergiDahilTutar >= 0
        );
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimYeniUrun_TalepId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimYeniUrunDetaylari')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimYeniUrun_TalepId
            ON dbo.IadeDegisimYeniUrunDetaylari(IadeDegisimTalebiId);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.indexes
        WHERE name = N'IX_IadeDegisimYeniUrun_YeniUrunId'
          AND object_id = OBJECT_ID(N'dbo.IadeDegisimYeniUrunDetaylari')
    )
    BEGIN
        CREATE INDEX IX_IadeDegisimYeniUrun_YeniUrunId
            ON dbo.IadeDegisimYeniUrunDetaylari(YeniUrunId);
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimYeniUrun_Talepler'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimYeniUrunDetaylari WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimYeniUrun_Talepler
        FOREIGN KEY (IadeDegisimTalebiId)
        REFERENCES dbo.IadeDegisimTalepleri(Id)
        ON DELETE NO ACTION;
    END;

    IF NOT EXISTS
    (
        SELECT 1 FROM sys.foreign_keys
        WHERE name = N'FK_IadeDegisimYeniUrun_Urunler'
    )
    BEGIN
        ALTER TABLE dbo.IadeDegisimYeniUrunDetaylari WITH CHECK
        ADD CONSTRAINT FK_IadeDegisimYeniUrun_Urunler
        FOREIGN KEY (YeniUrunId)
        REFERENCES dbo.Urunler(Id)
        ON DELETE NO ACTION;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

SELECT name
FROM sys.tables
WHERE name IN
(
    N'IadeDegisimTalepleri',
    N'IadeDegisimTalepDetaylari',
    N'IadeDegisimTalepHareketleri',
    N'IadeDegisimYeniUrunDetaylari'
)
ORDER BY name;