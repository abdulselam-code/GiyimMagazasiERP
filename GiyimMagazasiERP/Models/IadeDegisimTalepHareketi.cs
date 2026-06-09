namespace GiyimMagazasiERP.Models;

public class IadeDegisimTalepHareketi
{
    public int Id { get; set; }

    public int IadeDegisimTalebiId { get; set; }
    public int? KullaniciId { get; set; }

    public string? OncekiDurum { get; set; }

    public string YeniDurum { get; set; } =
        IadeDegisimTalebi.DurumYoneticiOnayiBekliyor;

    public DateTime IslemTarihi { get; set; } = DateTime.Now;

    public string? Aciklama { get; set; }

    public IadeDegisimTalebi IadeDegisimTalebi { get; set; }
        = null!;

    public Kullanici? Kullanici { get; set; }
}