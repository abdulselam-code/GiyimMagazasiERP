namespace GiyimMagazasiERP.ViewModels;

public class FaturaDetayViewModel
{
    public int SatisId { get; set; }

    public string FaturaNo { get; set; } = "";

    public DateTime SatisTarihi { get; set; }
    public DateTime FaturaTarihi { get; set; }
    public string SatisTuru { get; set; } = "Perakende";
    public string BelgeTuru { get; set; } = "SatisBelgesi";
    public string FaturaDurumu { get; set; } = "Olusturuldu";
    public string? UUID { get; set; }
    public string OdemeTipi { get; set; } = null!;

    public bool KayitliMusteriMi { get; set; }
    public string MusteriAdi { get; set; } = null!;
    public string? MusteriTelefon { get; set; }
    public string? MusteriEmail { get; set; }
    public string MusteriTipi { get; set; } = "Bireysel";
    public string? KurumsalUnvan { get; set; }
    public string? MusteriAdres { get; set; }
    public string? MusteriIl { get; set; }
    public string? MusteriIlce { get; set; }
    public string? MusteriTCKN { get; set; }
    public string? MusteriVKN { get; set; }
    public string? MusteriVergiDairesi { get; set; }

    public string PersonelAdi { get; set; } = null!;
    public string PersonelPozisyonu { get; set; } = null!;

    public decimal ToplamTutar { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal NetTutar { get; set; }
    public decimal ToplamKdvTutari { get; set; }
    public decimal VergiHaricToplam { get; set; }
    public decimal VergiDahilToplam { get; set; }

 

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
    public decimal IndirimTutari { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal KdvTutari { get; set; }
    public decimal VergiHaricTutar { get; set; }
    public decimal VergiDahilTutar { get; set; }
}