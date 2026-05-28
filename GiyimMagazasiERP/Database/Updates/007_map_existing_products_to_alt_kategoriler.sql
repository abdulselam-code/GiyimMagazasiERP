USE GiyimMagazasiERP;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    ------------------------------------------------------------
    -- Güvenlik kontrolleri
    ------------------------------------------------------------
    IF OBJECT_ID('dbo.Urunler', 'U') IS NULL
       OR OBJECT_ID('dbo.Kategoriler', 'U') IS NULL
       OR OBJECT_ID('dbo.AltKategoriler', 'U') IS NULL
       OR COL_LENGTH('dbo.Urunler', 'AltKategoriId') IS NULL
    BEGIN
        RAISERROR('Gerekli tablo veya kolonlar bulunamadı. Önce Alt Kategori kurulum scriptini çalıştırın.', 16, 1);
    END

    ------------------------------------------------------------
    -- Eşleşme kuralları
    -- Oncelik küçük olan daha güçlü kabul edilir.
    ------------------------------------------------------------
    DECLARE @Kurallar TABLE
    (
        AltKategoriAdi NVARCHAR(100) NOT NULL,
        LikePattern NVARCHAR(150) NOT NULL,
        EslesmeNotu NVARCHAR(250) NOT NULL,
        Oncelik INT NOT NULL
    );

    INSERT INTO @Kurallar (AltKategoriAdi, LikePattern, EslesmeNotu, Oncelik)
    VALUES
    -- Giyim genel
    (N'Takım Elbise', N'%takım elbise%', N'Ürün adında takım elbise geçtiği için eşleştirildi.', 5),
    (N'Takım Elbise', N'%takim elbise%', N'Ürün adında takim elbise geçtiği için eşleştirildi.', 5),
    (N'Takım Elbise', N'%takım%', N'Ürün adında takım geçtiği için eşleştirildi.', 15),
    (N'Takım Elbise', N'%takim%', N'Ürün adında takim geçtiği için eşleştirildi.', 15),

    (N'Pantolon', N'%pantolon%', N'Ürün adında pantolon geçtiği için eşleştirildi.', 20),
    (N'Pantolon', N'%jean%', N'Ürün adında jean geçtiği için eşleştirildi.', 20),
    (N'Pantolon', N'%kot%', N'Ürün adında kot geçtiği için eşleştirildi.', 20),

    (N'Gömlek', N'%gömlek%', N'Ürün adında gömlek geçtiği için eşleştirildi.', 20),
    (N'Gömlek', N'%gomlek%', N'Ürün adında gomlek geçtiği için eşleştirildi.', 20),

    (N'Tişört', N'%tişört%', N'Ürün adında tişört geçtiği için eşleştirildi.', 20),
    (N'Tişört', N'%tisort%', N'Ürün adında tisort geçtiği için eşleştirildi.', 20),
    (N'Tişört', N'%tshirt%', N'Ürün adında tshirt geçtiği için eşleştirildi.', 20),
    (N'Tişört', N'%t-shirt%', N'Ürün adında t-shirt geçtiği için eşleştirildi.', 20),

    (N'Sweatshirt', N'%sweatshirt%', N'Ürün adında sweatshirt geçtiği için eşleştirildi.', 20),
    (N'Sweatshirt', N'%sweat%', N'Ürün adında sweat geçtiği için eşleştirildi.', 25),

    (N'Ceket', N'%ceket%', N'Ürün adında ceket geçtiği için eşleştirildi.', 20),

    (N'Mont', N'%mont%', N'Ürün adında mont geçtiği için eşleştirildi.', 20),
    (N'Mont', N'%kaban%', N'Ürün adında kaban geçtiği için eşleştirildi.', 20),

    (N'Şort', N'%şort%', N'Ürün adında şort geçtiği için eşleştirildi.', 20),
    (N'Şort', N'%sort%', N'Ürün adında sort geçtiği için eşleştirildi.', 20),

    (N'Eşofman', N'%eşofman%', N'Ürün adında eşofman geçtiği için eşleştirildi.', 20),
    (N'Eşofman', N'%esofman%', N'Ürün adında esofman geçtiği için eşleştirildi.', 20),

    (N'Elbise', N'%elbise%', N'Ürün adında elbise geçtiği için eşleştirildi.', 30),
    (N'Bluz', N'%bluz%', N'Ürün adında bluz geçtiği için eşleştirildi.', 20),
    (N'Etek', N'%etek%', N'Ürün adında etek geçtiği için eşleştirildi.', 20),
    (N'Tunik', N'%tunik%', N'Ürün adında tunik geçtiği için eşleştirildi.', 20),
    (N'Okul Kıyafeti', N'%okul%', N'Ürün adında okul geçtiği için eşleştirildi.', 20),

    -- İç giyim
    (N'Bay İç Giyim', N'%bay iç giyim%', N'Ürün adında bay iç giyim geçtiği için eşleştirildi.', 5),
    (N'Bay İç Giyim', N'%erkek iç giyim%', N'Ürün adında erkek iç giyim geçtiği için eşleştirildi.', 5),
    (N'Bay İç Giyim', N'%bay ic giyim%', N'Ürün adında bay ic giyim geçtiği için eşleştirildi.', 5),
    (N'Bay İç Giyim', N'%erkek ic giyim%', N'Ürün adında erkek ic giyim geçtiği için eşleştirildi.', 5),

    (N'Kadın İç Giyim', N'%kadın iç giyim%', N'Ürün adında kadın iç giyim geçtiği için eşleştirildi.', 5),
    (N'Kadın İç Giyim', N'%bayan iç giyim%', N'Ürün adında bayan iç giyim geçtiği için eşleştirildi.', 5),
    (N'Kadın İç Giyim', N'%kadin ic giyim%', N'Ürün adında kadin ic giyim geçtiği için eşleştirildi.', 5),
    (N'Kadın İç Giyim', N'%bayan ic giyim%', N'Ürün adında bayan ic giyim geçtiği için eşleştirildi.', 5),

    (N'Çocuk İç Giyim', N'%çocuk iç giyim%', N'Ürün adında çocuk iç giyim geçtiği için eşleştirildi.', 5),
    (N'Çocuk İç Giyim', N'%cocuk ic giyim%', N'Ürün adında cocuk ic giyim geçtiği için eşleştirildi.', 5),

    (N'Termal İçlik', N'%termal içlik%', N'Ürün adında termal içlik geçtiği için eşleştirildi.', 10),
    (N'Termal İçlik', N'%termal iclik%', N'Ürün adında termal iclik geçtiği için eşleştirildi.', 10),
    (N'Termal İçlik', N'%içlik%', N'Ürün adında içlik geçtiği için eşleştirildi.', 25),
    (N'Termal İçlik', N'%iclik%', N'Ürün adında iclik geçtiği için eşleştirildi.', 25),

    (N'Atlet', N'%atlet%', N'Ürün adında atlet geçtiği için eşleştirildi.', 20),
    (N'Boxer', N'%boxer%', N'Ürün adında boxer geçtiği için eşleştirildi.', 20),
    (N'Külot', N'%külot%', N'Ürün adında külot geçtiği için eşleştirildi.', 20),
    (N'Külot', N'%kulot%', N'Ürün adında kulot geçtiği için eşleştirildi.', 20),
    (N'Sütyen', N'%sütyen%', N'Ürün adında sütyen geçtiği için eşleştirildi.', 20),
    (N'Sütyen', N'%sutyen%', N'Ürün adında sutyen geçtiği için eşleştirildi.', 20),
    (N'Pijama', N'%pijama%', N'Ürün adında pijama geçtiği için eşleştirildi.', 20),
    (N'Gecelik', N'%gecelik%', N'Ürün adında gecelik geçtiği için eşleştirildi.', 20),

    -- Çorap
    (N'Bay Çorap', N'%bay çorap%', N'Ürün adında bay çorap geçtiği için eşleştirildi.', 5),
    (N'Bay Çorap', N'%erkek çorap%', N'Ürün adında erkek çorap geçtiği için eşleştirildi.', 5),
    (N'Bay Çorap', N'%bay corap%', N'Ürün adında bay corap geçtiği için eşleştirildi.', 5),
    (N'Bay Çorap', N'%erkek corap%', N'Ürün adında erkek corap geçtiği için eşleştirildi.', 5),

    (N'Kadın Çorap', N'%kadın çorap%', N'Ürün adında kadın çorap geçtiği için eşleştirildi.', 5),
    (N'Kadın Çorap', N'%bayan çorap%', N'Ürün adında bayan çorap geçtiği için eşleştirildi.', 5),
    (N'Kadın Çorap', N'%kadin corap%', N'Ürün adında kadin corap geçtiği için eşleştirildi.', 5),
    (N'Kadın Çorap', N'%bayan corap%', N'Ürün adında bayan corap geçtiği için eşleştirildi.', 5),

    (N'Çocuk Çorap', N'%çocuk çorap%', N'Ürün adında çocuk çorap geçtiği için eşleştirildi.', 5),
    (N'Çocuk Çorap', N'%cocuk corap%', N'Ürün adında cocuk corap geçtiği için eşleştirildi.', 5),

    (N'Külotlu Çorap', N'%külotlu çorap%', N'Ürün adında külotlu çorap geçtiği için eşleştirildi.', 8),
    (N'Külotlu Çorap', N'%kulotlu corap%', N'Ürün adında kulotlu corap geçtiği için eşleştirildi.', 8),

    (N'Termal Çorap', N'%termal çorap%', N'Ürün adında termal çorap geçtiği için eşleştirildi.', 10),
    (N'Termal Çorap', N'%termal corap%', N'Ürün adında termal corap geçtiği için eşleştirildi.', 10),

    (N'Spor Çorap', N'%spor çorap%', N'Ürün adında spor çorap geçtiği için eşleştirildi.', 10),
    (N'Spor Çorap', N'%spor corap%', N'Ürün adında spor corap geçtiği için eşleştirildi.', 10),

    (N'Soket Çorap', N'%soket%', N'Ürün adında soket geçtiği için eşleştirildi.', 20),
    (N'Babet Çorap', N'%babet%', N'Ürün adında babet geçtiği için eşleştirildi.', 20),
    (N'Dizaltı Çorap', N'%dizaltı%', N'Ürün adında dizaltı geçtiği için eşleştirildi.', 20),
    (N'Dizaltı Çorap', N'%dizalti%', N'Ürün adında dizalti geçtiği için eşleştirildi.', 20),
    (N'Spor Çorap', N'%çorap%', N'Ürün adında genel çorap geçtiği için Spor Çorap olarak eşleştirildi.', 90),
    (N'Spor Çorap', N'%corap%', N'Ürün adında genel corap geçtiği için Spor Çorap olarak eşleştirildi.', 90),

    -- Ayakkabı
    (N'Spor Ayakkabı', N'%spor ayakkabı%', N'Ürün adında spor ayakkabı geçtiği için eşleştirildi.', 5),
    (N'Spor Ayakkabı', N'%spor ayakkabi%', N'Ürün adında spor ayakkabi geçtiği için eşleştirildi.', 5),
    (N'Günlük Ayakkabı', N'%günlük ayakkabı%', N'Ürün adında günlük ayakkabı geçtiği için eşleştirildi.', 10),
    (N'Günlük Ayakkabı', N'%gunluk ayakkabi%', N'Ürün adında gunluk ayakkabi geçtiği için eşleştirildi.', 10),
    (N'Bot', N'%bot%', N'Ürün adında bot geçtiği için eşleştirildi.', 20),
    (N'Çizme', N'%çizme%', N'Ürün adında çizme geçtiği için eşleştirildi.', 20),
    (N'Çizme', N'%cizme%', N'Ürün adında cizme geçtiği için eşleştirildi.', 20),
    (N'Sandalet', N'%sandalet%', N'Ürün adında sandalet geçtiği için eşleştirildi.', 20),
    (N'Terlik', N'%terlik%', N'Ürün adında terlik geçtiği için eşleştirildi.', 20),
    (N'Günlük Ayakkabı', N'%ayakkabı%', N'Ürün adında genel ayakkabı geçtiği için Günlük Ayakkabı olarak eşleştirildi.', 90),
    (N'Günlük Ayakkabı', N'%ayakkabi%', N'Ürün adında genel ayakkabi geçtiği için Günlük Ayakkabı olarak eşleştirildi.', 90),

    -- Aksesuar
    (N'Çanta', N'%çanta%', N'Ürün adında çanta geçtiği için eşleştirildi.', 20),
    (N'Çanta', N'%canta%', N'Ürün adında canta geçtiği için eşleştirildi.', 20),
    (N'Kemer', N'%kemer%', N'Ürün adında kemer geçtiği için eşleştirildi.', 20),
    (N'Şapka', N'%şapka%', N'Ürün adında şapka geçtiği için eşleştirildi.', 20),
    (N'Şapka', N'%sapka%', N'Ürün adında sapka geçtiği için eşleştirildi.', 20),
    (N'Bere', N'%bere%', N'Ürün adında bere geçtiği için eşleştirildi.', 20),
    (N'Eldiven', N'%eldiven%', N'Ürün adında eldiven geçtiği için eşleştirildi.', 20),
    (N'Atkı', N'%atkı%', N'Ürün adında atkı geçtiği için eşleştirildi.', 20),
    (N'Atkı', N'%atki%', N'Ürün adında atki geçtiği için eşleştirildi.', 20),
    (N'Cüzdan', N'%cüzdan%', N'Ürün adında cüzdan geçtiği için eşleştirildi.', 20),
    (N'Cüzdan', N'%cuzdan%', N'Ürün adında cuzdan geçtiği için eşleştirildi.', 20);

    ------------------------------------------------------------
    -- Aday eşleşmeleri çıkar.
    -- Alt kategori mutlaka ürünün kendi ana kategorisine ait olmalı.
    ------------------------------------------------------------
    DECLARE @AdayEslesmeler TABLE
    (
        UrunId INT NOT NULL,
        YeniAltKategoriId INT NOT NULL,
        EslesmeNotu NVARCHAR(250) NOT NULL,
        Oncelik INT NOT NULL
    );

    INSERT INTO @AdayEslesmeler
    (
        UrunId,
        YeniAltKategoriId,
        EslesmeNotu,
        Oncelik
    )
    SELECT
        u.Id,
        ak.Id,
        r.EslesmeNotu,
        r.Oncelik
    FROM dbo.Urunler u
    INNER JOIN dbo.AltKategoriler ak
        ON ak.KategoriId = u.KategoriId
    INNER JOIN @Kurallar r
        ON r.AltKategoriAdi = ak.AltKategoriAdi
    WHERE u.AltKategoriId IS NULL
      AND LOWER(u.UrunAdi) LIKE LOWER(r.LikePattern);

    ------------------------------------------------------------
    -- Aynı ürün için birden fazla eşleşme varsa en özel olanı seç.
    ------------------------------------------------------------
    DECLARE @SecilenEslesmeler TABLE
    (
        UrunId INT NOT NULL PRIMARY KEY,
        YeniAltKategoriId INT NOT NULL,
        EslesmeNotu NVARCHAR(250) NOT NULL
    );

    ;WITH Sirali AS
    (
        SELECT
            UrunId,
            YeniAltKategoriId,
            EslesmeNotu,
            ROW_NUMBER() OVER
            (
                PARTITION BY UrunId
                ORDER BY Oncelik ASC, YeniAltKategoriId ASC
            ) AS Sira
        FROM @AdayEslesmeler
    )
    INSERT INTO @SecilenEslesmeler
    (
        UrunId,
        YeniAltKategoriId,
        EslesmeNotu
    )
    SELECT
        UrunId,
        YeniAltKategoriId,
        EslesmeNotu
    FROM Sirali
    WHERE Sira = 1;

    ------------------------------------------------------------
    -- Güncellenen kayıtları raporlamak için sakla.
    ------------------------------------------------------------
    DECLARE @Guncellenen TABLE
    (
        UrunId INT NOT NULL,
        UrunAdi NVARCHAR(150) NOT NULL,
        AnaKategori NVARCHAR(100) NOT NULL,
        AltKategori NVARCHAR(100) NOT NULL,
        EslesmeNotu NVARCHAR(250) NOT NULL
    );

    INSERT INTO @Guncellenen
    (
        UrunId,
        UrunAdi,
        AnaKategori,
        AltKategori,
        EslesmeNotu
    )
    SELECT
        u.Id,
        u.UrunAdi,
        k.KategoriAdi,
        ak.AltKategoriAdi,
        s.EslesmeNotu
    FROM @SecilenEslesmeler s
    INNER JOIN dbo.Urunler u ON u.Id = s.UrunId
    INNER JOIN dbo.Kategoriler k ON k.Id = u.KategoriId
    INNER JOIN dbo.AltKategoriler ak ON ak.Id = s.YeniAltKategoriId
    WHERE u.AltKategoriId IS NULL;

    ------------------------------------------------------------
    -- Güvenli update:
    -- Sadece AltKategoriId NULL olan ürünler güncellenir.
    ------------------------------------------------------------
    UPDATE u
    SET u.AltKategoriId = s.YeniAltKategoriId
    FROM dbo.Urunler u
    INNER JOIN @SecilenEslesmeler s ON s.UrunId = u.Id
    INNER JOIN dbo.AltKategoriler ak
        ON ak.Id = s.YeniAltKategoriId
       AND ak.KategoriId = u.KategoriId
    WHERE u.AltKategoriId IS NULL;

    COMMIT TRANSACTION;

    ------------------------------------------------------------
    -- Kontrol 1: Güncellenen ürünler
    ------------------------------------------------------------
    SELECT
        UrunId,
        UrunAdi,
        AnaKategori,
        AltKategori,
        EslesmeNotu
    FROM @Guncellenen
    ORDER BY AnaKategori, AltKategori, UrunAdi;

    ------------------------------------------------------------
    -- Kontrol 2: Hâlâ AltKategoriId boş kalan ürünler
    ------------------------------------------------------------
    SELECT
        u.Id AS UrunId,
        u.UrunAdi,
        k.KategoriAdi AS AnaKategori
    FROM dbo.Urunler u
    INNER JOIN dbo.Kategoriler k ON k.Id = u.KategoriId
    WHERE u.AltKategoriId IS NULL
    ORDER BY k.KategoriAdi, u.UrunAdi;

    ------------------------------------------------------------
    -- Kontrol 3: Alt kategori dağılımı
    ------------------------------------------------------------
    SELECT
        k.KategoriAdi AS AnaKategori,
        ak.AltKategoriAdi AS AltKategori,
        COUNT(u.Id) AS UrunSayisi
    FROM dbo.Urunler u
    INNER JOIN dbo.Kategoriler k ON k.Id = u.KategoriId
    LEFT JOIN dbo.AltKategoriler ak ON ak.Id = u.AltKategoriId
    GROUP BY k.KategoriAdi, ak.AltKategoriAdi
    ORDER BY k.KategoriAdi, ak.AltKategoriAdi;

    ------------------------------------------------------------
    -- Kontrol 4: Toplam durum
    ------------------------------------------------------------
    SELECT
        COUNT(*) AS ToplamUrunSayisi,
        SUM(CASE WHEN AltKategoriId IS NOT NULL THEN 1 ELSE 0 END) AS AltKategoriDoluUrunSayisi,
        SUM(CASE WHEN AltKategoriId IS NULL THEN 1 ELSE 0 END) AS AltKategoriBosUrunSayisi
    FROM dbo.Urunler;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT ERROR_MESSAGE() AS HataMesaji;
END CATCH;
GO