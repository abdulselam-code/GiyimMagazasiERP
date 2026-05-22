namespace GiyimMagazasiERP.Models;

public class FinansHareketi
{
    public int Id { get; set; }

    public int? SatisId { get; set; }
    public int KullaniciId { get; set; }

    public string HareketTipi { get; set; } = null!;
    public string Kategori { get; set; } = null!;

    public decimal Tutar { get; set; }
    public DateTime Tarih { get; set; }

    public string? Aciklama { get; set; }

    public Satis? Satis { get; set; }
    public Kullanici Kullanici { get; set; } = null!;
}