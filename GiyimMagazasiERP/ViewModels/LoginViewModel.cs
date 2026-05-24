using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı veya email zorunludur.")]
    public string KullaniciAdiVeyaEmail { get; set; } = null!;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    public string Sifre { get; set; } = null!;

    public string? ReturnUrl { get; set; }
}