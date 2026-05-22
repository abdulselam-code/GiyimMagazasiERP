using System.Diagnostics;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var toplamGelir = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gelir")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var toplamGider = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var viewModel = new HomeIndexViewModel
        {
            ToplamUrunSayisi = await _context.Urunler
                .AsNoTracking()
                .CountAsync(),

            ToplamMusteriSayisi = await _context.Musteriler
                .AsNoTracking()
                .CountAsync(),

            ToplamPersonelSayisi = await _context.Personeller
                .AsNoTracking()
                .CountAsync(),

            ToplamSatisSayisi = await _context.Satislar
                .AsNoTracking()
                .CountAsync(),

            KritikStokSayisi = await _context.Urunler
                .AsNoTracking()
                .CountAsync(x => x.AktifMi && x.StokMiktari <= x.MinimumStok),

            ToplamGelir = toplamGelir,
            ToplamGider = toplamGider,
            NetKazanc = toplamGelir - toplamGider
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}