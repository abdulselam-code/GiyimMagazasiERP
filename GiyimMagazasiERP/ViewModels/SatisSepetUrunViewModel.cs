using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class SatisSepetUrunViewModel
{
    [Required]
    public int UrunId { get; set; }

    public string? UrunAdi { get; set; }
    public string? Barkod { get; set; }
    public string? Beden { get; set; }
    public string? Renk { get; set; }

    public decimal BirimFiyat { get; set; }

    [Range(1, 999, ErrorMessage = "Ürün adedi en az 1 olmalıdır.")]
    public int Adet { get; set; }

    public int StokMiktari { get; set; }

    public decimal AraToplam { get; set; }
    public decimal IndirimTutari { get; set; }
    public decimal KdvDahilToplam { get; set; }
}