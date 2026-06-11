using System.Globalization;
using GiyimMagazasiERP.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var turkceKultur = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentCulture = turkceKultur;
CultureInfo.DefaultThreadCurrentUICulture = turkceKultur;

// MVC servisleri
builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        alanAdi => $"{alanAdi} alanına geçerli bir sayı giriniz.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (deger, alanAdi) =>
            $"{alanAdi} alanına geçerli bir değer giriniz.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        deger => $"'{deger}' geçerli bir değer değildir.");
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture =
        new RequestCulture(turkceKultur);
    options.SupportedCultures = new[] { turkceKultur };
    options.SupportedUICultures = new[] { turkceKultur };
});

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Rol bazlı yetkilendirme
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// SQL Server + Entity Framework Core
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

var app = builder.Build();

// Hata yönetimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization();

app.UseRouting();

// Sıralama önemli:
// Önce Authentication, sonra Authorization
app.UseAuthentication();
app.UseAuthorization();

// Varsayılan route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
