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

    public string OdemeTipi { get; set; } = null!;

    public Musteri? Musteri { get; set; }
    public Personel Personel { get; set; } = null!;

    public ICollection<SatisDetayi> SatisDetaylari { get; set; }
        = new List<SatisDetayi>();

    public ICollection<FinansHareketi> FinansHareketleri { get; set; }
        = new List<FinansHareketi>();
}