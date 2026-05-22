namespace GiyimMagazasiERP.Models;

public class SatisDetayi
{
    public int Id { get; set; }

    public int SatisId { get; set; }
    public int UrunId { get; set; }

    public int Adet { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal ToplamTutar { get; set; }

    public Satis Satis { get; set; } = null!;
    public Urun Urun { get; set; } = null!;
}