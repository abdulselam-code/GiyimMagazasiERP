using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class FinansHareketleriController : Controller
{
    private readonly AppDbContext _context;

    public FinansHareketleriController(AppDbContext context)
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

        var query = _context.FinansHareketleri
            .Include(x => x.Kullanici)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.HareketTipi.Contains(arama) ||
                x.Kategori.Contains(arama) ||
                x.Aciklama.Contains(arama) ||
                x.Kullanici.KullaniciAdi.Contains(arama));
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

        var model = new PagedResultViewModel<FinansHareketi>
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

        var hareket = await _context.FinansHareketleri
            .Include(x => x.Kullanici)
            .Include(x => x.Satis)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (hareket is null)
            return NotFound();

        return View(hareket);
    }

    public async Task<IActionResult> Create()
    {
        await DropdownlariDoldur();

        return View(new FinansHareketi
        {
            Tarih = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("SatisId,KullaniciId,HareketTipi,Kategori,Tutar,Tarih,Aciklama")]
        FinansHareketi hareket)
    {
        ModelState.Remove("Kullanici");
        ModelState.Remove("Satis");

        if (ModelState.IsValid)
        {
            _context.FinansHareketleri.Add(hareket);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await DropdownlariDoldur(hareket.KullaniciId, hareket.SatisId, hareket.HareketTipi);
        return View(hareket);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var hareket = await _context.FinansHareketleri.FindAsync(id);

        if (hareket is null)
            return NotFound();

        await DropdownlariDoldur(hareket.KullaniciId, hareket.SatisId, hareket.HareketTipi);
        return View(hareket);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,SatisId,KullaniciId,HareketTipi,Kategori,Tutar,Tarih,Aciklama")]
        FinansHareketi hareket)
    {
        if (id != hareket.Id)
            return NotFound();

        ModelState.Remove("Kullanici");
        ModelState.Remove("Satis");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(hareket);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.FinansHareketleri.AnyAsync(x => x.Id == hareket.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await DropdownlariDoldur(hareket.KullaniciId, hareket.SatisId, hareket.HareketTipi);
        return View(hareket);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var hareket = await _context.FinansHareketleri
            .Include(x => x.Kullanici)
            .Include(x => x.Satis)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (hareket is null)
            return NotFound();

        return View(hareket);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var hareket = await _context.FinansHareketleri.FindAsync(id);

        if (hareket is not null)
        {
            _context.FinansHareketleri.Remove(hareket);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task DropdownlariDoldur(
        int? kullaniciId = null,
        int? satisId = null,
        string? hareketTipi = null)
    {
        ViewData["KullaniciId"] = new SelectList(
            await _context.Kullanicilar
                .OrderBy(x => x.KullaniciAdi)
                .ToListAsync(),
            "Id",
            "KullaniciAdi",
            kullaniciId);

        var satislar = await _context.Satislar
            .OrderByDescending(x => x.SatisTarihi)
            .Select(x => new
            {
                x.Id,
                Gorunum = "Satış #" + x.Id + " - " + x.NetTutar + " TL"
            })
            .ToListAsync();

        ViewData["SatisId"] = new SelectList(
            satislar,
            "Id",
            "Gorunum",
            satisId);

        ViewData["HareketTipleri"] = new SelectList(
            new[] { "Gelir", "Gider" },
            hareketTipi);
    }
}