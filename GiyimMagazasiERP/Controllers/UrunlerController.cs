using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Depo")]
public class UrunlerController : Controller
{
    private readonly AppDbContext _context;

    public UrunlerController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? arama,
        int? kategoriId,
        int? altKategoriId,
        int? tedarikciId,
        string? durum,
        bool? kritikStok,
        int page = 1,
        int pageSize = 10)
    {
        var izinliPageSizeDegerleri = new[] { 5, 10, 25, 50, 100 };

        if (page < 1)
            page = 1;

        if (!izinliPageSizeDegerleri.Contains(pageSize))
            pageSize = 10;

        durum = string.IsNullOrWhiteSpace(durum)
            ? "hepsi"
            : durum.Trim().ToLowerInvariant();

        var query = _context.Urunler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Include(x => x.AltKategori)
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
                (x.AltKategori != null && x.AltKategori.AltKategoriAdi.Contains(arama)) ||
                x.Tedarikci.FirmaAdi.Contains(arama));
        }

        if (kategoriId.HasValue)
        {
            query = query.Where(x => x.KategoriId == kategoriId.Value);
        }

        if (altKategoriId.HasValue)
        {
            query = query.Where(x => x.AltKategoriId == altKategoriId.Value);
        }

        if (tedarikciId.HasValue)
        {
            query = query.Where(x => x.TedarikciId == tedarikciId.Value);
        }

        if (durum == "aktif")
        {
            query = query.Where(x => x.AktifMi);
        }
        else if (durum == "pasif")
        {
            query = query.Where(x => !x.AktifMi);
        }

        if (kritikStok == true)
        {
            query = query.Where(x => x.StokMiktari <= x.MinimumStok);
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

        await UrunListeFiltreleriniDoldur(
            kategoriId,
            altKategoriId,
            tedarikciId,
            durum,
            kritikStok);

        return View(model);
    }

    private async Task UrunListeFiltreleriniDoldur(
        int? kategoriId,
        int? altKategoriId,
        int? tedarikciId,
        string? durum,
        bool? kritikStok)
    {
        ViewData["KategoriId"] = kategoriId;
        ViewData["AltKategoriId"] = altKategoriId;
        ViewData["TedarikciId"] = tedarikciId;
        ViewData["Durum"] = string.IsNullOrWhiteSpace(durum) ? "hepsi" : durum;
        ViewData["KritikStok"] = kritikStok == true;

        ViewData["Kategoriler"] = await _context.Kategoriler
            .AsNoTracking()
            .OrderBy(x => x.KategoriAdi)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.KategoriAdi,
                Selected = kategoriId.HasValue && x.Id == kategoriId.Value
            })
            .ToListAsync();

        ViewData["AltKategoriler"] = await _context.AltKategoriler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Where(x => x.AktifMi)
            .OrderBy(x => x.Kategori.KategoriAdi)
            .ThenBy(x => x.AltKategoriAdi)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Kategori.KategoriAdi + " - " + x.AltKategoriAdi,
                Selected = altKategoriId.HasValue && x.Id == altKategoriId.Value
            })
            .ToListAsync();

        ViewData["AltKategorilerJson"] = await _context.AltKategoriler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Where(x => x.AktifMi)
            .OrderBy(x => x.Kategori.KategoriAdi)
            .ThenBy(x => x.AltKategoriAdi)
            .Select(x => new
            {
                x.Id,
                x.KategoriId,
                x.AltKategoriAdi,
                KategoriAdi = x.Kategori.KategoriAdi
            })
            .ToListAsync();

        ViewData["Tedarikciler"] = await _context.Tedarikciler
            .AsNoTracking()
            .OrderBy(x => x.FirmaAdi)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.FirmaAdi,
                Selected = tedarikciId.HasValue && x.Id == tedarikciId.Value
            })
            .ToListAsync();
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null)
            return NotFound();

        var urun = await _context.Urunler
            .Include(x => x.Kategori)
            .Include(x => x.AltKategori)
            .Include(x => x.Tedarikci)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (urun is null)
            return NotFound();

        return View(urun);
    }

    public async Task<IActionResult> Create()
    {
        await DropdownlariDoldur();

        return View(new Urun
        {
            AktifMi = true,
            KdvOrani = 20m,
            OlusturmaTarihi = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("UrunAdi,Barkod,KategoriId,AltKategoriId,TedarikciId,Beden,Renk,AlisFiyati,SatisFiyati,KdvOrani,StokMiktari,MinimumStok,AktifMi")]
        Urun urun)
    {
        urun.OlusturmaTarihi = DateTime.Now;

        ModelState.Remove("Kategori");
        ModelState.Remove("AltKategori");
        ModelState.Remove("Tedarikci");
        ModelState.Remove("SatisDetaylari");
        ModelState.Remove("StokHareketleri");

        if (urun.KdvOrani < 0 || urun.KdvOrani > 100)
        {
            ModelState.AddModelError(
                nameof(urun.KdvOrani),
                "KDV oranı 0 ile 100 arasında olmalıdır.");
        }

        await TedarikciAltKategoriUyumunuKontrolEt(urun);

        var barkodVarMi = await _context.Urunler
            .AnyAsync(x => x.Barkod == urun.Barkod);

        if (barkodVarMi)
        {
            ModelState.AddModelError(
                nameof(urun.Barkod),
                "Bu barkod zaten başka bir üründe kullanılıyor.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(urun);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        await DropdownlariDoldur(urun.KategoriId, urun.TedarikciId, urun.AltKategoriId);
        return View(urun);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null)
            return NotFound();

        var urun = await _context.Urunler.FindAsync(id);

        if (urun is null)
            return NotFound();

        if (urun.KdvOrani < 0)
            urun.KdvOrani = 20m;

        await DropdownlariDoldur(urun.KategoriId, urun.TedarikciId, urun.AltKategoriId);
        return View(urun);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,UrunAdi,Barkod,KategoriId,AltKategoriId,TedarikciId,Beden,Renk,AlisFiyati,SatisFiyati,KdvOrani,StokMiktari,MinimumStok,AktifMi,OlusturmaTarihi")]
        Urun urun)
    {
        if (id != urun.Id)
            return NotFound();

        ModelState.Remove("Kategori");
        ModelState.Remove("AltKategori");
        ModelState.Remove("Tedarikci");
        ModelState.Remove("SatisDetaylari");
        ModelState.Remove("StokHareketleri");

        if (urun.KdvOrani < 0 || urun.KdvOrani > 100)
        {
            ModelState.AddModelError(
                nameof(urun.KdvOrani),
                "KDV oranı 0 ile 100 arasında olmalıdır.");
        }

        await TedarikciAltKategoriUyumunuKontrolEt(urun);

        var barkodVarMi = await _context.Urunler
            .AnyAsync(x => x.Barkod == urun.Barkod && x.Id != urun.Id);

        if (barkodVarMi)
        {
            ModelState.AddModelError(
                nameof(urun.Barkod),
                "Bu barkod zaten başka bir üründe kullanılıyor.");
        }

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

        await DropdownlariDoldur(urun.KategoriId, urun.TedarikciId, urun.AltKategoriId);
        return View(urun);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null)
            return NotFound();

        var urun = await _context.Urunler
            .Include(x => x.Kategori)
            .Include(x => x.AltKategori)
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

    private async Task DropdownlariDoldur(
        int? kategoriId = null,
        int? tedarikciId = null,
        int? altKategoriId = null)
    {
        var kategoriler = await _context.Kategoriler
            .AsNoTracking()
            .OrderBy(x => x.KategoriAdi)
            .ToListAsync();

        ViewData["KategoriId"] = new SelectList(
            kategoriler,
            "Id",
            "KategoriAdi",
            kategoriId);

        var altKategoriler = await _context.AltKategoriler
            .AsNoTracking()
            .Where(x => x.AktifMi)
            .OrderBy(x => x.Kategori.KategoriAdi)
            .ThenBy(x => x.AltKategoriAdi)
            .Select(x => new
            {
                x.Id,
                x.KategoriId,
                x.AltKategoriAdi,
                KategoriAdi = x.Kategori.KategoriAdi,
                Gorunum = x.Kategori.KategoriAdi + " - " + x.AltKategoriAdi
            })
            .ToListAsync();

        ViewData["AltKategoriId"] = new SelectList(
            altKategoriler,
            "Id",
            "AltKategoriAdi",
            altKategoriId);

        ViewData["AltKategorilerJson"] = altKategoriler;

        var tedarikciAltKategoriler = await _context.TedarikciAltKategoriler
            .AsNoTracking()
            .Include(x => x.AltKategori)
            .Where(x => x.AktifMi && x.AltKategori.AktifMi)
            .Select(x => new
            {
                x.TedarikciId,
                x.AltKategoriId,
                KategoriId = x.AltKategori.KategoriId
            })
            .ToListAsync();

        ViewData["TedarikciAltKategorilerJson"] = tedarikciAltKategoriler;

        ViewData["TedarikciId"] = new SelectList(
            await _context.Tedarikciler
                .AsNoTracking()
                .OrderBy(x => x.FirmaAdi)
                .ToListAsync(),
            "Id",
            "FirmaAdi",
            tedarikciId);
    }

    private async Task TedarikciAltKategoriUyumunuKontrolEt(Urun urun)
    {
        if (!urun.AltKategoriId.HasValue)
            return;

        var tedarikciIcinAktifTanimVarMi = await _context.TedarikciAltKategoriler
            .AnyAsync(x => x.TedarikciId == urun.TedarikciId && x.AktifMi);

        if (!tedarikciIcinAktifTanimVarMi)
            return;

        var uyumluMu = await _context.TedarikciAltKategoriler
            .AnyAsync(x =>
                x.TedarikciId == urun.TedarikciId &&
                x.AltKategoriId == urun.AltKategoriId.Value &&
                x.AktifMi);

        if (!uyumluMu)
        {
            ModelState.AddModelError(
                nameof(urun.AltKategoriId),
                "Seçilen alt kategori bu tedarikçi için tanımlı değil.");
        }
    }
}