using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class DepoSiparisTalebiOlusturViewModel
{
    public const string TercihEnUygunFiyat = "EnUygunFiyat";
    public const string TercihEnHizliTeslimat = "EnHizliTeslimat";
    public const string TercihDengeli = "Dengeli";

    public static readonly string[] TedarikTercihleri =
    {
        TercihEnUygunFiyat,
        TercihEnHizliTeslimat,
        TercihDengeli
    };

    [Required(ErrorMessage = "Öncelik seçilmelidir.")]
    public string Oncelik { get; set; } = "Normal";

    [Required(ErrorMessage = "Tedarik tercihi seçilmelidir.")]
    public string TedarikTercihi { get; set; } = TercihDengeli;

    [MaxLength(500)] public string? Aciklama { get; set; }
    public List<DepoSiparisTalepKalemiViewModel> Kalemler { get; set; } =
        new() { new DepoSiparisTalepKalemiViewModel() };
}
