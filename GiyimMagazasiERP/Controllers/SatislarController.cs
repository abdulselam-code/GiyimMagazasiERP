using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer,Muhasebe")]
public class SatislarController : Controller
{
    private readonly AppDbContext _context;

    public SatislarController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? arama, int page = 1, int pageSize = 10)
    {
        var izinliPageSizeDegerleri = new[] { 5, 10, 25, 50, 100 };

        if (page < 1)
            page = 1;

        if (!izinliPageSizeDegerleri.Contains(pageSize))
            pageSize = 10;

        var query = _context.Satislar
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                (x.Musteri != null && x.Musteri.AdSoyad.Contains(arama)) ||
                x.Personel.AdSoyad.Contains(arama) ||
                x.OdemeTipi.Contains(arama));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var satislar = await query
            .OrderByDescending(x => x.SatisTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResultViewModel<Satis>
        {
            Items = satislar,
            Arama = arama,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return View(model);
    }
}