using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Muhasebe")]
public class MusterilerController : Controller
{
    private readonly AppDbContext _context;

    public MusterilerController(AppDbContext context)
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

        var query = _context.Musteriler.AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.AdSoyad.Contains(arama) ||
                (x.Telefon != null && x.Telefon.Contains(arama)) ||
                (x.Email != null && x.Email.Contains(arama)) ||
                (x.KurumsalUnvan != null && x.KurumsalUnvan.Contains(arama)) ||
                (x.VKN != null && x.VKN.Contains(arama)) ||
                (x.TCKN != null && x.TCKN.Contains(arama)) ||
                (x.VergiDairesi != null && x.VergiDairesi.Contains(arama)) ||
                (x.Il != null && x.Il.Contains(arama)) ||
                (x.Ilce != null && x.Ilce.Contains(arama)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var musteriler = await query
            .OrderBy(x => x.AdSoyad)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var model = new PagedResultViewModel<Musteri>
        {
            Items = musteriler,
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

        var musteri = await _context.Musteriler
            .FirstOrDefaultAsync(x => x.Id == id);

        if (musteri is null)
            return NotFound();

        return View(musteri);
    }

    public IActionResult Create()
    {
        return View(new Musteri
        {
            KayitTarihi = DateTime.Now,
            MusteriTipi = "Bireysel"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("AdSoyad,Telefon,Email,MusteriTipi,KurumsalUnvan,Adres,Il,Ilce,TCKN,VKN,VergiDairesi,SadakatPuani,IndirimOrani,ToplamHarcama")]
        Musteri musteri)
    {
        musteri.KayitTarihi = DateTime.Now;

        musteri.MusteriTipi = string.IsNullOrWhiteSpace(musteri.MusteriTipi)
            ? "Bireysel"
            : musteri.MusteriTipi;

        if (ModelState.IsValid)
        {
            _context.Add(musteri);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(musteri);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var musteri = await _context.Musteriler.FindAsync(id);

        if (musteri is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(musteri.MusteriTipi))
            musteri.MusteriTipi = "Bireysel";

        return View(musteri);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,AdSoyad,Telefon,Email,MusteriTipi,KurumsalUnvan,Adres,Il,Ilce,TCKN,VKN,VergiDairesi,SadakatPuani,IndirimOrani,ToplamHarcama,KayitTarihi")]
        Musteri musteri)
    {
        if (id != musteri.Id)
            return NotFound();

        musteri.MusteriTipi = string.IsNullOrWhiteSpace(musteri.MusteriTipi)
            ? "Bireysel"
            : musteri.MusteriTipi;

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(musteri);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Musteriler.AnyAsync(x => x.Id == musteri.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(musteri);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var musteri = await _context.Musteriler
            .FirstOrDefaultAsync(x => x.Id == id);

        if (musteri is null)
            return NotFound();

        return View(musteri);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var musteri = await _context.Musteriler.FindAsync(id);

        if (musteri is not null)
        {
            try
            {
                _context.Musteriler.Remove(musteri);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Hata"] = "Bu müşteri satış kayıtlarında kullanıldığı için silinemedi.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}