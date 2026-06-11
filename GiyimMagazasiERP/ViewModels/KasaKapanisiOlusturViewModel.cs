using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class KasaKapanisiOlusturViewModel
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime Tarih { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Kasiyer seçilmelidir.")]
    [Display(Name = "Kasiyer")]
    public int? KasaPersonelId { get; set; }

    public decimal BeklenenNakit { get; set; }
    public decimal BeklenenKrediKarti { get; set; }
    public decimal BeklenenHavale { get; set; }
    public decimal BeklenenToplam { get; set; }

    [Display(Name = "Sayılan nakit")]
    [Required(ErrorMessage = "Sayılan nakit alanı zorunludur.")]
    [Range(
        typeof(decimal),
        "0",
        "999999999,99",
        ErrorMessage = "Sayılan nakit alanına geçerli bir tutar giriniz.")]
    public decimal SayilanNakit { get; set; }

    [Display(Name = "Sayılan kredi kartı")]
    [Required(ErrorMessage = "Sayılan kredi kartı alanı zorunludur.")]
    [Range(
        typeof(decimal),
        "0",
        "999999999,99",
        ErrorMessage = "Sayılan kredi kartı alanına geçerli bir tutar giriniz.")]
    public decimal SayilanKrediKarti { get; set; }

    [Display(Name = "Sayılan havale")]
    [Required(ErrorMessage = "Sayılan havale alanı zorunludur.")]
    [Range(
        typeof(decimal),
        "0",
        "999999999,99",
        ErrorMessage = "Sayılan havale alanına geçerli bir tutar giriniz.")]
    public decimal SayilanHavale { get; set; }

    [MaxLength(500)]
    public string? Aciklama { get; set; }

    public int SatisSayisi { get; set; }
    public int IadeSayisi { get; set; }
    public decimal IadeToplami { get; set; }
    public decimal DagitilamayanIadeToplami { get; set; }

    public decimal SayilanToplam =>
        SayilanNakit + SayilanKrediKarti + SayilanHavale;
    public decimal FarkNakit => SayilanNakit - BeklenenNakit;
    public decimal FarkKrediKarti => SayilanKrediKarti - BeklenenKrediKarti;
    public decimal FarkHavale => SayilanHavale - BeklenenHavale;
    public decimal FarkToplam => SayilanToplam - BeklenenToplam;
}
