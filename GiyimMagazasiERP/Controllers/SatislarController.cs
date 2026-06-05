using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer,Muhasebe")]
public class SatislarController : Controller
{
    private readonly AppDbContext _context;

    public SatislarController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
    string? arama,
    string? scope,
    string? tarih,
    string? donem,
    int page = 1,
    int pageSize = 10)
    {
        var izinliPageSizeDegerleri = new[] { 5, 10, 25, 50, 100 };

        if (page < 1)
            page = 1;

        if (!izinliPageSizeDegerleri.Contains(pageSize))
            pageSize = 10;

        var query = _context.Satislar
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .AsQueryable();

        if (User.IsInRole("Kasiyer"))
        {
            var personelId = await GirisYapanKullanicininPersonelIdGetir();

            if (!personelId.HasValue)
            {
                query = query.Where(x => false);
                ViewData["KasiyerUyari"] = "Bu kullanıcıya bağlı personel kaydı bulunamadı.";
            }
            else
            {
                query = query.Where(x => x.PersonelId == personelId.Value);
            }
        }
        else if (scope == "benim")
        {
            var personelId = await GirisYapanKullanicininPersonelIdGetir();

            if (personelId.HasValue)
                query = query.Where(x => x.PersonelId == personelId.Value);
        }

        if (tarih == "bugun")
        {
            var bugun = DateTime.Today;
            var yarin = bugun.AddDays(1);

            query = query.Where(x => x.SatisTarihi >= bugun && x.SatisTarihi < yarin);
        }

        if (donem == "buAy")
        {
            var bugun = DateTime.Today;
            var ayBaslangici = new DateTime(bugun.Year, bugun.Month, 1);
            var sonrakiAy = ayBaslangici.AddMonths(1);

            query = query.Where(x => x.SatisTarihi >= ayBaslangici && x.SatisTarihi < sonrakiAy);
        }

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

        ViewData["Scope"] = scope;
        ViewData["Tarih"] = tarih;
        ViewData["Donem"] = donem;

        return View(model);
    }
    private async Task<int?> GirisYapanKullanicininPersonelIdGetir()
    {
        var kullaniciIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(kullaniciIdText, out var kullaniciId))
            return null;

        return await _context.Kullanicilar
            .AsNoTracking()
            .Where(x => x.Id == kullaniciId && x.AktifMi)
            .Select(x => x.PersonelId)
            .FirstOrDefaultAsync();
    }
}