namespace GiyimMagazasiERP.ViewModels;

public class FaturaDetayViewModel
{
    public int SatisId { get; set; }

    public string FaturaNo => $"FAT-{SatisId:D6}";

    public DateTime SatisTarihi { get; set; }
    public string OdemeTipi { get; set; } = null!;

    public bool KayitliMusteriMi { get; set; }
    public string MusteriAdi { get; set; } = null!;
    public string? MusteriTelefon { get; set; }
    public string? MusteriEmail { get; set; }

    public string PersonelAdi { get; set; } = null!;
    public string PersonelPozisyonu { get; set; } = null!;

    public decimal ToplamTutar { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal NetTutar { get; set; }

    public decimal KdvOrani => 20m;

    public decimal KdvDahilGenelToplam => NetTutar;

    public decimal KdvHaricTutar => KdvDahilGenelToplam / 1.20m;

    public decimal KdvTutari => KdvDahilGenelToplam - KdvHaricTutar;

    public List<FaturaKalemiViewModel> Kalemler { get; set; } = new();
}

public class FaturaKalemiViewModel
{
    public string UrunAdi { get; set; } = null!;
    public string Barkod { get; set; } = null!;
    public string Beden { get; set; } = null!;
    public string Renk { get; set; } = null!;

    public int Adet { get; set; }
    public decimal BirimFiyat { get; set; }
    public decimal ToplamTutar { get; set; }
}