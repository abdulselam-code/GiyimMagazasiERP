using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class SatisOlusturViewModel
{
    public int? MusteriId { get; set; }

    [Required(ErrorMessage = "Personel seçimi zorunludur.")]
    public int PersonelId { get; set; }

    [Required(ErrorMessage = "Ürün seçimi zorunludur.")]
    public int UrunId { get; set; }

    [Required(ErrorMessage = "Adet girilmelidir.")]
    [Range(1, 999, ErrorMessage = "Adet en az 1 olmalıdır.")]
    public int Adet { get; set; } = 1;

    [Required(ErrorMessage = "Ödeme tipi seçilmelidir.")]
    public string OdemeTipi { get; set; } = null!;
}