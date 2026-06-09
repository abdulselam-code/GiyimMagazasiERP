namespace GiyimMagazasiERP.Models;

public class Urun
{
    public int Id { get; set; }

    public string UrunAdi { get; set; } = null!;
    public string Barkod { get; set; } = null!;

    public int KategoriId { get; set; }

    public int? AltKategoriId { get; set; }
    public int TedarikciId { get; set; }

    public string Beden { get; set; } = null!;
    public string Renk { get; set; } = null!;

    public decimal AlisFiyati { get; set; }
    public decimal SatisFiyati { get; set; }
    public decimal KdvOrani { get; set; } = 20m;
    public int StokMiktari { get; set; }
    public int MinimumStok { get; set; }

    public bool AktifMi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }

    public Kategori Kategori { get; set; } = null!;

    public AltKategori? AltKategori { get; set; }
    public Tedarikci Tedarikci { get; set; } = null!;

    public ICollection<SatisDetayi> SatisDetaylari { get; set; }
        = new List<SatisDetayi>();

    public ICollection<StokHareketi> StokHareketleri { get; set; }
        = new List<StokHareketi>();
}