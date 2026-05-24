using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,InsanKaynaklari")]
public class PersonellerController : Controller
{
    private readonly AppDbContext _context;

    public PersonellerController(AppDbContext context)
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

        var query = _context.Personeller.AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.AdSoyad.Contains(arama) ||
                x.Telefon.Contains(arama) ||
                x.Email.Contains(arama) ||
                x.Pozisyon.Contains(arama) ||
                x.Departman.Contains(arama));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var personeller = await query
            .OrderBy(x => x.AdSoyad)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResultViewModel<Personel>
        {
            Items = personeller,
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

        var personel = await _context.Personeller
            .FirstOrDefaultAsync(x => x.Id == id);

        if (personel is null)
            return NotFound();

        return View(personel);
    }

    public IActionResult Create()
    {
        return View(new Personel
        {
            AktifMi = true,
            IseBaslamaTarihi = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("AdSoyad,Telefon,Email,Pozisyon,Maas,PrimOrani,GirisSaati,CikisSaati,MesaiSaati,IzinGunu,Departman,AktifMi,IseBaslamaTarihi")]
        Personel personel)
    {
        if (ModelState.IsValid)
        {
            _context.Add(personel);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(personel);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var personel = await _context.Personeller.FindAsync(id);

        if (personel is null)
            return NotFound();

        return View(personel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,AdSoyad,Telefon,Email,Pozisyon,Maas,PrimOrani,GirisSaati,CikisSaati,MesaiSaati,IzinGunu,Departman,AktifMi,IseBaslamaTarihi")]
        Personel personel)
    {
        if (id != personel.Id)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(personel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Personeller.AnyAsync(x => x.Id == personel.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(personel);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var personel = await _context.Personeller
            .FirstOrDefaultAsync(x => x.Id == id);

        if (personel is null)
            return NotFound();

        return View(personel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var personel = await _context.Personeller.FindAsync(id);

        if (personel is not null)
        {
            try
            {
                _context.Personeller.Remove(personel);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Hata"] = "Bu personel satış kayıtlarında kullanıldığı için silinemedi.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}