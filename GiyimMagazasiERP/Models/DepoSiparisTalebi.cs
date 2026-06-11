using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class DepoSiparisTalebi
{
    public const string DurumOnayBekliyor = "OnayBekliyor";
    public const string DurumOnaylandi = "Onaylandi";
    public const string DurumReddedildi = "Reddedildi";
    public const string DurumIptalEdildi = "IptalEdildi";
    public const string DurumTeslimAlindi = "TeslimAlindi";

    public static readonly string[] Oncelikler = { "Dusuk", "Normal", "Yuksek", "Kritik" };

    public int Id { get; set; }
    [MaxLength(30)] public string TalepNo { get; set; } = "";
    public int TalepEdenKullaniciId { get; set; }
    public int? TalepEdenPersonelId { get; set; }
    public DateTime TalepTarihi { get; set; }
    [MaxLength(30)] public string Durum { get; set; } = DurumOnayBekliyor;
    [MaxLength(20)] public string Oncelik { get; set; } = "Normal";
    [MaxLength(500)] public string? Aciklama { get; set; }
    public int? OnaylayanKullaniciId { get; set; }
    public DateTime? OnayTarihi { get; set; }
    [MaxLength(500)] public string? RedNedeni { get; set; }
    public int? TeslimAlanKullaniciId { get; set; }
    public DateTime? TeslimAlmaTarihi { get; set; }
    public DateTime? IptalTarihi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Kullanici TalepEdenKullanici { get; set; } = null!;
    public Personel? TalepEdenPersonel { get; set; }
    public Kullanici? OnaylayanKullanici { get; set; }
    public Kullanici? TeslimAlanKullanici { get; set; }
    public ICollection<DepoSiparisTalepKalemi> Kalemler { get; set; } = new List<DepoSiparisTalepKalemi>();
}
