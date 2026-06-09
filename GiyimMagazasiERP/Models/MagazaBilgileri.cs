namespace GiyimMagazasiERP.Models;

public class MagazaBilgileri
{
    public int Id { get; set; }

    public string MagazaAdi { get; set; } = null!;
    public string? Adres { get; set; }
    public string? Telefon { get; set; }
    public string? Email { get; set; }
    public string? VergiDairesi { get; set; }
    public string? VergiNo { get; set; }
    public string? TicariUnvan { get; set; }
    public string? Il { get; set; }
    public string? Ilce { get; set; }
    public string? WebAdresi { get; set; }
    public string? MersisNo { get; set; }
    public string? TicaretSicilNo { get; set; }
    public DateTime? KurulusTarihi { get; set; }
    public bool AktifMi { get; set; } = true;
}