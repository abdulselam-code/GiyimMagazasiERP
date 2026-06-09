namespace GiyimMagazasiERP.Models;

public class ToptanSatisTalebi
{
    public const string DurumYoneticiOnayiBekliyor = "YoneticiOnayiBekliyor";
    public const string DurumMuhasebeOnayiBekliyor = "MuhasebeOnayiBekliyor";
    public const string DurumReddedildi = "Reddedildi";
    public const string DurumSatisaDonusturuldu = "SatisaDonusturuldu";

    public int Id { get; set; }

    public string TalepNo { get; set; } = "";
    public int MusteriId { get; set; }

    public int? TalepEdenPersonelId { get; set; }
    public int? TalepEdenKullaniciId { get; set; }

    public string OdemeTipi { get; set; } = "";
    public string? Aciklama { get; set; }

    public string Durum { get; set; } = DurumYoneticiOnayiBekliyor;
    public DateTime TalepTarihi { get; set; }

    public int? YoneticiOnaylayanKullaniciId { get; set; }
    public DateTime? YoneticiOnayTarihi { get; set; }

    public int? MuhasebeOnaylayanKullaniciId { get; set; }
    public DateTime? MuhasebeOnayTarihi { get; set; }

    public int? ReddedenKullaniciId { get; set; }
    public DateTime? RedTarihi { get; set; }
    public string? RedNedeni { get; set; }

    public decimal ToplamTutar { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal NetTutar { get; set; }
    public decimal ToplamKdvTutari { get; set; }
    public decimal VergiHaricToplam { get; set; }
    public decimal VergiDahilToplam { get; set; }

    public int? SatisId { get; set; }
    public DateTime? SatisaDonusturulmeTarihi { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Musteri Musteri { get; set; } = null!;
    public Personel? TalepEdenPersonel { get; set; }
    public Kullanici? TalepEdenKullanici { get; set; }
    public Kullanici? YoneticiOnaylayanKullanici { get; set; }
    public Kullanici? MuhasebeOnaylayanKullanici { get; set; }
    public Kullanici? ReddedenKullanici { get; set; }
    public Satis? Satis { get; set; }

    public ICollection<ToptanSatisTalepDetayi> Detaylar { get; set; }
        = new List<ToptanSatisTalepDetayi>();

    public ICollection<ToptanSatisTalepHareketi> Hareketler { get; set; }
        = new List<ToptanSatisTalepHareketi>();
}