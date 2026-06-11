using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class KasaKapanisi
{
    public const string DurumHazirlandi = "Hazirlandi";
    public const string DurumOnaylandi = "Onaylandi";
    public const string DurumReddedildi = "Reddedildi";

    public int Id { get; set; }

    [MaxLength(30)]
    public string KapanisNo { get; set; } = "";

    public int KasaPersonelId { get; set; }
    public int KasaKullaniciId { get; set; }
    public DateTime Tarih { get; set; }

    public decimal BeklenenNakit { get; set; }
    public decimal BeklenenKrediKarti { get; set; }
    public decimal BeklenenHavale { get; set; }
    public decimal BeklenenToplam { get; set; }

    public decimal SayilanNakit { get; set; }
    public decimal SayilanKrediKarti { get; set; }
    public decimal SayilanHavale { get; set; }
    public decimal SayilanToplam { get; set; }

    public decimal FarkNakit { get; set; }
    public decimal FarkKrediKarti { get; set; }
    public decimal FarkHavale { get; set; }
    public decimal FarkToplam { get; set; }

    public int SatisSayisi { get; set; }
    public int IadeSayisi { get; set; }
    public decimal IadeToplami { get; set; }

    [MaxLength(30)]
    public string Durum { get; set; } = DurumHazirlandi;

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    public int? OnaylayanKullaniciId { get; set; }
    public DateTime? OnayTarihi { get; set; }

    [MaxLength(500)]
    public string? RedNedeni { get; set; }

    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Personel KasaPersonel { get; set; } = null!;
    public Kullanici KasaKullanici { get; set; } = null!;
    public Kullanici? OnaylayanKullanici { get; set; }
}
