USE GiyimMagazasiERP;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF COL_LENGTH('dbo.Kullanicilar', 'PersonelId') IS NULL
    BEGIN
        ALTER TABLE dbo.Kullanicilar
        ADD PersonelId INT NULL;
    END

    IF OBJECT_ID('dbo.Personeller', 'U') IS NOT NULL
       AND OBJECT_ID('dbo.Kullanicilar', 'U') IS NOT NULL
       AND NOT EXISTS
       (
           SELECT 1
           FROM sys.foreign_keys
           WHERE name = 'FK_Kullanicilar_Personeller_PersonelId'
       )
    BEGIN
        ALTER TABLE dbo.Kullanicilar
        ADD CONSTRAINT FK_Kullanicilar_Personeller_PersonelId
        FOREIGN KEY (PersonelId) REFERENCES dbo.Personeller(Id)
        ON DELETE SET NULL;
    END

    DECLARE @KasaPersonelId INT;
    DECLARE @DepoPersonelId INT;
    DECLARE @MuhasebePersonelId INT;
    DECLARE @IkPersonelId INT;
    DECLARE @YoneticiPersonelId INT;

    SELECT TOP 1 @KasaPersonelId = Id
    FROM dbo.Personeller
    WHERE AktifMi = 1
      AND AdSoyad = N'Elif Acar'
      AND Pozisyon = N'Kasiyer'
    ORDER BY Id;

    IF @KasaPersonelId IS NULL
    BEGIN
        SELECT TOP 1 @KasaPersonelId = Id
        FROM dbo.Personeller
        WHERE AktifMi = 1
          AND Pozisyon = N'Kasiyer'
        ORDER BY Id;
    END

    SELECT TOP 1 @DepoPersonelId = Id
    FROM dbo.Personeller
    WHERE AktifMi = 1
      AND Pozisyon LIKE N'%Depo%'
    ORDER BY Id;

    SELECT TOP 1 @MuhasebePersonelId = Id
    FROM dbo.Personeller
    WHERE AktifMi = 1
      AND Pozisyon LIKE N'%Muhasebe%'
    ORDER BY Id;

    SELECT TOP 1 @IkPersonelId = Id
    FROM dbo.Personeller
    WHERE AktifMi = 1
      AND (
          Pozisyon LIKE N'%İnsan%'
          OR Pozisyon LIKE N'%Insan%'
          OR Pozisyon LIKE N'%Kaynak%'
      )
    ORDER BY Id;

    SELECT TOP 1 @YoneticiPersonelId = Id
    FROM dbo.Personeller
    WHERE AktifMi = 1
      AND (
          Pozisyon LIKE N'%Müdür%'
          OR Pozisyon LIKE N'%Mudur%'
          OR Pozisyon LIKE N'%Yönetici%'
          OR Pozisyon LIKE N'%Yonetici%'
      )
    ORDER BY Id;

    UPDATE dbo.Kullanicilar
    SET PersonelId = @KasaPersonelId
    WHERE KullaniciAdi = N'kasa1'
      AND PersonelId IS NULL
      AND @KasaPersonelId IS NOT NULL;

    UPDATE dbo.Kullanicilar
    SET PersonelId = @DepoPersonelId
    WHERE KullaniciAdi = N'depo'
      AND PersonelId IS NULL
      AND @DepoPersonelId IS NOT NULL;

    UPDATE dbo.Kullanicilar
    SET PersonelId = @MuhasebePersonelId
    WHERE KullaniciAdi = N'muhasebe'
      AND PersonelId IS NULL
      AND @MuhasebePersonelId IS NOT NULL;

    UPDATE dbo.Kullanicilar
    SET PersonelId = @IkPersonelId
    WHERE KullaniciAdi = N'ik'
      AND PersonelId IS NULL
      AND @IkPersonelId IS NOT NULL;

    UPDATE dbo.Kullanicilar
    SET PersonelId = @YoneticiPersonelId
    WHERE KullaniciAdi = N'yonetici'
      AND PersonelId IS NULL
      AND @YoneticiPersonelId IS NOT NULL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT ERROR_MESSAGE() AS HataMesaji;
END CATCH;
GO

SELECT 
    k.KullaniciAdi,
    k.Rol,
    k.PersonelId,
    p.AdSoyad AS PersonelAdi,
    p.Pozisyon
FROM Kullanicilar k
LEFT JOIN Personeller p ON p.Id = k.PersonelId
ORDER BY k.Id;
GO