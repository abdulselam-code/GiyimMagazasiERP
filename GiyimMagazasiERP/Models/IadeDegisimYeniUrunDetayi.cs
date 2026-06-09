namespace GiyimMagazasiERP.Models;

public class IadeDegisimYeniUrunDetayi
{
    public int Id { get; set; }

    public int IadeDegisimTalebiId { get; set; }
    public int YeniUrunId { get; set; }

    public int Adet { get; set; }

    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal KdvTutari { get; set; }
    public decimal VergiHaricTutar { get; set; }
    public decimal VergiDahilTutar { get; set; }

    public string UrunAdiSnapshot { get; set; } = "";
    public string? BarkodSnapshot { get; set; }
    public string? BedenSnapshot { get; set; }
    public string? RenkSnapshot { get; set; }

    public IadeDegisimTalebi IadeDegisimTalebi { get; set; }
        = null!;

    public Urun YeniUrun { get; set; } = null!;
}