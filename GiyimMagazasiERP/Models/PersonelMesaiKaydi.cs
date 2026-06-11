using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GiyimMagazasiERP.Models;

public class PersonelMesaiKaydi
{
    public const string DurumOnayBekliyor = "OnayBekliyor";
    public const string DurumOnaylandi = "Onaylandi";
    public const string DurumReddedildi = "Reddedildi";
    public const string DurumIptalEdildi = "IptalEdildi";

    public static readonly string[] MesaiTurleri =
    {
        "Normal Vardiya",
        "Fazla Mesai",
        "Hafta Sonu Mesaisi",
        "Resmi Tatil Mesaisi",
        "Ek Vardiya",
        "Diğer"
    };

    public static readonly string[] PersonelTalepTurleri =
    {
        "Fazla Mesai",
        "Ek Vardiya"
    };

    public static readonly string[] FazlaMesaiKapsamindakiTurler =
    {
        "Fazla Mesai",
        "Hafta Sonu Mesaisi",
        "Resmi Tatil Mesaisi",
        "Ek Vardiya"
    };

    public int Id { get; set; }
    public int PersonelId { get; set; }
    public int KullaniciId { get; set; }

    public DateTime Tarih { get; set; }
    public TimeSpan VardiyaBaslangic { get; set; }
    public TimeSpan VardiyaBitis { get; set; }
    public TimeSpan? GercekGiris { get; set; }
    public TimeSpan? GercekCikis { get; set; }

    public decimal PlanlananSaat { get; set; }
    public decimal GerceklesenSaat { get; set; }
    public decimal FazlaMesaiSaati { get; set; }

    [MaxLength(50)]
    public string MesaiTuru { get; set; } = "";

    [MaxLength(30)]
    public string Durum { get; set; } = DurumOnayBekliyor;

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    public int? OnaylayanKullaniciId { get; set; }
    public DateTime? OnayTarihi { get; set; }

    [MaxLength(500)]
    public string? RedNedeni { get; set; }

    public DateTime? IptalTarihi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Personel Personel { get; set; } = null!;
    public Kullanici Kullanici { get; set; } = null!;
    public Kullanici? OnaylayanKullanici { get; set; }

    [NotMapped]
    public decimal GosterilecekFazlaMesaiSaati =>
        FazlaMesaiKapsamindaMi(MesaiTuru)
            ? FazlaMesaiSaati > 0
                ? FazlaMesaiSaati
                : GerceklesenSaat > 0
                    ? GerceklesenSaat
                    : PlanlananSaat
            : FazlaMesaiSaati;

    public static bool FazlaMesaiKapsamindaMi(string? mesaiTuru)
    {
        return !string.IsNullOrWhiteSpace(mesaiTuru) &&
               FazlaMesaiKapsamindakiTurler.Contains(mesaiTuru);
    }
}
