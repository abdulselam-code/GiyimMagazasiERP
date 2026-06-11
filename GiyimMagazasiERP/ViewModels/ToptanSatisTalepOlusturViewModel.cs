using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class ToptanSatisTalepOlusturViewModel
{
    [Required(ErrorMessage = "Müşteri seçilmelidir.")]
    public int? MusteriId { get; set; }

    public int? TalepEdenPersonelId { get; set; }

    [Required(ErrorMessage = "Ödeme tipi seçilmelidir.")]
    public string OdemeTipi { get; set; } = "";

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    public string? Aciklama { get; set; }

    public List<ToptanSatisTalepSepetUrunViewModel> Sepet { get; set; }
        = new();
}

public class ToptanSatisTalepSepetUrunViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    public int UrunId { get; set; }

    [Range(1, 999, ErrorMessage = "Ürün adedi en az 1 olmalıdır.")]
    public int Adet { get; set; }

    // Yalnızca ekran gösterimi içindir. Sunucu bu değerlere güvenmez.
    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
}
