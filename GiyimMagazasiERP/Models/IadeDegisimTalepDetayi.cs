namespace GiyimMagazasiERP.Models;

public class IadeDegisimTalepDetayi
{
    public const string UrunDurumuSatilabilir = "Satilabilir";
    public const string UrunDurumuHasarli = "Hasarli";
    public const string UrunDurumuIncelemeGerekli =
        "IncelemeGerekli";

    public int Id { get; set; }

    public int IadeDegisimTalebiId { get; set; }
    public int SatisDetayiId { get; set; }
    public int UrunId { get; set; }

    public int IadeAdedi { get; set; }

    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal SatirIndirimTutari { get; set; }
    public decimal KdvTutari { get; set; }
    public decimal VergiHaricTutar { get; set; }
    public decimal VergiDahilTutar { get; set; }

    public string? IadeNedeni { get; set; }

    public string UrunDurumu { get; set; } =
        UrunDurumuSatilabilir;

    public bool StogaGeriAlinsinMi { get; set; } = true;

    public string UrunAdiSnapshot { get; set; } = "";
    public string? BarkodSnapshot { get; set; }
    public string? BedenSnapshot { get; set; }
    public string? RenkSnapshot { get; set; }

    public IadeDegisimTalebi IadeDegisimTalebi { get; set; }
        = null!;

    public SatisDetayi SatisDetayi { get; set; } = null!;
    public Urun Urun { get; set; } = null!;
}