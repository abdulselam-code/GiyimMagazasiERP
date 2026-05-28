using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Depo")]
public class AltKategorilerController : Controller
{
    private readonly AppDbContext _context;

    public AltKategorilerController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? arama)
    {
        var query = _context.AltKategoriler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            query = query.Where(x =>
                x.AltKategoriAdi.Contains(arama) ||
                x.Kategori.KategoriAdi.Contains(arama) ||
                (x.Aciklama != null && x.Aciklama.Contains(arama)));
        }

        var altKategoriler = await query
            .OrderBy(x => x.Kategori.KategoriAdi)
            .ThenBy(x => x.AltKategoriAdi)
            .ToListAsync();

        ViewBag.Arama = arama;

        return View(altKategoriler);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var altKategori = await _context.AltKategoriler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (altKategori is null)
            return NotFound();

        return View(altKategori);
    }

    public async Task<IActionResult> Create()
    {
        await KategorileriDoldur();
        return View(new AltKategori
        {
            AktifMi = true,
            OlusturmaTarihi = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("KategoriId,AltKategoriAdi,Aciklama,AktifMi")]
        AltKategori altKategori)
    {
        altKategori.OlusturmaTarihi = DateTime.Now;

        ModelState.Remove("Kategori");
        ModelState.Remove("Urunler");

        if (ModelState.IsValid)
        {
            var tekrarVarMi = await _context.AltKategoriler.AnyAsync(x =>
                x.KategoriId == altKategori.KategoriId &&
                x.AltKategoriAdi == altKategori.AltKategoriAdi);

            if (tekrarVarMi)
            {
                ModelState.AddModelError("", "Bu ana kategori altında aynı isimde alt kategori zaten var.");
                await KategorileriDoldur(altKategori.KategoriId);
                return View(altKategori);
            }

            _context.Add(altKategori);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        await KategorileriDoldur(altKategori.KategoriId);
        return View(altKategori);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var altKategori = await _context.AltKategoriler.FindAsync(id);

        if (altKategori is null)
            return NotFound();

        await KategorileriDoldur(altKategori.KategoriId);

        return View(altKategori);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,KategoriId,AltKategoriAdi,Aciklama,AktifMi,OlusturmaTarihi")]
        AltKategori altKategori)
    {
        if (id != altKategori.Id)
            return NotFound();

        ModelState.Remove("Kategori");
        ModelState.Remove("Urunler");

        if (ModelState.IsValid)
        {
            var tekrarVarMi = await _context.AltKategoriler.AnyAsync(x =>
                x.Id != altKategori.Id &&
                x.KategoriId == altKategori.KategoriId &&
                x.AltKategoriAdi == altKategori.AltKategoriAdi);

            if (tekrarVarMi)
            {
                ModelState.AddModelError("", "Bu ana kategori altında aynı isimde alt kategori zaten var.");
                await KategorileriDoldur(altKategori.KategoriId);
                return View(altKategori);
            }

            try
            {
                _context.Update(altKategori);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.AltKategoriler.AnyAsync(x => x.Id == altKategori.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        await KategorileriDoldur(altKategori.KategoriId);
        return View(altKategori);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DurumDegistir(int id)
    {
        var altKategori = await _context.AltKategoriler.FindAsync(id);

        if (altKategori is null)
            return NotFound();

        altKategori.AktifMi = !altKategori.AktifMi;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task KategorileriDoldur(int? kategoriId = null)
    {
        ViewData["KategoriId"] = new SelectList(
            await _context.Kategoriler
                .AsNoTracking()
                .OrderBy(x => x.KategoriAdi)
                .ToListAsync(),
            "Id",
            "KategoriAdi",
            kategoriId);
    }
}