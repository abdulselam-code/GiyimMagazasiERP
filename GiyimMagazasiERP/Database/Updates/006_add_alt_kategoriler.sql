USE GiyimMagazasiERP;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.AltKategoriler', 'U') IS NULL
    BEGIN
        CREATE TABLE dbo.AltKategoriler
        (
            Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
            KategoriId INT NOT NULL,
            AltKategoriAdi NVARCHAR(100) NOT NULL,
            Aciklama NVARCHAR(250) NULL,
            AktifMi BIT NOT NULL CONSTRAINT DF_AltKategoriler_AktifMi DEFAULT 1,
            OlusturmaTarihi DATETIME2 NOT NULL CONSTRAINT DF_AltKategoriler_OlusturmaTarihi DEFAULT GETDATE()
        );
    END

    IF COL_LENGTH('dbo.Urunler', 'AltKategoriId') IS NULL
    BEGIN
        ALTER TABLE dbo.Urunler
        ADD AltKategoriId INT NULL;
    END

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_AltKategoriler_Kategoriler_KategoriId'
    )
    BEGIN
        ALTER TABLE dbo.AltKategoriler
        ADD CONSTRAINT FK_AltKategoriler_Kategoriler_KategoriId
        FOREIGN KEY (KategoriId) REFERENCES dbo.Kategoriler(Id);
    END

    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.foreign_keys
        WHERE name = 'FK_Urunler_AltKategoriler_AltKategoriId'
    )
    BEGIN
        ALTER TABLE dbo.Urunler
        ADD CONSTRAINT FK_Urunler_AltKategoriler_AltKategoriId
        FOREIGN KEY (AltKategoriId) REFERENCES dbo.AltKategoriler(Id)
        ON DELETE SET NULL;
    END

    DECLARE @AltKategoriler TABLE
    (
        KategoriAdi NVARCHAR(100),
        AltKategoriAdi NVARCHAR(100)
    );

    INSERT INTO @AltKategoriler (KategoriAdi, AltKategoriAdi)
    VALUES
    (N'Bay Giyim', N'Pantolon'),
    (N'Bay Giyim', N'Gömlek'),
    (N'Bay Giyim', N'Tişört'),
    (N'Bay Giyim', N'Sweatshirt'),
    (N'Bay Giyim', N'Ceket'),
    (N'Bay Giyim', N'Mont'),
    (N'Bay Giyim', N'Takım Elbise'),
    (N'Bay Giyim', N'Şort'),
    (N'Bay Giyim', N'Eşofman'),

    (N'Kadın Giyim', N'Elbise'),
    (N'Kadın Giyim', N'Bluz'),
    (N'Kadın Giyim', N'Gömlek'),
    (N'Kadın Giyim', N'Tişört'),
    (N'Kadın Giyim', N'Pantolon'),
    (N'Kadın Giyim', N'Etek'),
    (N'Kadın Giyim', N'Ceket'),
    (N'Kadın Giyim', N'Mont'),
    (N'Kadın Giyim', N'Tunik'),
    (N'Kadın Giyim', N'Eşofman'),

    (N'Çocuk Giyim', N'Tişört'),
    (N'Çocuk Giyim', N'Sweatshirt'),
    (N'Çocuk Giyim', N'Pantolon'),
    (N'Çocuk Giyim', N'Eşofman'),
    (N'Çocuk Giyim', N'Elbise'),
    (N'Çocuk Giyim', N'Mont'),
    (N'Çocuk Giyim', N'Şort'),
    (N'Çocuk Giyim', N'Okul Kıyafeti'),

    (N'İç Giyim', N'Bay İç Giyim'),
    (N'İç Giyim', N'Kadın İç Giyim'),
    (N'İç Giyim', N'Çocuk İç Giyim'),
    (N'İç Giyim', N'Atlet'),
    (N'İç Giyim', N'Boxer'),
    (N'İç Giyim', N'Külot'),
    (N'İç Giyim', N'Sütyen'),
    (N'İç Giyim', N'Pijama'),
    (N'İç Giyim', N'Gecelik'),
    (N'İç Giyim', N'Termal İçlik'),

    (N'Çorap', N'Bay Çorap'),
    (N'Çorap', N'Kadın Çorap'),
    (N'Çorap', N'Çocuk Çorap'),
    (N'Çorap', N'Soket Çorap'),
    (N'Çorap', N'Babet Çorap'),
    (N'Çorap', N'Dizaltı Çorap'),
    (N'Çorap', N'Külotlu Çorap'),
    (N'Çorap', N'Termal Çorap'),
    (N'Çorap', N'Spor Çorap'),

    (N'Ayakkabı', N'Spor Ayakkabı'),
    (N'Ayakkabı', N'Günlük Ayakkabı'),
    (N'Ayakkabı', N'Bot'),
    (N'Ayakkabı', N'Çizme'),
    (N'Ayakkabı', N'Sandalet'),
    (N'Ayakkabı', N'Terlik'),

    (N'Aksesuar', N'Çanta'),
    (N'Aksesuar', N'Kemer'),
    (N'Aksesuar', N'Şapka'),
    (N'Aksesuar', N'Bere'),
    (N'Aksesuar', N'Eldiven'),
    (N'Aksesuar', N'Atkı'),
    (N'Aksesuar', N'Cüzdan');

    INSERT INTO dbo.Kategoriler (KategoriAdi, Aciklama)
    SELECT DISTINCT v.KategoriAdi, N'Otomatik oluşturulan ana kategori'
    FROM @AltKategoriler v
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Kategoriler k
        WHERE LOWER(k.KategoriAdi) = LOWER(v.KategoriAdi)
    );

    INSERT INTO dbo.AltKategoriler
    (
        KategoriId,
        AltKategoriAdi,
        Aciklama,
        AktifMi,
        OlusturmaTarihi
    )
    SELECT
        k.Id,
        v.AltKategoriAdi,
        NULL,
        1,
        GETDATE()
    FROM @AltKategoriler v
    INNER JOIN dbo.Kategoriler k
        ON LOWER(k.KategoriAdi) = LOWER(v.KategoriAdi)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.AltKategoriler ak
        WHERE ak.KategoriId = k.Id
          AND LOWER(ak.AltKategoriAdi) = LOWER(v.AltKategoriAdi)
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT ERROR_MESSAGE() AS HataMesaji;
END CATCH;
GO

SELECT 
    k.KategoriAdi,
    ak.AltKategoriAdi,
    ak.AktifMi
FROM Kategoriler k
INNER JOIN AltKategoriler ak ON ak.KategoriId = k.Id
ORDER BY k.KategoriAdi, ak.AltKategoriAdi;
GO