using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class AuthController : Controller
{
    private readonly AppDbContext _context;
    private readonly PasswordHasher<Kullanici> _passwordHasher = new();

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var giris = model.KullaniciAdiVeyaEmail.Trim();

        var kullanici = await _context.Kullanicilar
            .FirstOrDefaultAsync(x =>
                x.AktifMi &&
                (x.KullaniciAdi == giris || x.Email == giris));

        if (kullanici is null || string.IsNullOrWhiteSpace(kullanici.SifreHash))
        {
            ModelState.AddModelError("", "Kullanıcı adı/email veya şifre hatalı.");
            return View(model);
        }

        var sonuc = _passwordHasher.VerifyHashedPassword(
            kullanici,
            kullanici.SifreHash,
            model.Sifre);

        if (sonuc == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Kullanıcı adı/email veya şifre hatalı.");
            return View(model);
        }

        if (sonuc == PasswordVerificationResult.SuccessRehashNeeded)
        {
            kullanici.SifreHash = _passwordHasher.HashPassword(kullanici, model.Sifre);
        }

        kullanici.SonGirisTarihi = DateTime.Now;
        await _context.SaveChangesAsync();

        var kullaniciAdi = !string.IsNullOrWhiteSpace(kullanici.AdSoyad)
            ? kullanici.AdSoyad
            : kullanici.KullaniciAdi;

        var email = kullanici.Email ?? "";
        var rol = !string.IsNullOrWhiteSpace(kullanici.Rol)
            ? kullanici.Rol
            : "Personel";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
            new(ClaimTypes.Name, kullaniciAdi),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, rol)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }
}