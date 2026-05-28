namespace GiyimMagazasiERP.Models;

public class AltKategori
{
    public int Id { get; set; }

    public int KategoriId { get; set; }

    public string AltKategoriAdi { get; set; } = null!;
    public string? Aciklama { get; set; }

    public bool AktifMi { get; set; } = true;

    public ICollection<TedarikciAltKategori> TedarikciAltKategoriler { get; set; }
    = new List<TedarikciAltKategori>();
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    public Kategori Kategori { get; set; } = null!;

    public ICollection<Urun> Urunler { get; set; } = new List<Urun>();
}