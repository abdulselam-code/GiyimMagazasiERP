using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class PersonelIzni
{
    public const string DurumOnayBekliyor = "OnayBekliyor";
    public const string DurumOnaylandi = "Onaylandi";
    public const string DurumReddedildi = "Reddedildi";
    public const string DurumIptalEdildi = "IptalEdildi";

    public static readonly string[] IzinTurleri =
    {
        "Yıllık İzin",
        "Mazeret İzni",
        "Hastalık İzni",
        "Ücretsiz İzin",
        "Doğum İzni",
        "Diğer"
    };

    public int Id { get; set; }
    public int PersonelId { get; set; }
    public int KullaniciId { get; set; }

    [MaxLength(50)]
    public string IzinTuru { get; set; } = "";

    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int GunSayisi { get; set; }

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    [MaxLength(30)]
    public string Durum { get; set; } = DurumOnayBekliyor;

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
}
