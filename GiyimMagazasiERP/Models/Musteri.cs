namespace GiyimMagazasiERP.Models;

public class Musteri
{
    public int Id { get; set; }

    public string AdSoyad { get; set; } = null!;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string MusteriTipi { get; set; } = "Bireysel";
    public string? KurumsalUnvan { get; set; }
    public string? Adres { get; set; }
    public string? Il { get; set; }
    public string? Ilce { get; set; }
    public string? TCKN { get; set; }
    public string? VKN { get; set; }
    public string? VergiDairesi { get; set; }

    public int SadakatPuani { get; set; }
    public decimal IndirimOrani { get; set; }
    public decimal ToplamHarcama { get; set; }

    public DateTime KayitTarihi { get; set; }

    public ICollection<Satis> Satislar { get; set; }
        = new List<Satis>();
}