using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class SatisOlusturViewModel
{
    public string SatisTuru { get; set; } = "Perakende";

    public int? MusteriId { get; set; }

    public int? PersonelId { get; set; }

    [Required(ErrorMessage = "Ödeme tipi seçilmelidir.")]
    public string OdemeTipi { get; set; } = null!;

    public string? Aciklama { get; set; }

    public List<SatisSepetUrunViewModel> SepetUrunleri { get; set; } = new();
}