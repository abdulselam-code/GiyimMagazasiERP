using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GiyimMagazasiERP.ViewModels;

namespace GiyimMagazasiERP.Controllers;

public class StokHareketleriController : Controller
{
    private readonly AppDbContext _context;

    public StokHareketleriController(AppDbContext context)
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

        var query = _context.StokHareketleri
            .Include(x => x.Urun)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.Urun.UrunAdi.Contains(arama) ||
                x.HareketTipi.Contains(arama) ||
                x.Aciklama.Contains(arama));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var hareketler = await query
            .OrderByDescending(x => x.Tarih)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResultViewModel<StokHareketi>
        {
            Items = hareketler,
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
        if (id is null)
            return NotFound();

        var hareket = await _context.StokHareketleri
            .Include(x => x.Urun)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (hareket is null)
            return NotFound();

        return View(hareket);
    }

    public async Task<IActionResult> Create()
    {
        await DropdownlariDoldur();

        return View(new StokHareketi
        {
            Tarih = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
    [Bind("UrunId,HareketTipi,Miktar,Tarih,Aciklama")] StokHareketi hareket)
    {
        ModelState.Remove("Urun");

        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(hareket.UrunId, hareket.HareketTipi);
            return View(hareket);
        }

        var urun = await _context.Urunler
            .FirstOrDefaultAsync(x => x.Id == hareket.UrunId);

        if (urun is null)
        {
            ModelState.AddModelError("", "Seçilen ürün bulunamadı.");
            await DropdownlariDoldur(hareket.UrunId, hareket.HareketTipi);
            return View(hareket);
        }

        var cikisHareketiMi =
            hareket.HareketTipi == "SatisCikis" ||
            hareket.HareketTipi == "FireCikis";

        if (cikisHareketiMi && urun.StokMiktari < hareket.Miktar)
        {
            ModelState.AddModelError("", $"Stok yetersiz. Mevcut stok: {urun.StokMiktari}");
            await DropdownlariDoldur(hareket.UrunId, hareket.HareketTipi);
            return View(hareket);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (hareket.HareketTipi == "Giris" || hareket.HareketTipi == "IadeGiris")
            {
                urun.StokMiktari += hareket.Miktar;
            }
            else if (cikisHareketiMi)
            {
                urun.StokMiktari -= hareket.Miktar;
            }

            _context.StokHareketleri.Add(hareket);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError("", "Stok hareketi kaydedilirken hata oluştu.");
            await DropdownlariDoldur(hareket.UrunId, hareket.HareketTipi);
            return View(hareket);
        }
    }
    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var hareket = await _context.StokHareketleri.FindAsync(id);

        if (hareket is null)
            return NotFound();

        await DropdownlariDoldur(hareket.UrunId, hareket.HareketTipi);
        return View(hareket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,UrunId,HareketTipi,Miktar,Tarih,Aciklama")] StokHareketi hareket)
    {
        if (id != hareket.Id)
            return NotFound();

        ModelState.Remove("Urun");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(hareket);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.StokHareketleri.AnyAsync(x => x.Id == hareket.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await DropdownlariDoldur(hareket.UrunId, hareket.HareketTipi);
        return View(hareket);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var hareket = await _context.StokHareketleri
            .Include(x => x.Urun)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (hareket is null)
            return NotFound();

        return View(hareket);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var hareket = await _context.StokHareketleri.FindAsync(id);

        if (hareket is not null)
        {
            _context.StokHareketleri.Remove(hareket);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task DropdownlariDoldur(int? urunId = null, string? hareketTipi = null)
    {
        ViewData["UrunId"] = new SelectList(
            await _context.Urunler
                .OrderBy(x => x.UrunAdi)
                .ToListAsync(),
            "Id",
            "UrunAdi",
            urunId);

        ViewData["HareketTipleri"] = new SelectList(
            new[] { "Giris", "SatisCikis", "IadeGiris", "FireCikis", "Duzeltme" },
            hareketTipi);
    }
}