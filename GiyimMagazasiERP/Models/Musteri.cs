namespace GiyimMagazasiERP.Models;

public class Musteri
{
    public int Id { get; set; }

    public string AdSoyad { get; set; } = null!;
    public string? Telefon { get; set; }
    public string? Email { get; set; }

    public int SadakatPuani { get; set; }
    public decimal IndirimOrani { get; set; }
    public decimal ToplamHarcama { get; set; }

    public DateTime KayitTarihi { get; set; }

    public ICollection<Satis> Satislar { get; set; }
        = new List<Satis>();
}