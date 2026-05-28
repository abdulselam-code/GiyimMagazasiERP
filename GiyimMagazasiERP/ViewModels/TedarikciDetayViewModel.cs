using GiyimMagazasiERP.Models;

namespace GiyimMagazasiERP.ViewModels;

public class TedarikciDetayViewModel
{
    public Tedarikci Tedarikci { get; set; } = null!;

    public int ToplamUrunCesidi { get; set; }
    public int ToplamStokAdedi { get; set; }
    public int KritikStokSayisi { get; set; }

    public decimal ToplamStokAlisDegeri { get; set; }
    public decimal ToplamStokSatisDegeri { get; set; }
    public decimal ToplamMaliyet { get; set; }
    public decimal OrtalamaAlisFiyati { get; set; }
    public decimal OrtalamaSatisFiyati { get; set; }
    public decimal OrtalamaKarMarji { get; set; }
    public decimal TedarikciIndirimOrani { get; set; }

    public DateTime? IlkUrunKayitTarihi { get; set; }
    public DateTime? SonUrunKayitTarihi { get; set; }
    public DateTime? SonStokHareketiTarihi { get; set; }

    public string YaklasikCalismaSuresi { get; set; } = "Hesaplanamadı";

    public List<TedarikciUrunDetayViewModel> Urunler { get; set; } = new();
}

public class TedarikciUrunDetayViewModel
{
    public int Id { get; set; }
    public string UrunAdi { get; set; } = "";
    public string Barkod { get; set; } = "";
    public string AnaKategori { get; set; } = "-";
    public string AltKategori { get; set; } = "-";
    public string Beden { get; set; } = "";
    public string Renk { get; set; } = "";

    public decimal AlisFiyati { get; set; }
    public decimal SatisFiyati { get; set; }
    public decimal? KarMarji { get; set; }

    public int StokMiktari { get; set; }
    public int MinimumStok { get; set; }
    public bool AktifMi { get; set; }
}