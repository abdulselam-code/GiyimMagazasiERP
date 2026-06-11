using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class UrunTedarikci
{
    public int Id { get; set; }
    public int UrunId { get; set; }
    public int TedarikciId { get; set; }

    [MaxLength(100)]
    public string? TedarikciUrunKodu { get; set; }

    public decimal BirimMaliyet { get; set; }
    public decimal IndirimOrani { get; set; }
    public decimal NetBirimMaliyet { get; set; }
    public int MinimumSiparisAdedi { get; set; } = 1;
    public int TeslimSuresiGun { get; set; }
    public bool VarsayilanMi { get; set; }
    public bool AktifMi { get; set; } = true;

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    public DateTime OlusturmaTarihi { get; set; }
    public DateTime? GuncellemeTarihi { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Urun Urun { get; set; } = null!;
    public Tedarikci Tedarikci { get; set; } = null!;
}
