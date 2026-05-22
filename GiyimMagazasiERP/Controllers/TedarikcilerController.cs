using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class TedarikcilerController : Controller
{
    private readonly AppDbContext _context;

    public TedarikcilerController(AppDbContext context)
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

        var query = _context.Tedarikciler
            .Include(x => x.Urunler)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.FirmaAdi.Contains(arama) ||
                x.Telefon.Contains(arama) ||
                x.Email.Contains(arama) ||
                x.Adres.Contains(arama));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var tedarikciler = await query
            .OrderBy(x => x.FirmaAdi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResultViewModel<Tedarikci>
        {
            Items = tedarikciler,
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

        var tedarikci = await _context.Tedarikciler
            .Include(x => x.Urunler)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tedarikci is null) return NotFound();

        return View(tedarikci);
    }

    public IActionResult Create()
    {
        return View(new Tedarikci { AktifMi = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("FirmaAdi,Telefon,Email,Adres,IndirimOrani,AktifMi")] Tedarikci tedarikci)
    {
        if (ModelState.IsValid)
        {
            _context.Tedarikciler.Add(tedarikci);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(tedarikci);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();

        var tedarikci = await _context.Tedarikciler.FindAsync(id);

        if (tedarikci is null) return NotFound();

        return View(tedarikci);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,FirmaAdi,Telefon,Email,Adres,IndirimOrani,AktifMi")] Tedarikci tedarikci)
    {
        if (id != tedarikci.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(tedarikci);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Tedarikciler.AnyAsync(x => x.Id == tedarikci.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(tedarikci);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();

        var tedarikci = await _context.Tedarikciler
            .Include(x => x.Urunler)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tedarikci is null) return NotFound();

        return View(tedarikci);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tedarikci = await _context.Tedarikciler.FindAsync(id);

        if (tedarikci is not null)
        {
            try
            {
                _context.Tedarikciler.Remove(tedarikci);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Hata"] = "Bu tedarikçiye bağlı ürünler olduğu için silinemedi.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}