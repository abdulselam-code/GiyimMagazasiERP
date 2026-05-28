namespace GiyimMagazasiERP.Models;

public class Tedarikci
{
    public int Id { get; set; }

    public string FirmaAdi { get; set; } = null!;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? Adres { get; set; }

    public decimal IndirimOrani { get; set; }
    public bool AktifMi { get; set; }

    public ICollection<TedarikciAltKategori> TedarikciAltKategoriler { get; set; }
    = new List<TedarikciAltKategori>();

    public ICollection<Urun> Urunler { get; set; }
        = new List<Urun>();
}