namespace GiyimMagazasiERP.Models;

public class IadeDegisimTalebi
{
    public const string IslemTipiIade = "Iade";
    public const string IslemTipiDegisim = "Degisim";

    public const string DurumYoneticiOnayiBekliyor =
        "YoneticiOnayiBekliyor";

    public const string DurumMuhasebeOnayiBekliyor =
        "MuhasebeOnayiBekliyor";

    public const string DurumReddedildi = "Reddedildi";
    public const string DurumIptalEdildi = "IptalEdildi";
    public const string DurumTamamlandi = "Tamamlandi";

    public int Id { get; set; }

    public string TalepNo { get; set; } = "";
    public string? IadeBelgeNo { get; set; }

    public int SatisId { get; set; }
    public int? MusteriId { get; set; }

    public int? TalepEdenKullaniciId { get; set; }
    public int? TalepEdenPersonelId { get; set; }

    public string IslemTipi { get; set; } = IslemTipiIade;

    public string Durum { get; set; } =
        DurumYoneticiOnayiBekliyor;

    public DateTime TalepTarihi { get; set; } = DateTime.Now;
    public string? Aciklama { get; set; }

    public int? YoneticiOnaylayanKullaniciId { get; set; }
    public DateTime? YoneticiOnayTarihi { get; set; }

    public int? MuhasebeOnaylayanKullaniciId { get; set; }
    public DateTime? MuhasebeOnayTarihi { get; set; }

    public int? ReddedenKullaniciId { get; set; }
    public DateTime? RedTarihi { get; set; }
    public string? RedNedeni { get; set; }

    public int? IptalEdenKullaniciId { get; set; }
    public DateTime? IptalTarihi { get; set; }
    public string? IptalNedeni { get; set; }

    public DateTime? TamamlanmaTarihi { get; set; }
    public int? FinansHareketiId { get; set; }

    public decimal ToplamIadeTutari { get; set; }
    public decimal ToplamKdvTutari { get; set; }
    public decimal VergiHaricToplam { get; set; }
    public decimal VergiDahilToplam { get; set; }

    public string? OdemeTipiSnapshot { get; set; }

    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Satis Satis { get; set; } = null!;
    public Musteri? Musteri { get; set; }

    public Kullanici? TalepEdenKullanici { get; set; }
    public Personel? TalepEdenPersonel { get; set; }

    public Kullanici? YoneticiOnaylayanKullanici { get; set; }
    public Kullanici? MuhasebeOnaylayanKullanici { get; set; }

    public Kullanici? ReddedenKullanici { get; set; }
    public Kullanici? IptalEdenKullanici { get; set; }

    public FinansHareketi? FinansHareketi { get; set; }

    public ICollection<IadeDegisimTalepDetayi> Detaylar { get; set; }
        = new List<IadeDegisimTalepDetayi>();

    public ICollection<IadeDegisimTalepHareketi> Hareketler { get; set; }
        = new List<IadeDegisimTalepHareketi>();

    public ICollection<IadeDegisimYeniUrunDetayi> YeniUrunDetaylari { get; set; }
        = new List<IadeDegisimYeniUrunDetayi>();
}