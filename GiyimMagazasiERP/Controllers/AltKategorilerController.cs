using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
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

    public async Task<IActionResult> Index(
      string? arama,
      int? kategoriId,
      string durum = "Tumu",
      int page = 1,
      int pageSize = 10)
    {
        if (page < 1)
            page = 1;

        if (pageSize != 10 && pageSize != 20 && pageSize != 50)
            pageSize = 10;

        durum = string.IsNullOrWhiteSpace(durum) ? "Tumu" : durum;

        var sorgu = _context.AltKategoriler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var aranan = arama.Trim();

            sorgu = sorgu.Where(x =>
                x.AltKategoriAdi.Contains(aranan) ||
                (x.Aciklama != null && x.Aciklama.Contains(aranan)) ||
                x.Kategori.KategoriAdi.Contains(aranan));
        }

        if (kategoriId.HasValue)
        {
            sorgu = sorgu.Where(x => x.KategoriId == kategoriId.Value);
        }

        if (durum == "Aktif")
        {
            sorgu = sorgu.Where(x => x.AktifMi);
        }
        else if (durum == "Pasif")
        {
            sorgu = sorgu.Where(x => !x.AktifMi);
        }

        var totalCount = await sorgu.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var items = await sorgu
            .OrderBy(x => x.Kategori.KategoriAdi)
            .ThenBy(x => x.AltKategoriAdi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var kategoriler = await _context.Kategoriler
     .AsNoTracking()
     .OrderBy(x => x.KategoriAdi)
     .Select(x => new SelectListItem
     {
         Value = x.Id.ToString(),
         Text = x.KategoriAdi,
         Selected = kategoriId.HasValue && x.Id == kategoriId.Value
     })
     .ToListAsync();

        var model = new AltKategoriListeViewModel
        {
            Arama = arama,
            KategoriId = kategoriId,
            Durum = durum,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Items = items,
            Kategoriler = kategoriler
        };

        return View(model);
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