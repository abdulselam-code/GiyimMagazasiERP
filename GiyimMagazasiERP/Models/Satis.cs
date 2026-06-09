namespace GiyimMagazasiERP.Models;

public class Satis
{
    public int Id { get; set; }

    public int? MusteriId { get; set; }
    public int PersonelId { get; set; }

    public DateTime SatisTarihi { get; set; }

    public decimal ToplamTutar { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal NetTutar { get; set; }
    public decimal ToplamKdvTutari { get; set; }
    public decimal VergiHaricToplam { get; set; }
    public decimal VergiDahilToplam { get; set; }

    public string OdemeTipi { get; set; } = null!;

    public string? SatisTuru { get; set; }
    public string FaturaNo { get; set; } = "";
    public string FaturaSeri { get; set; } = "FAT";
    public int FaturaSiraNo { get; set; }
    public DateTime FaturaTarihi { get; set; }
    public string BelgeTuru { get; set; } = "SatisBelgesi";
    public string FaturaDurumu { get; set; } = "Olusturuldu";
    public string? UUID { get; set; }

    public Musteri? Musteri { get; set; }
    public Personel Personel { get; set; } = null!;

    public ICollection<SatisDetayi> SatisDetaylari { get; set; }
        = new List<SatisDetayi>();

    public ICollection<FinansHareketi> FinansHareketleri { get; set; }
        = new List<FinansHareketi>();
}