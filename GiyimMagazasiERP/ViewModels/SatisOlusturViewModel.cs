using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class SatisOlusturViewModel
{
    [Required(ErrorMessage = "Satış işlemi için müşteri seçilmelidir.")]
    public int? MusteriId { get; set; }

    [Required(ErrorMessage = "Personel seçimi zorunludur.")]
    public int PersonelId { get; set; }

    [Required(ErrorMessage = "Ödeme tipi seçilmelidir.")]
    public string OdemeTipi { get; set; } = null!;

    public List<SatisSepetUrunViewModel> SepetUrunleri { get; set; } = new();
}