namespace GiyimMagazasiERP.Models;

public class StokHareketi
{
    public int Id { get; set; }

    public int UrunId { get; set; }

    public string HareketTipi { get; set; } = null!;
    public int Miktar { get; set; }

    public DateTime Tarih { get; set; }
    public string? Aciklama { get; set; }

    public Urun Urun { get; set; } = null!;
}