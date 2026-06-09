namespace GiyimMagazasiERP.Models;

public class ToptanSatisTalepDetayi
{
    public int Id { get; set; }

    public int ToptanSatisTalebiId { get; set; }
    public int UrunId { get; set; }

    public int Adet { get; set; }

    public decimal BirimFiyat { get; set; }
    public decimal SatirAraToplam { get; set; }
    public decimal SatirIndirimTutari { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal KdvTutari { get; set; }
    public decimal VergiHaricTutar { get; set; }
    public decimal VergiDahilTutar { get; set; }

    public string UrunAdiSnapshot { get; set; } = "";
    public string BarkodSnapshot { get; set; } = "";
    public string BedenSnapshot { get; set; } = "";
    public string RenkSnapshot { get; set; } = "";

    public ToptanSatisTalebi ToptanSatisTalebi { get; set; } = null!;
    public Urun Urun { get; set; } = null!;
}