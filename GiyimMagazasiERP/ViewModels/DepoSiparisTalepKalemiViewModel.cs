using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class DepoSiparisTalepKalemiViewModel
{
    [Required(ErrorMessage = "Ürün seçilmelidir.")]
    public int? UrunId { get; set; }
    public int? TedarikciId { get; set; }
    public int? UrunTedarikciId { get; set; }
    [Range(1, 100000, ErrorMessage = "Talep adedi 0'dan büyük olmalıdır.")]
    public int TalepAdedi { get; set; } = 1;
    [MaxLength(300, ErrorMessage = "Kalem açıklaması en fazla 300 karakter olabilir.")]
    public string? Aciklama { get; set; }
}
