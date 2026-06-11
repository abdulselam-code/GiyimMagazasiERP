using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class PersonelMesaiOlusturViewModel
{
    [Required(ErrorMessage = "Personel seçilmelidir.")]
    [Display(Name = "Personel")]
    public int? PersonelId { get; set; }

    [Required(ErrorMessage = "Tarih seçilmelidir.")]
    [DataType(DataType.Date)]
    [Display(Name = "Tarih")]
    public DateTime? Tarih { get; set; }

    [Required(ErrorMessage = "Vardiya başlangıç saati seçilmelidir.")]
    [DataType(DataType.Time)]
    [Display(Name = "Vardiya Başlangıç")]
    public TimeSpan? VardiyaBaslangic { get; set; }

    [Required(ErrorMessage = "Vardiya bitiş saati seçilmelidir.")]
    [DataType(DataType.Time)]
    [Display(Name = "Vardiya Bitiş")]
    public TimeSpan? VardiyaBitis { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Gerçek Giriş")]
    public TimeSpan? GercekGiris { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Gerçek Çıkış")]
    public TimeSpan? GercekCikis { get; set; }

    [Required(ErrorMessage = "Mesai türü seçilmelidir.")]
    [Display(Name = "Mesai Türü")]
    public string MesaiTuru { get; set; } = "";

    [MaxLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }
}
