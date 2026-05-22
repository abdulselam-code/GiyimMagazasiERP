namespace GiyimMagazasiERP.Models;

public class Kategori
{
    public int Id { get; set; }

    public string KategoriAdi { get; set; } = null!;
    public string? Aciklama { get; set; }

    public ICollection<Urun> Urunler { get; set; }
        = new List<Urun>();
}