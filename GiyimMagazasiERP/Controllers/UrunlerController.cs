using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class UrunlerController : Controller
{
    private readonly AppDbContext _context;

    public UrunlerController(AppDbContext context)
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

        var query = _context.Urunler
            .Include(x => x.Kategori)
            .Include(x => x.Tedarikci)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.UrunAdi.Contains(arama) ||
                x.Barkod.Contains(arama) ||
                x.Beden.Contains(arama) ||
                x.Renk.Contains(arama) ||
                x.Kategori.KategoriAdi.Contains(arama) ||
                x.Tedarikci.FirmaAdi.Contains(arama));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var urunler = await query
            .OrderBy(x => x.UrunAdi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResultViewModel<Urun>
        {
            Items = urunler,
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

        var urun = await _context.Urunler
            .Include(x => x.Kategori)
            .Include(x => x.Tedarikci)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (urun is null)
            return NotFound();

        return View(urun);
    }

    public async Task<IActionResult> Create()
    {
        await DropdownlariDoldur();
        return View(new Urun { AktifMi = true, OlusturmaTarihi = DateTime.Now });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("UrunAdi,Barkod,KategoriId,TedarikciId,Beden,Renk,AlisFiyati,SatisFiyati,StokMiktari,MinimumStok,AktifMi")]
    Urun urun)
    {
        urun.OlusturmaTarihi = DateTime.Now;

        ModelState.Remove("Kategori");
        ModelState.Remove("Tedarikci");

        if (ModelState.IsValid)
        {
            _context.Add(urun);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await DropdownlariDoldur(urun.KategoriId, urun.TedarikciId);
        return View(urun);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var urun = await _context.Urunler.FindAsync(id);

        if (urun is null)
            return NotFound();

        await DropdownlariDoldur(urun.KategoriId, urun.TedarikciId);
        return View(urun);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,UrunAdi,Barkod,KategoriId,TedarikciId,Beden,Renk,AlisFiyati,SatisFiyati,StokMiktari,MinimumStok,AktifMi,OlusturmaTarihi")]
        Urun urun)
    {
        if (id != urun.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(urun);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Urunler.AnyAsync(x => x.Id == urun.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await DropdownlariDoldur(urun.KategoriId, urun.TedarikciId);
        return View(urun);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var urun = await _context.Urunler
            .Include(x => x.Kategori)
            .Include(x => x.Tedarikci)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (urun is null)
            return NotFound();

        return View(urun);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var urun = await _context.Urunler.FindAsync(id);

        if (urun is not null)
        {
            try
            {
                _context.Urunler.Remove(urun);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Hata"] = "Bu ürün satış veya stok hareketlerinde kullanıldığı için silinemedi.";
            }
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task DropdownlariDoldur(int? kategoriId = null, int? tedarikciId = null)
    {
        ViewData["KategoriId"] = new SelectList(
            await _context.Kategoriler.OrderBy(x => x.KategoriAdi).ToListAsync(),
            "Id",
            "KategoriAdi",
            kategoriId);

        ViewData["TedarikciId"] = new SelectList(
            await _context.Tedarikciler.OrderBy(x => x.FirmaAdi).ToListAsync(),
            "Id",
            "FirmaAdi",
            tedarikciId);
    }
}