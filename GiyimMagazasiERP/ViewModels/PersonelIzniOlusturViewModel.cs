using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class PersonelIzniOlusturViewModel
{
    [Required(ErrorMessage = "Personel seçilmelidir.")]
    [Display(Name = "Personel")]
    public int? PersonelId { get; set; }

    [Required(ErrorMessage = "İzin türü seçilmelidir.")]
    [Display(Name = "İzin Türü")]
    public string IzinTuru { get; set; } = "";

    [Required(ErrorMessage = "Başlangıç tarihi seçilmelidir.")]
    [DataType(DataType.Date)]
    [Display(Name = "Başlangıç Tarihi")]
    public DateTime? BaslangicTarihi { get; set; }

    [Required(ErrorMessage = "Bitiş tarihi seçilmelidir.")]
    [DataType(DataType.Date)]
    [Display(Name = "Bitiş Tarihi")]
    public DateTime? BitisTarihi { get; set; }

    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }
}
