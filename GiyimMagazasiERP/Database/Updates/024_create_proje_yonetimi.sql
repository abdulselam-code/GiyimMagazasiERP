USE GiyimMagazasiERP;
GO
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.Projeler', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Projeler
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Projeler PRIMARY KEY,
            ProjeAdi NVARCHAR(150) NOT NULL,
            Aciklama NVARCHAR(500) NULL,
            Durum NVARCHAR(30) NOT NULL,
            BaslangicTarihi DATE NOT NULL,
            PlanlananBitisTarihi DATE NOT NULL,
            PlanlananButce DECIMAL(18,2) NOT NULL,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_Projeler_Olusturma DEFAULT GETDATE()
        );
    END;

    IF OBJECT_ID(N'dbo.ProjeEkipUyeleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProjeEkipUyeleri
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProjeEkipUyeleri PRIMARY KEY,
            ProjeId INT NOT NULL,
            AdSoyad NVARCHAR(100) NOT NULL,
            Rol NVARCHAR(100) NOT NULL,
            AktifMi BIT NOT NULL CONSTRAINT DF_ProjeEkipUyeleri_Aktif DEFAULT 1,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_ProjeEkipUyeleri_Olusturma DEFAULT GETDATE(),
            CONSTRAINT FK_ProjeEkipUyeleri_Projeler FOREIGN KEY (ProjeId) REFERENCES dbo.Projeler(Id)
        );
    END;

    IF OBJECT_ID(N'dbo.ProjeGorevleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProjeGorevleri
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProjeGorevleri PRIMARY KEY,
            ProjeId INT NOT NULL,
            SorumluEkipUyesiId INT NULL,
            GorevAdi NVARCHAR(150) NOT NULL,
            Aciklama NVARCHAR(500) NULL,
            ModulAdi NVARCHAR(100) NOT NULL,
            BaslangicTarihi DATE NOT NULL,
            BitisTarihi DATE NOT NULL,
            PlanlananSaat DECIMAL(8,2) NOT NULL,
            GerceklesenSaat DECIMAL(8,2) NOT NULL,
            Durum NVARCHAR(30) NOT NULL,
            Oncelik NVARCHAR(20) NOT NULL,
            TamamlanmaYuzdesi INT NOT NULL,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_ProjeGorevleri_Olusturma DEFAULT GETDATE(),
            CONSTRAINT FK_ProjeGorevleri_Projeler FOREIGN KEY (ProjeId) REFERENCES dbo.Projeler(Id),
            CONSTRAINT FK_ProjeGorevleri_Ekip FOREIGN KEY (SorumluEkipUyesiId) REFERENCES dbo.ProjeEkipUyeleri(Id),
            CONSTRAINT CK_ProjeGorevleri_Tamamlanma CHECK (TamamlanmaYuzdesi BETWEEN 0 AND 100)
        );
    END;

    IF OBJECT_ID(N'dbo.ProjeButceKalemleri', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProjeButceKalemleri
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProjeButceKalemleri PRIMARY KEY,
            ProjeId INT NOT NULL,
            KalemAdi NVARCHAR(150) NOT NULL,
            KalemTuru NVARCHAR(20) NOT NULL,
            Kategori NVARCHAR(50) NOT NULL,
            PlanlananTutar DECIMAL(18,2) NOT NULL,
            GerceklesenTutar DECIMAL(18,2) NOT NULL,
            Aciklama NVARCHAR(300) NULL,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_ProjeButceKalemleri_Olusturma DEFAULT GETDATE(),
            CONSTRAINT FK_ProjeButceKalemleri_Projeler FOREIGN KEY (ProjeId) REFERENCES dbo.Projeler(Id)
        );
    END;

    IF OBJECT_ID(N'dbo.ProjeGorevBagimliliklari', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.ProjeGorevBagimliliklari
        (
            Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProjeGorevBagimliliklari PRIMARY KEY,
            GorevId INT NOT NULL,
            BagimliOlduguGorevId INT NOT NULL,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_ProjeGorevBagimliliklari_Olusturma DEFAULT GETDATE(),
            CONSTRAINT FK_ProjeBagimlilik_Gorev FOREIGN KEY (GorevId) REFERENCES dbo.ProjeGorevleri(Id),
            CONSTRAINT FK_ProjeBagimlilik_Oncul FOREIGN KEY (BagimliOlduguGorevId) REFERENCES dbo.ProjeGorevleri(Id),
            CONSTRAINT CK_ProjeBagimlilik_FarkliGorev CHECK (GorevId <> BagimliOlduguGorevId)
        );
    END;

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_Projeler_ProjeAdi' AND object_id=OBJECT_ID(N'dbo.Projeler'))
        CREATE UNIQUE INDEX UX_Projeler_ProjeAdi ON dbo.Projeler(ProjeAdi);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_ProjeEkipUyeleri_Proje_Ad' AND object_id=OBJECT_ID(N'dbo.ProjeEkipUyeleri'))
        CREATE UNIQUE INDEX UX_ProjeEkipUyeleri_Proje_Ad ON dbo.ProjeEkipUyeleri(ProjeId, AdSoyad);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_ProjeGorevleri_Proje_Gorev' AND object_id=OBJECT_ID(N'dbo.ProjeGorevleri'))
        CREATE UNIQUE INDEX UX_ProjeGorevleri_Proje_Gorev ON dbo.ProjeGorevleri(ProjeId, GorevAdi);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_ProjeButceKalemleri_Proje_Kalem' AND object_id=OBJECT_ID(N'dbo.ProjeButceKalemleri'))
        CREATE UNIQUE INDEX UX_ProjeButceKalemleri_Proje_Kalem ON dbo.ProjeButceKalemleri(ProjeId, KalemAdi);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_ProjeBagimlilik_Gorev_Oncul' AND object_id=OBJECT_ID(N'dbo.ProjeGorevBagimliliklari'))
        CREATE UNIQUE INDEX UX_ProjeBagimlilik_Gorev_Oncul ON dbo.ProjeGorevBagimliliklari(GorevId, BagimliOlduguGorevId);

    IF NOT EXISTS (SELECT 1 FROM dbo.Projeler WHERE ProjeAdi=N'Giyim Mağazası ERP')
        INSERT dbo.Projeler (ProjeAdi, Aciklama, Durum, BaslangicTarihi, PlanlananBitisTarihi, PlanlananButce)
        VALUES (N'Giyim Mağazası ERP', N'Giyim mağazası operasyonlarını tek merkezden yöneten ASP.NET Core MVC ERP projesi.', N'Teslime Hazır', '20260521', '20260620', 180000);

    DECLARE @ProjeId INT = (SELECT Id FROM dbo.Projeler WHERE ProjeAdi=N'Giyim Mağazası ERP');

    UPDATE dbo.Projeler
    SET Aciklama = N'Giyim mağazası operasyonlarını tek merkezden yöneten ASP.NET Core MVC ERP projesi.',
        Durum = N'Teslime Hazır',
        BaslangicTarihi = '20260521',
        PlanlananBitisTarihi = '20260620',
        PlanlananButce = 180000
    WHERE Id = @ProjeId;

    UPDATE dbo.ProjeEkipUyeleri
    SET AdSoyad = N'Abdülselam Onakbaş',
        Rol = N'Proje geliştirici / analiz'
    WHERE ProjeId = @ProjeId
      AND AdSoyad = N'Fatma Koyuncu'
      AND NOT EXISTS
      (
          SELECT 1
          FROM dbo.ProjeEkipUyeleri mevcut
          WHERE mevcut.ProjeId = @ProjeId
            AND mevcut.AdSoyad = N'Abdülselam Onakbaş'
      );

    DECLARE @Ekip TABLE (AdSoyad NVARCHAR(100), Rol NVARCHAR(100));
    INSERT @Ekip VALUES
    (N'Abdülselam Onakbaş', N'Proje geliştirici / analiz'),
    (N'Backend Geliştirici', N'Backend geliştirici'),
    (N'Veritabanı Tasarımcısı', N'Veritabanı tasarımcısı'),
    (N'Test Sorumlusu', N'Test sorumlusu'),
    (N'Dokümantasyon Sorumlusu', N'Dokümantasyon sorumlusu');

    INSERT dbo.ProjeEkipUyeleri (ProjeId, AdSoyad, Rol, AktifMi)
    SELECT @ProjeId, e.AdSoyad, e.Rol, 1 FROM @Ekip e
    WHERE NOT EXISTS (SELECT 1 FROM dbo.ProjeEkipUyeleri x WHERE x.ProjeId=@ProjeId AND x.AdSoyad=e.AdSoyad);

    UPDATE eu
    SET eu.Rol = e.Rol,
        eu.AktifMi = 1
    FROM dbo.ProjeEkipUyeleri eu
    INNER JOIN @Ekip e ON e.AdSoyad = eu.AdSoyad
    WHERE eu.ProjeId = @ProjeId;

    DECLARE @Gorev TABLE
    (
        GorevAdi NVARCHAR(150), ModulAdi NVARCHAR(100), Sorumlu NVARCHAR(100),
        Baslangic DATE, Bitis DATE, Planlanan DECIMAL(8,2), Gerceklesen DECIMAL(8,2),
        Durum NVARCHAR(30), Oncelik NVARCHAR(20), Yuzde INT
    );
    INSERT @Gorev VALUES
    (N'Veritabanı tasarımı',N'Altyapı',N'Veritabanı Tasarımcısı','20260521','20260523',90,92,N'Tamamlandı',N'Kritik',100),
    (N'Ürün/stok modülü',N'Ürün ve Stok',N'Backend Geliştirici','20260523','20260526',110,116,N'Tamamlandı',N'Kritik',100),
    (N'Satış modülü',N'Satış',N'Abdülselam Onakbaş','20260526','20260529',100,104,N'Tamamlandı',N'Kritik',100),
    (N'Fatura modülü',N'Fatura',N'Backend Geliştirici','20260529','20260531',55,58,N'Tamamlandı',N'Yüksek',100),
    (N'Finans modülü',N'Finans',N'Backend Geliştirici','20260531','20260602',72,75,N'Tamamlandı',N'Kritik',100),
    (N'İade modülü',N'İade',N'Abdülselam Onakbaş','20260602','20260604',85,90,N'Tamamlandı',N'Yüksek',100),
    (N'Personel modülü',N'İnsan Kaynakları',N'Backend Geliştirici','20260604','20260606',64,62,N'Tamamlandı',N'Normal',100),
    (N'Personel izinleri',N'İnsan Kaynakları',N'Abdülselam Onakbaş','20260606','20260608',60,64,N'Tamamlandı',N'Yüksek',100),
    (N'Mesai/vardiya',N'İnsan Kaynakları',N'Backend Geliştirici','20260608','20260610',68,70,N'Tamamlandı',N'Yüksek',100),
    (N'Puantaj raporu',N'İnsan Kaynakları',N'Abdülselam Onakbaş','20260610','20260611',48,46,N'Tamamlandı',N'Normal',100),
    (N'Kasa kapanışı',N'Kasa',N'Backend Geliştirici','20260610','20260611',58,61,N'Tamamlandı',N'Kritik',100),
    (N'Depo sipariş talebi',N'Depo',N'Abdülselam Onakbaş','20260611','20260612',70,73,N'Tamamlandı',N'Yüksek',100),
    (N'Ürün-tedarikçi karşılaştırması',N'Tedarik',N'Veritabanı Tasarımcısı','20260612','20260613',44,48,N'Tamamlandı',N'Normal',100),
    (N'Dashboard ve raporlar',N'Raporlama',N'Abdülselam Onakbaş','20260613','20260616',70,74,N'Tamamlandı',N'Yüksek',100),
    (N'Test ve teslim dokümantasyonu',N'Teslim',N'Test Sorumlusu','20260616','20260620',72,18,N'Test Ediliyor',N'Kritik',25);

    INSERT dbo.ProjeGorevleri
        (ProjeId,SorumluEkipUyesiId,GorevAdi,Aciklama,ModulAdi,BaslangicTarihi,BitisTarihi,
         PlanlananSaat,GerceklesenSaat,Durum,Oncelik,TamamlanmaYuzdesi)
    SELECT @ProjeId, eu.Id, g.GorevAdi, g.GorevAdi + N' geliştirme ve doğrulama çalışması.',
           g.ModulAdi,g.Baslangic,g.Bitis,g.Planlanan,g.Gerceklesen,g.Durum,g.Oncelik,g.Yuzde
    FROM @Gorev g
    LEFT JOIN dbo.ProjeEkipUyeleri eu ON eu.ProjeId=@ProjeId AND eu.AdSoyad=g.Sorumlu
    WHERE NOT EXISTS (SELECT 1 FROM dbo.ProjeGorevleri x WHERE x.ProjeId=@ProjeId AND x.GorevAdi=g.GorevAdi);

    UPDATE pg
    SET pg.SorumluEkipUyesiId = eu.Id,
        pg.Aciklama = g.GorevAdi + N' geliştirme ve doğrulama çalışması.',
        pg.ModulAdi = g.ModulAdi,
        pg.BaslangicTarihi = g.Baslangic,
        pg.BitisTarihi = g.Bitis,
        pg.PlanlananSaat = g.Planlanan,
        pg.GerceklesenSaat = g.Gerceklesen,
        pg.Durum = g.Durum,
        pg.Oncelik = g.Oncelik,
        pg.TamamlanmaYuzdesi = g.Yuzde
    FROM dbo.ProjeGorevleri pg
    INNER JOIN @Gorev g ON g.GorevAdi = pg.GorevAdi
    LEFT JOIN dbo.ProjeEkipUyeleri eu ON eu.ProjeId = @ProjeId AND eu.AdSoyad = g.Sorumlu
    WHERE pg.ProjeId = @ProjeId;

    DECLARE @Butce TABLE
    (Kalem NVARCHAR(150), Tur NVARCHAR(20), Kategori NVARCHAR(50), Planlanan DECIMAL(18,2), Gerceklesen DECIMAL(18,2), Aciklama NVARCHAR(300));
    INSERT @Butce VALUES
    (N'Proje geliştirme desteği',N'Gelir',N'Geliştirme',180000,180000,N'Proje için ayrılan toplam kaynak.'),
    (N'Analiz ve geliştirme',N'Gider',N'Geliştirme',105000,101500,N'Analiz, backend ve arayüz geliştirme eforu.'),
    (N'Test çalışmaları',N'Gider',N'Test',28000,26500,N'Regresyon ve kullanıcı kabul testleri.'),
    (N'Dokümantasyon',N'Gider',N'Dokümantasyon',18000,16500,N'Kılavuz, ER diyagramı ve teslim belgeleri.'),
    (N'Sunum hazırlığı',N'Gider',N'Sunum',9000,6500,N'Sunum ve demo hazırlığı.'),
    (N'Araç ve yazılım',N'Gider',N'Araç/Yazılım',12000,9800,N'Geliştirme araçları ve yardımcı yazılımlar.');

    INSERT dbo.ProjeButceKalemleri
        (ProjeId,KalemAdi,KalemTuru,Kategori,PlanlananTutar,GerceklesenTutar,Aciklama)
    SELECT @ProjeId,b.Kalem,b.Tur,b.Kategori,b.Planlanan,b.Gerceklesen,b.Aciklama
    FROM @Butce b
    WHERE NOT EXISTS (SELECT 1 FROM dbo.ProjeButceKalemleri x WHERE x.ProjeId=@ProjeId AND x.KalemAdi=b.Kalem);

    UPDATE pb
    SET pb.KalemTuru = b.Tur,
        pb.Kategori = b.Kategori,
        pb.PlanlananTutar = b.Planlanan,
        pb.GerceklesenTutar = b.Gerceklesen,
        pb.Aciklama = b.Aciklama
    FROM dbo.ProjeButceKalemleri pb
    INNER JOIN @Butce b ON b.Kalem = pb.KalemAdi
    WHERE pb.ProjeId = @ProjeId;

    DECLARE @Bag TABLE (Gorev NVARCHAR(150), Oncul NVARCHAR(150));
    INSERT @Bag VALUES
    (N'Ürün/stok modülü',N'Veritabanı tasarımı'),
    (N'Satış modülü',N'Ürün/stok modülü'),
    (N'Fatura modülü',N'Satış modülü'),
    (N'Finans modülü',N'Satış modülü'),
    (N'İade modülü',N'Fatura modülü'),
    (N'Personel izinleri',N'Personel modülü'),
    (N'Mesai/vardiya',N'Personel modülü'),
    (N'Puantaj raporu',N'Personel izinleri'),
    (N'Puantaj raporu',N'Mesai/vardiya'),
    (N'Kasa kapanışı',N'Satış modülü'),
    (N'Depo sipariş talebi',N'Ürün/stok modülü'),
    (N'Ürün-tedarikçi karşılaştırması',N'Depo sipariş talebi'),
    (N'Dashboard ve raporlar',N'Finans modülü'),
    (N'Test ve teslim dokümantasyonu',N'İade modülü'),
    (N'Test ve teslim dokümantasyonu',N'Puantaj raporu'),
    (N'Test ve teslim dokümantasyonu',N'Kasa kapanışı'),
    (N'Test ve teslim dokümantasyonu',N'Ürün-tedarikçi karşılaştırması'),
    (N'Test ve teslim dokümantasyonu',N'Dashboard ve raporlar');

    INSERT dbo.ProjeGorevBagimliliklari (GorevId, BagimliOlduguGorevId)
    SELECT g.Id, o.Id
    FROM @Bag b
    INNER JOIN dbo.ProjeGorevleri g ON g.ProjeId=@ProjeId AND g.GorevAdi=b.Gorev
    INNER JOIN dbo.ProjeGorevleri o ON o.ProjeId=@ProjeId AND o.GorevAdi=b.Oncul
    WHERE NOT EXISTS
    (
        SELECT 1 FROM dbo.ProjeGorevBagimliliklari x
        WHERE x.GorevId=g.Id AND x.BagimliOlduguGorevId=o.Id
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    DECLARE @Hata NVARCHAR(4000)=ERROR_MESSAGE();
    RAISERROR(N'Proje yönetimi tabloları oluşturulamadı: %s',16,1,@Hata);
END CATCH;
GO

SELECT p.ProjeAdi, p.Durum,
       COUNT(DISTINCT g.Id) AS GorevSayisi,
       COUNT(DISTINCT e.Id) AS EkipUyesiSayisi,
       COUNT(DISTINCT b.Id) AS ButceKalemiSayisi
FROM dbo.Projeler p
LEFT JOIN dbo.ProjeGorevleri g ON g.ProjeId=p.Id
LEFT JOIN dbo.ProjeEkipUyeleri e ON e.ProjeId=p.Id
LEFT JOIN dbo.ProjeButceKalemleri b ON b.ProjeId=p.Id
WHERE p.ProjeAdi=N'Giyim Mağazası ERP'
GROUP BY p.ProjeAdi,p.Durum;
GO
