USE GiyimMagazasiERP;
GO

/*
    Güvenli giriş ve rol sistemi için Kullanicilar tablosunu hazırlar.
    Mevcut verileri silmez.
    DROP, DELETE, TRUNCATE kullanmaz.
    Tekrar çalıştırılabilir.
*/

IF COL_LENGTH('Kullanicilar', 'AdSoyad') IS NULL
BEGIN
    ALTER TABLE Kullanicilar ADD AdSoyad NVARCHAR(100) NULL;
END;
GO

IF COL_LENGTH('Kullanicilar', 'SonGirisTarihi') IS NULL
BEGIN
    ALTER TABLE Kullanicilar ADD SonGirisTarihi DATETIME2 NULL;
END;
GO

IF COL_LENGTH('Kullanicilar', 'Rol') IS NULL
BEGIN
    ALTER TABLE Kullanicilar ADD Rol NVARCHAR(50) NULL;
END;
GO

IF COL_LENGTH('Kullanicilar', 'AktifMi') IS NULL
BEGIN
    ALTER TABLE Kullanicilar ADD AktifMi BIT NOT NULL CONSTRAINT DF_Kullanicilar_AktifMi DEFAULT 1;
END;
GO

IF COL_LENGTH('Kullanicilar', 'SifreHash') IS NULL
BEGIN
    ALTER TABLE Kullanicilar ADD SifreHash NVARCHAR(255) NULL;
END;
GO

UPDATE Kullanicilar
SET Rol = ISNULL(Rol, 'Personel'),
    AktifMi = ISNULL(AktifMi, 1)
WHERE Rol IS NULL OR AktifMi IS NULL;
GO

/*
    Demo kullanıcı şifresi: Erp2026!
    Veritabanında açık metin şifre tutulmaz.
    Aşağıdaki SifreHash değerleri ASP.NET Core PasswordHasher uyumludur.
*/

IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE KullaniciAdi = 'admin' OR Email = 'admin@giyimerp.local')
BEGIN
    INSERT INTO Kullanicilar (KullaniciAdi, Email, SifreHash, Rol, AktifMi, OlusturmaTarihi, AdSoyad)
    VALUES ('admin', 'admin@giyimerp.local', 'AQAAAAIAAYagAAAAELJGb3/pJgg+IY50Xa/dEM6P/VikrveUw9SUVtny+hz0+kwC64H9LKtpMdKS5w9peQ==', 'Admin', 1, GETDATE(), N'Sistem Yöneticisi');
END;
GO

IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE KullaniciAdi = 'yonetici' OR Email = 'yonetici@giyimerp.local')
BEGIN
    INSERT INTO Kullanicilar (KullaniciAdi, Email, SifreHash, Rol, AktifMi, OlusturmaTarihi, AdSoyad)
    VALUES ('yonetici', 'yonetici@giyimerp.local', 'AQAAAAIAAYagAAAAEHevXxxS7rloWz+1xk6dXzl3yVmvw5Bby4lHh+oiwKPjsuB+3LTJt9cx8K5imbQdOw==', 'Yonetici', 1, GETDATE(), N'Mağaza Yöneticisi');
END;
GO

IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE KullaniciAdi = 'kasa1' OR Email = 'kasa1@giyimerp.local')
BEGIN
    INSERT INTO Kullanicilar (KullaniciAdi, Email, SifreHash, Rol, AktifMi, OlusturmaTarihi, AdSoyad)
    VALUES ('kasa1', 'kasa1@giyimerp.local', 'AQAAAAIAAYagAAAAEL2cydgxj/WAbMw8FG2zGnPMvWcXp6HjJYZqyDFHA/lP+BpdVg8YNrnlf88RuYc4vQ==', 'Kasiyer', 1, GETDATE(), N'Kasa Kullanıcısı');
END;
GO

IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE KullaniciAdi = 'depo' OR Email = 'depo@giyimerp.local')
BEGIN
    INSERT INTO Kullanicilar (KullaniciAdi, Email, SifreHash, Rol, AktifMi, OlusturmaTarihi, AdSoyad)
    VALUES ('depo', 'depo@giyimerp.local', 'AQAAAAIAAYagAAAAEDRCDTyIBDN7r+PqAW39e8wU/SWh6koKyBhvrhVu7Gjc/RXFwACT1E6awdtTy67DDg==', 'Depo', 1, GETDATE(), N'Depo Kullanıcısı');
END;
GO

IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE KullaniciAdi = 'muhasebe' OR Email = 'muhasebe@giyimerp.local')
BEGIN
    INSERT INTO Kullanicilar (KullaniciAdi, Email, SifreHash, Rol, AktifMi, OlusturmaTarihi, AdSoyad)
    VALUES ('muhasebe', 'muhasebe@giyimerp.local', 'AQAAAAIAAYagAAAAEEZnIEpZJM5mJZmRbm6a5Z4zDBIbEj6G0zCj6trpUtkyG13YL7IQUh8WZJxGRHYUZQ==', 'Muhasebe', 1, GETDATE(), N'Muhasebe Kullanıcısı');
END;
GO

IF NOT EXISTS (SELECT 1 FROM Kullanicilar WHERE KullaniciAdi = 'ik' OR Email = 'ik@giyimerp.local')
BEGIN
    INSERT INTO Kullanicilar (KullaniciAdi, Email, SifreHash, Rol, AktifMi, OlusturmaTarihi, AdSoyad)
    VALUES ('ik', 'ik@giyimerp.local', 'AQAAAAIAAYagAAAAECyQTVj8j1luUmGGWqijWASv9ltjX+pv1wE7BMmlW0yZH8SZET+Rjrfc4XITLfx5xw==', 'InsanKaynaklari', 1, GETDATE(), N'İnsan Kaynakları Kullanıcısı');
END;
GO

SELECT KullaniciAdi, Email, Rol, AktifMi, AdSoyad
FROM Kullanicilar
ORDER BY Id;
GO