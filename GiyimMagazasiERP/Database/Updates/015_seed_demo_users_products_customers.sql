USE GiyimMagazasiERP;
GO

/*
    Teslim/demo verisi hazırlığı
    ---------------------------
    - Mevcut satış, fatura, finans, stok ve iade geçmişini silmez.
    - Kullanılmış ürün, müşteri, personel veya kategori silmez.
    - Tekrar çalıştırılabilir.
    - Demo kullanıcı parolası: Erp2026!
    - SifreHash, ASP.NET Core PasswordHasher<Kullanici> ile uyumludur.
    - Gerçek kullanımdan önce bütün demo parolaları değiştirilmelidir.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID('dbo.Kullanicilar', 'U') IS NULL
       OR OBJECT_ID('dbo.Personeller', 'U') IS NULL
       OR OBJECT_ID('dbo.Musteriler', 'U') IS NULL
       OR OBJECT_ID('dbo.Kategoriler', 'U') IS NULL
       OR OBJECT_ID('dbo.AltKategoriler', 'U') IS NULL
       OR OBJECT_ID('dbo.Urunler', 'U') IS NULL
       OR OBJECT_ID('dbo.Tedarikciler', 'U') IS NULL
       OR OBJECT_ID('dbo.TedarikciAltKategoriler', 'U') IS NULL
       OR OBJECT_ID('dbo.SatisDetaylari', 'U') IS NULL
       OR OBJECT_ID('dbo.StokHareketleri', 'U') IS NULL
       OR COL_LENGTH('dbo.Kullanicilar', 'PersonelId') IS NULL
       OR COL_LENGTH('dbo.Urunler', 'AltKategoriId') IS NULL
       OR COL_LENGTH('dbo.Urunler', 'KdvOrani') IS NULL
       OR COL_LENGTH('dbo.Musteriler', 'MusteriTipi') IS NULL
       OR COL_LENGTH('dbo.Musteriler', 'Il') IS NULL
    BEGIN
        RAISERROR(N'Gerekli tablolar bulunamadı. Önce 001-014 güncelleme scriptlerini çalıştırın.', 16, 1);
        RETURN;
    END;

    /* ============================================================
       1. Güvenli kategori ve alt kategori yazım normalizasyonu
       Çakışan kayıtlar silinmez; kontrol sorgularında raporlanır.
       ============================================================ */

    UPDATE k
    SET KategoriAdi =
        CASE k.KategoriAdi
            WHEN N'Cocuk Giyim' THEN N'Çocuk Giyim'
            WHEN N'Kadin Giyim' THEN N'Kadın Giyim'
            WHEN N'Bayan Giyim' THEN N'Kadın Giyim'
            WHEN N'Ic Giyim' THEN N'İç Giyim'
            WHEN N'Corap' THEN N'Çorap'
            WHEN N'Ayakkabi' THEN N'Ayakkabı'
            ELSE k.KategoriAdi
        END
    FROM dbo.Kategoriler k
    WHERE k.KategoriAdi IN
          (N'Cocuk Giyim', N'Kadin Giyim', N'Bayan Giyim',
           N'Ic Giyim', N'Corap', N'Ayakkabi')
      AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Kategoriler hedef
              WHERE hedef.Id <> k.Id
                AND hedef.KategoriAdi =
                    CASE k.KategoriAdi
                        WHEN N'Cocuk Giyim' THEN N'Çocuk Giyim'
                        WHEN N'Kadin Giyim' THEN N'Kadın Giyim'
                        WHEN N'Bayan Giyim' THEN N'Kadın Giyim'
                        WHEN N'Ic Giyim' THEN N'İç Giyim'
                        WHEN N'Corap' THEN N'Çorap'
                        WHEN N'Ayakkabi' THEN N'Ayakkabı'
                    END
          );

    UPDATE ak
    SET AltKategoriAdi =
        CASE ak.AltKategoriAdi
            WHEN N'Tisort' THEN N'Tişört'
            WHEN N'Gomlek' THEN N'Gömlek'
            WHEN N'Canta' THEN N'Çanta'
            WHEN N'Sapka' THEN N'Şapka'
            WHEN N'Sort' THEN N'Şort'
            WHEN N'Esofman' THEN N'Eşofman'
            WHEN N'Hirka' THEN N'Hırka'
            WHEN N'Kulot' THEN N'Külot'
            WHEN N'Sutyen' THEN N'Sütyen'
            ELSE ak.AltKategoriAdi
        END
    FROM dbo.AltKategoriler ak
    WHERE ak.AltKategoriAdi IN
          (N'Tisort', N'Gomlek', N'Canta', N'Sapka', N'Sort',
           N'Esofman', N'Hirka', N'Kulot', N'Sutyen')
      AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.AltKategoriler hedef
              WHERE hedef.Id <> ak.Id
                AND hedef.KategoriId = ak.KategoriId
                AND hedef.AltKategoriAdi =
                    CASE ak.AltKategoriAdi
                        WHEN N'Tisort' THEN N'Tişört'
                        WHEN N'Gomlek' THEN N'Gömlek'
                        WHEN N'Canta' THEN N'Çanta'
                        WHEN N'Sapka' THEN N'Şapka'
                        WHEN N'Sort' THEN N'Şort'
                        WHEN N'Esofman' THEN N'Eşofman'
                        WHEN N'Hirka' THEN N'Hırka'
                        WHEN N'Kulot' THEN N'Külot'
                        WHEN N'Sutyen' THEN N'Sütyen'
                    END
          );

    /* ============================================================
       2. Demo personel ve kullanıcılar
       ============================================================ */

    DECLARE @DemoKullanicilar TABLE
    (
        KullaniciAdi NVARCHAR(50) NOT NULL,
        Email NVARCHAR(100) NOT NULL,
        Rol NVARCHAR(30) NOT NULL,
        AdSoyad NVARCHAR(100) NOT NULL,
        Pozisyon NVARCHAR(50) NULL,
        Departman NVARCHAR(50) NULL,
        Telefon NVARCHAR(20) NULL,
        Maas DECIMAL(18,2) NULL
    );

    INSERT INTO @DemoKullanicilar
        (KullaniciAdi, Email, Rol, AdSoyad, Pozisyon, Departman, Telefon, Maas)
    VALUES
        (N'admin',      N'admin@giyimerp.local',      N'Admin',             N'Sistem Yöneticisi',       NULL,                         NULL,                 NULL,             NULL),
        (N'yonetici1',  N'yonetici1@giyimerp.local',  N'Yonetici',          N'Mehmet Kaya',             N'Mağaza Müdürü',             N'Yönetim',            N'0500 100 00 01', 52000),
        (N'muhasebe1',  N'muhasebe1@giyimerp.local',  N'Muhasebe',          N'Zeynep Demir',            N'Muhasebe Sorumlusu',        N'Muhasebe',           N'0500 100 00 02', 44000),
        (N'kasa1',      N'kasa1@giyimerp.local',      N'Kasiyer',           N'Elif Acar',               N'Kasiyer',                    N'Satış',              N'0500 100 00 03', 31000),
        (N'kasa2',      N'kasa2@giyimerp.local',      N'Kasiyer',           N'Burak Yalçın',            N'Kasiyer',                    N'Satış',              N'0500 100 00 04', 31000),
        (N'personel1',  N'personel1@giyimerp.local',  N'Personel',          N'Ayşe Karaca',             N'Satış Danışmanı',           N'Satış',              N'0500 100 00 05', 30000),
        (N'personel2',  N'personel2@giyimerp.local',  N'Personel',          N'Emre Şahin',              N'Satış Danışmanı',           N'Satış',              N'0500 100 00 06', 30000),
        (N'depo1',      N'depo1@giyimerp.local',      N'Depo',              N'Hasan Polat',             N'Depo Sorumlusu',            N'Depo',               N'0500 100 00 07', 34000),
        (N'ik1',        N'ik1@giyimerp.local',        N'InsanKaynaklari',   N'Selin Arslan',            N'İnsan Kaynakları Sorumlusu',N'İnsan Kaynakları',   N'0500 100 00 08', 42000);

    INSERT INTO dbo.Personeller
    (
        AdSoyad, Telefon, Email, Pozisyon, Maas, PrimOrani,
        GirisSaati, CikisSaati, MesaiSaati, IzinGunu,
        Departman, AktifMi, IseBaslamaTarihi
    )
    SELECT
        d.AdSoyad,
        d.Telefon,
        d.Email,
        d.Pozisyon,
        d.Maas,
        CASE WHEN d.Pozisyon = N'Satış Danışmanı' THEN 2.50 ELSE 0 END,
        CAST('09:00' AS TIME),
        CAST('18:00' AS TIME),
        0,
        14,
        d.Departman,
        1,
        DATEFROMPARTS(2025, 1 + ABS(CHECKSUM(d.KullaniciAdi)) % 10, 1)
    FROM @DemoKullanicilar d
    WHERE d.Pozisyon IS NOT NULL
      AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.Personeller p
              WHERE p.Email = d.Email
                 OR (p.AdSoyad = d.AdSoyad AND p.Pozisyon = d.Pozisyon)
          );

    DECLARE @DemoSifreHash NVARCHAR(255) =
        N'AQAAAAIAAYagAAAAELJGb3/pJgg+IY50Xa/dEM6P/VikrveUw9SUVtny+hz0+kwC64H9LKtpMdKS5w9peQ==';

    INSERT INTO dbo.Kullanicilar
    (
        KullaniciAdi, Email, SifreHash, Rol, AdSoyad,
        PersonelId, SonGirisTarihi, AktifMi, OlusturmaTarihi
    )
    SELECT
        d.KullaniciAdi,
        d.Email,
        @DemoSifreHash,
        d.Rol,
        d.AdSoyad,
        p.Id,
        NULL,
        1,
        GETDATE()
    FROM @DemoKullanicilar d
    LEFT JOIN dbo.Personeller p ON p.Email = d.Email
    WHERE NOT EXISTS
          (
              SELECT 1
              FROM dbo.Kullanicilar k
              WHERE k.KullaniciAdi = d.KullaniciAdi
                 OR k.Email = d.Email
          );

    UPDATE k
    SET k.AktifMi = 1,
        k.Rol = d.Rol,
        k.AdSoyad = COALESCE(NULLIF(k.AdSoyad, N''), d.AdSoyad),
        k.PersonelId = COALESCE(k.PersonelId, p.Id)
    FROM dbo.Kullanicilar k
    INNER JOIN @DemoKullanicilar d
        ON d.KullaniciAdi = k.KullaniciAdi
    LEFT JOIN dbo.Personeller p
        ON p.Email = d.Email;

    /* ============================================================
       3. Demo müşteri verileri
       ============================================================ */

    DECLARE @DemoMusteriler TABLE
    (
        AdSoyad NVARCHAR(100),
        Telefon NVARCHAR(20),
        Email NVARCHAR(100),
        MusteriTipi NVARCHAR(30),
        KurumsalUnvan NVARCHAR(150),
        Adres NVARCHAR(250),
        Il NVARCHAR(50),
        Ilce NVARCHAR(50),
        TCKN NVARCHAR(20),
        VKN NVARCHAR(20),
        VergiDairesi NVARCHAR(100),
        SadakatPuani INT,
        IndirimOrani DECIMAL(5,2)
    );

    INSERT INTO @DemoMusteriler
    VALUES
        (N'Mehmet Yıldız', N'0500 200 00 01', N'mehmet.yildiz@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 1', N'Şanlıurfa', N'Haliliye', N'99000000001', NULL, NULL, 120, 2),
        (N'Ayşe Karataş', N'0500 200 00 02', N'ayse.karatas@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 2', N'Şanlıurfa', N'Eyyübiye', N'99000000002', NULL, NULL, 85, 0),
        (N'Fatma Demir', N'0500 200 00 03', N'fatma.demir@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 3', N'Şanlıurfa', N'Karaköprü', N'99000000003', NULL, NULL, 210, 3),
        (N'Ahmet Çelik', N'0500 200 00 04', N'ahmet.celik@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 4', N'Şanlıurfa', N'Siverek', N'99000000004', NULL, NULL, 45, 0),
        (N'Zeynep Kaya', N'0500 200 00 05', N'zeynep.kaya@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 5', N'Şanlıurfa', N'Viranşehir', N'99000000005', NULL, NULL, 160, 2),
        (N'Mustafa Arslan', N'0500 200 00 06', N'mustafa.arslan@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 6', N'Şanlıurfa', N'Akçakale', N'99000000006', NULL, NULL, 30, 0),
        (N'Emine Şahin', N'0500 200 00 07', N'emine.sahin@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 7', N'Şanlıurfa', N'Birecik', N'99000000007', NULL, NULL, 95, 1),
        (N'Yusuf Aydın', N'0500 200 00 08', N'yusuf.aydin@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 8', N'Şanlıurfa', N'Harran', N'99000000008', NULL, NULL, 70, 0),
        (N'Gülcan Öztürk', N'0500 200 00 09', N'gulcan.ozturk@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 9', N'Şanlıurfa', N'Haliliye', N'99000000009', NULL, NULL, 140, 2),
        (N'İbrahim Koç', N'0500 200 00 10', N'ibrahim.koc@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 10', N'Şanlıurfa', N'Karaköprü', N'99000000010', NULL, NULL, 55, 0),
        (N'Nazan Kurt', N'0500 200 00 19', N'nazan.kurt@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 19', N'Şanlıurfa', N'Bozova', N'99000000019', NULL, NULL, 80, 1),
        (N'Ömer Aslan', N'0500 200 00 20', N'omer.aslan@demo.local', N'Bireysel', NULL, N'Demo Mahallesi No: 20', N'Şanlıurfa', N'Suruç', N'99000000020', NULL, NULL, 105, 1),
        (N'Elif Yılmaz', N'0500 200 00 11', N'elif.yilmaz@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 11', N'İstanbul', N'Kadıköy', N'99000000011', NULL, NULL, 180, 2),
        (N'Burak Koç', N'0500 200 00 12', N'burak.koc@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 12', N'Ankara', N'Çankaya', N'99000000012', NULL, NULL, 90, 1),
        (N'Ceren Özdemir', N'0500 200 00 13', N'ceren.ozdemir@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 13', N'Gaziantep', N'Şahinbey', N'99000000013', NULL, NULL, 65, 0),
        (N'Ali Polat', N'0500 200 00 14', N'ali.polat@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 14', N'Diyarbakır', N'Kayapınar', N'99000000014', NULL, NULL, 75, 0),
        (N'Merve Acar', N'0500 200 00 15', N'merve.acar@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 15', N'Mardin', N'Artuklu', N'99000000015', NULL, NULL, 110, 1),
        (N'Selin Korkmaz', N'0500 200 00 16', N'selin.korkmaz@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 16', N'Kayseri', N'Melikgazi', N'99000000016', NULL, NULL, 50, 0),
        (N'Hakan Eren', N'0500 200 00 17', N'hakan.eren@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 17', N'İzmir', N'Bornova', N'99000000017', NULL, NULL, 135, 2),
        (N'Derya Uslu', N'0500 200 00 18', N'derya.uslu@demo.local', N'Bireysel', NULL, N'Demo Cadde No: 18', N'Bursa', N'Nilüfer', N'99000000018', NULL, NULL, 100, 1),
        (N'Harran Tekstil', N'0500 300 00 01', N'harran.tekstil@demo.local', N'Kurumsal', N'Harran Tekstil Ltd. Şti.', N'Organize Sanayi Demo Blok A', N'Şanlıurfa', N'Haliliye', NULL, N'9990000001', N'Şanlıurfa', 0, 8),
        (N'Karaköprü Butik', N'0500 300 00 02', N'karakopru.butik@demo.local', N'Kurumsal', N'Karaköprü Butik Mağazacılık', N'Demo Bulvarı No: 20', N'Şanlıurfa', N'Karaköprü', NULL, N'9990000002', N'Şanlıurfa', 0, 6),
        (N'Güneydoğu Giyim', N'0500 300 00 03', N'guneydogu.giyim@demo.local', N'Kurumsal', N'Güneydoğu Giyim Toptan', N'Ticaret Merkezi Demo Blok', N'Gaziantep', N'Şehitkamil', NULL, N'9990000003', N'Gaziantep', 0, 7),
        (N'Mezopotamya Tekstil', N'0500 300 00 04', N'mezopotamya.tekstil@demo.local', N'Kurumsal', N'Mezopotamya Tekstil', N'Demo İş Merkezi Kat: 2', N'Diyarbakır', N'Kayapınar', NULL, N'9990000004', N'Diyarbakır', 0, 5);

    INSERT INTO dbo.Musteriler
    (
        AdSoyad, Telefon, Email, MusteriTipi, KurumsalUnvan,
        Adres, Il, Ilce, TCKN, VKN, VergiDairesi,
        SadakatPuani, IndirimOrani, ToplamHarcama, KayitTarihi
    )
    SELECT
        d.AdSoyad, d.Telefon, d.Email, d.MusteriTipi, d.KurumsalUnvan,
        d.Adres, d.Il, d.Ilce, d.TCKN, d.VKN, d.VergiDairesi,
        d.SadakatPuani, d.IndirimOrani, 0, GETDATE()
    FROM @DemoMusteriler d
    WHERE NOT EXISTS
          (
              SELECT 1
              FROM dbo.Musteriler m
              WHERE m.Email = d.Email
                 OR m.Telefon = d.Telefon
                 OR (d.TCKN IS NOT NULL AND m.TCKN = d.TCKN)
                 OR (d.VKN IS NOT NULL AND m.VKN = d.VKN)
          );

    UPDATE m
    SET Telefon = COALESCE(NULLIF(m.Telefon, N''), N'0509' + RIGHT(N'0000000' + CAST(m.Id AS NVARCHAR(7)), 7)),
        Email = COALESCE(NULLIF(m.Email, N''), N'musteri' + CAST(m.Id AS NVARCHAR(10)) + N'@demo.local'),
        Adres = COALESCE(NULLIF(m.Adres, N''), N'Demo müşteri adresi'),
        Il = COALESCE(NULLIF(m.Il, N''), N'Şanlıurfa'),
        Ilce = COALESCE(NULLIF(m.Ilce, N''), N'Haliliye'),
        MusteriTipi = COALESCE(NULLIF(m.MusteriTipi, N''), N'Bireysel')
    FROM dbo.Musteriler m
    WHERE NULLIF(m.Telefon, N'') IS NULL
       OR NULLIF(m.Email, N'') IS NULL
       OR NULLIF(m.Adres, N'') IS NULL
       OR NULLIF(m.Il, N'') IS NULL
       OR NULLIF(m.Ilce, N'') IS NULL
       OR NULLIF(m.MusteriTipi, N'') IS NULL;

    /* ============================================================
       4. Alt kategoriler için eksik demo ürünleri ve stok dengesi
       ============================================================ */

    IF NOT EXISTS (SELECT 1 FROM dbo.Tedarikciler WHERE AktifMi = 1)
    BEGIN
        INSERT INTO dbo.Tedarikciler
            (FirmaAdi, Telefon, Email, Adres, IndirimOrani, AktifMi)
        VALUES
            (N'Demo Tekstil Tedarik', N'0500 400 00 01',
             N'tedarik@demo.local', N'Şanlıurfa Demo Sanayi Bölgesi', 5, 1);
    END;

    DECLARE @EklenenUrunler TABLE (UrunId INT PRIMARY KEY);
    DECLARE @AltKategoriId INT;
    DECLARE @KategoriId INT;
    DECLARE @AltKategoriAdi NVARCHAR(100);
    DECLARE @MevcutAdet INT;
    DECLARE @Sira INT;
    DECLARE @TedarikciId INT;
    DECLARE @YeniUrunId INT;

    DECLARE alt_kategori_cursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT ak.Id, ak.KategoriId, ak.AltKategoriAdi
        FROM dbo.AltKategoriler ak
        WHERE ak.AktifMi = 1
        ORDER BY ak.Id;

    OPEN alt_kategori_cursor;
    FETCH NEXT FROM alt_kategori_cursor
        INTO @AltKategoriId, @KategoriId, @AltKategoriAdi;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @MevcutAdet = COUNT(*)
        FROM dbo.Urunler
        WHERE AltKategoriId = @AltKategoriId;

        SELECT TOP 1 @TedarikciId = tak.TedarikciId
        FROM dbo.TedarikciAltKategoriler tak
        INNER JOIN dbo.Tedarikciler t ON t.Id = tak.TedarikciId
        WHERE tak.AltKategoriId = @AltKategoriId
          AND tak.AktifMi = 1
          AND t.AktifMi = 1
        ORDER BY tak.Id;

        IF @TedarikciId IS NULL
        BEGIN
            SELECT TOP 1 @TedarikciId = Id
            FROM dbo.Tedarikciler
            WHERE AktifMi = 1
            ORDER BY Id;
        END;

        IF OBJECT_ID('dbo.TedarikciAltKategoriler', 'U') IS NOT NULL
           AND NOT EXISTS
               (
                   SELECT 1
                   FROM dbo.TedarikciAltKategoriler
                   WHERE TedarikciId = @TedarikciId
                     AND AltKategoriId = @AltKategoriId
               )
        BEGIN
            INSERT INTO dbo.TedarikciAltKategoriler
                (TedarikciId, AltKategoriId, AktifMi, OlusturmaTarihi)
            VALUES
                (@TedarikciId, @AltKategoriId, 1, GETDATE());
        END;

        SET @Sira = @MevcutAdet + 1;

        WHILE @Sira <= 2
        BEGIN
            DECLARE @Barkod NVARCHAR(50) =
                N'DEMO-AK' + RIGHT(N'000000' + CAST(@AltKategoriId AS NVARCHAR(6)), 6)
                + N'-' + RIGHT(N'00' + CAST(@Sira AS NVARCHAR(2)), 2);

            IF NOT EXISTS (SELECT 1 FROM dbo.Urunler WHERE Barkod = @Barkod)
            BEGIN
                INSERT INTO dbo.Urunler
                (
                    UrunAdi, Barkod, KategoriId, AltKategoriId, TedarikciId,
                    Beden, Renk, AlisFiyati, SatisFiyati, KdvOrani,
                    StokMiktari, MinimumStok, AktifMi, OlusturmaTarihi
                )
                VALUES
                (
                    @AltKategoriAdi + CASE @Sira WHEN 1 THEN N' Classic' ELSE N' Premium' END,
                    @Barkod,
                    @KategoriId,
                    @AltKategoriId,
                    @TedarikciId,
                    CASE
                        WHEN @AltKategoriAdi LIKE N'%Ayakkabı%'
                          OR @AltKategoriAdi IN (N'Bot', N'Çizme', N'Sandalet', N'Terlik') THEN N'40'
                        WHEN @AltKategoriAdi LIKE N'%Çanta%'
                          OR @AltKategoriAdi IN (N'Kemer', N'Şapka', N'Bere', N'Eldiven', N'Atkı', N'Cüzdan') THEN N'Standart'
                        WHEN @AltKategoriAdi LIKE N'%Çocuk%' THEN N'10 Yaş'
                        ELSE N'M'
                    END,
                    CASE @Sira WHEN 1 THEN N'Lacivert' ELSE N'Siyah' END,
                    CAST(180 + ((@AltKategoriId % 8) * 25) AS DECIMAL(18,2)),
                    CAST((180 + ((@AltKategoriId % 8) * 25)) * 1.65 AS DECIMAL(18,2)),
                    20,
                    CASE WHEN @AltKategoriId % 17 = 0 AND @Sira = 1 THEN 3
                         ELSE 14 + (@AltKategoriId % 20) END,
                    5,
                    1,
                    GETDATE()
                );

                SET @YeniUrunId = SCOPE_IDENTITY();
                INSERT INTO @EklenenUrunler (UrunId) VALUES (@YeniUrunId);
            END;

            SET @Sira += 1;
        END;

        SET @TedarikciId = NULL;
        FETCH NEXT FROM alt_kategori_cursor
            INTO @AltKategoriId, @KategoriId, @AltKategoriAdi;
    END;

    CLOSE alt_kategori_cursor;
    DEALLOCATE alt_kategori_cursor;

    /*
       Yalnızca hiç satılmamış sıfır stoklu ürünler makul stoğa çekilir.
       Stok geçmişinin tutarlı kalması için düzeltme hareketi oluşturulur.
    */
    DECLARE @StokDuzeltilecek TABLE (UrunId INT PRIMARY KEY, YeniStok INT);

    INSERT INTO @StokDuzeltilecek (UrunId, YeniStok)
    SELECT u.Id, 12
    FROM dbo.Urunler u
    WHERE u.StokMiktari = 0
      AND u.AktifMi = 1
      AND NOT EXISTS
          (
              SELECT 1
              FROM dbo.SatisDetaylari sd
              WHERE sd.UrunId = u.Id
          );

    UPDATE u
    SET StokMiktari = d.YeniStok
    FROM dbo.Urunler u
    INNER JOIN @StokDuzeltilecek d ON d.UrunId = u.Id;

    INSERT INTO dbo.StokHareketleri
        (UrunId, HareketTipi, Miktar, Tarih, Aciklama)
    SELECT d.UrunId, N'Giris', d.YeniStok, GETDATE(),
           N'015 teslim/demo veri hazırlığı stok düzeltmesi'
    FROM @StokDuzeltilecek d
    WHERE NOT EXISTS
          (
              SELECT 1
              FROM dbo.StokHareketleri sh
              WHERE sh.UrunId = d.UrunId
                AND sh.Aciklama = N'015 teslim/demo veri hazırlığı stok düzeltmesi'
          );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF CURSOR_STATUS('local', 'alt_kategori_cursor') >= 0
        CLOSE alt_kategori_cursor;
    IF CURSOR_STATUS('local', 'alt_kategori_cursor') > -3
        DEALLOCATE alt_kategori_cursor;

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    SELECT
        ERROR_NUMBER() AS HataNo,
        ERROR_LINE() AS HataSatiri,
        ERROR_MESSAGE() AS HataMesaji;

    DECLARE @HataMesaji NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@HataMesaji, 16, 1);
    RETURN;
END CATCH;
GO

/* ================================================================
   KONTROL SORGULARI
   ================================================================ */

SELECT Rol, COUNT(*) AS KullaniciSayisi
FROM dbo.Kullanicilar
WHERE AktifMi = 1
GROUP BY Rol
ORDER BY Rol;

SELECT
    k.KategoriAdi,
    ak.AltKategoriAdi,
    COUNT(u.Id) AS UrunSayisi
FROM dbo.Kategoriler k
LEFT JOIN dbo.AltKategoriler ak ON ak.KategoriId = k.Id
LEFT JOIN dbo.Urunler u ON u.AltKategoriId = ak.Id
GROUP BY k.KategoriAdi, ak.AltKategoriAdi
ORDER BY k.KategoriAdi, ak.AltKategoriAdi;

SELECT Id, UrunAdi, Barkod, StokMiktari, MinimumStok
FROM dbo.Urunler
WHERE StokMiktari = 0
ORDER BY UrunAdi;

SELECT Id, UrunAdi, Barkod, StokMiktari, MinimumStok
FROM dbo.Urunler
WHERE AktifMi = 1
  AND StokMiktari <= MinimumStok
ORDER BY StokMiktari, UrunAdi;

SELECT COUNT(*) AS ToplamMusteriSayisi
FROM dbo.Musteriler;

SELECT Il, COUNT(*) AS MusteriSayisi
FROM dbo.Musteriler
GROUP BY Il
ORDER BY MusteriSayisi DESC, Il;

SELECT COUNT(*) AS SanliurfaMusteriSayisi
FROM dbo.Musteriler
WHERE Il = N'Şanlıurfa';

SELECT
    SUM(CASE WHEN NULLIF(Telefon, N'') IS NULL THEN 1 ELSE 0 END) AS EksikTelefon,
    SUM(CASE WHEN NULLIF(Email, N'') IS NULL THEN 1 ELSE 0 END) AS EksikEmail,
    SUM(CASE WHEN NULLIF(Adres, N'') IS NULL THEN 1 ELSE 0 END) AS EksikAdres,
    SUM(CASE WHEN MusteriTipi = N'Kurumsal' THEN 1 ELSE 0 END) AS KurumsalMusteriSayisi
FROM dbo.Musteriler;

/* Çakışma nedeniyle otomatik birleştirilmeyen olası kategori tekrarları. */
SELECT
    k1.Id AS BirinciKategoriId,
    k1.KategoriAdi AS BirinciKategori,
    k2.Id AS IkinciKategoriId,
    k2.KategoriAdi AS IkinciKategori
FROM dbo.Kategoriler k1
INNER JOIN dbo.Kategoriler k2 ON k1.Id < k2.Id
WHERE REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER(k1.KategoriAdi),
          N'ç', N'c'), N'ı', N'i'), N'ş', N's'), N'ö', N'o'), N'ü', N'u')
    = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(LOWER(k2.KategoriAdi),
          N'ç', N'c'), N'ı', N'i'), N'ş', N's'), N'ö', N'o'), N'ü', N'u');

SELECT Id, KategoriAdi AS BirlesmeIncelemesiGerekenKategori
FROM dbo.Kategoriler
WHERE KategoriAdi IN
      (N'Bayan Giyim', N'Kadin Giyim', N'Cocuk Giyim',
       N'Ic Giyim', N'Corap', N'Ayakkabi')
ORDER BY KategoriAdi;
GO
