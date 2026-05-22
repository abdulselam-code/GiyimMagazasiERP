namespace GiyimMagazasiERP.Models;

public class Personel
{
    public int Id { get; set; }

    public string AdSoyad { get; set; } = null!;
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string Pozisyon { get; set; } = null!;

    public decimal Maas { get; set; }
    public decimal PrimOrani { get; set; }

    public TimeSpan? GirisSaati { get; set; }
    public TimeSpan? CikisSaati { get; set; }

    public decimal MesaiSaati { get; set; }
    public int IzinGunu { get; set; }

    public string Departman { get; set; } = null!;
    public bool AktifMi { get; set; }
    public DateTime IseBaslamaTarihi { get; set; }

    public ICollection<Satis> Satislar { get; set; }
        = new List<Satis>();
}