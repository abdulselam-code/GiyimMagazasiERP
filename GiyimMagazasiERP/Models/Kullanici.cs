namespace GiyimMagazasiERP.Models;

public class Kullanici
{
    public int Id { get; set; }

    public string KullaniciAdi { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string SifreHash { get; set; } = null!;
    public string Rol { get; set; } = null!;

    public bool AktifMi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }

    public ICollection<FinansHareketi> FinansHareketleri { get; set; }
        = new List<FinansHareketi>();
}