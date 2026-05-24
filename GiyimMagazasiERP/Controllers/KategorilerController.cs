using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Depo")]
public class KategorilerController : Controller
{
    private readonly AppDbContext _context;

    public KategorilerController(AppDbContext context)
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

        var query = _context.Kategoriler
            .Include(x => x.Urunler)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.KategoriAdi.Contains(arama) ||
                x.Aciklama.Contains(arama));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var kategoriler = await query
            .OrderBy(x => x.KategoriAdi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResultViewModel<Kategori>
        {
            Items = kategoriler,
            Arama = arama,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();

        var kategori = await _context.Kategoriler
            .Include(x => x.Urunler)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kategori is null) return NotFound();

        return View(kategori);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("KategoriAdi,Aciklama")] Kategori kategori)
    {
        if (ModelState.IsValid)
        {
            _context.Kategoriler.Add(kategori);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(kategori);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var kategori = await _context.Kategoriler.FindAsync(id);

        if (kategori is null) return NotFound();

        return View(kategori);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,KategoriAdi,Aciklama")] Kategori kategori)
    {
        if (id != kategori.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(kategori);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Kategoriler.AnyAsync(x => x.Id == kategori.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(kategori);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var kategori = await _context.Kategoriler
            .Include(x => x.Urunler)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (kategori is null) return NotFound();

        return View(kategori);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var kategori = await _context.Kategoriler.FindAsync(id);

        if (kategori is not null)
        {
            try
            {
                _context.Kategoriler.Remove(kategori);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Hata"] = "Bu kategoriye bağlı ürünler olduğu için silinemedi.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}