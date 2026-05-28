namespace GiyimMagazasiERP.Models;

public class TedarikciAltKategori
{
    public int Id { get; set; }

    public int TedarikciId { get; set; }
    public int AltKategoriId { get; set; }

    public bool AktifMi { get; set; } = true;
    public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;

    public Tedarikci Tedarikci { get; set; } = null!;
    public AltKategori AltKategori { get; set; } = null!;
}