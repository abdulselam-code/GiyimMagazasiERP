USE GiyimMagazasiERP;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    IF OBJECT_ID(N'dbo.PersonelMesaiKayitlari', N'U') IS NOT NULL
    BEGIN
        UPDATE dbo.PersonelMesaiKayitlari
        SET FazlaMesaiSaati =
            CASE
                WHEN GercekGiris IS NOT NULL
                 AND GercekCikis IS NOT NULL
                 AND GerceklesenSaat > 0
                    THEN GerceklesenSaat
                ELSE PlanlananSaat
            END,
            GuncellemeTarihi = GETDATE()
        WHERE MesaiTuru IN
        (
            N'Fazla Mesai',
            N'Hafta Sonu Mesaisi',
            N'Resmi Tatil Mesaisi',
            N'Ek Vardiya'
        )
          AND FazlaMesaiSaati <>
            CASE
                WHEN GercekGiris IS NOT NULL
                 AND GercekCikis IS NOT NULL
                 AND GerceklesenSaat > 0
                    THEN GerceklesenSaat
                ELSE PlanlananSaat
            END;
    END;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @HataMesaji NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(N'Personel fazla mesai kayıtları düzeltilemedi: %s', 16, 1, @HataMesaji);
END CATCH;
GO

SELECT
    Id,
    PersonelId,
    Tarih,
    MesaiTuru,
    PlanlananSaat,
    GerceklesenSaat,
    FazlaMesaiSaati,
    Durum
FROM dbo.PersonelMesaiKayitlari
WHERE MesaiTuru IN
(
    N'Fazla Mesai',
    N'Hafta Sonu Mesaisi',
    N'Resmi Tatil Mesaisi',
    N'Ek Vardiya'
)
ORDER BY Tarih DESC, Id DESC;
GO
