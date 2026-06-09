namespace GiyimMagazasiERP.Models;

public class ToptanSatisTalepHareketi
{
    public int Id { get; set; }

    public int ToptanSatisTalebiId { get; set; }
    public int? KullaniciId { get; set; }

    public string? OncekiDurum { get; set; }
    public string YeniDurum { get; set; } = "";

    public DateTime IslemTarihi { get; set; }
    public string? Aciklama { get; set; }

    public ToptanSatisTalebi ToptanSatisTalebi { get; set; } = null!;
    public Kullanici? Kullanici { get; set; }
}