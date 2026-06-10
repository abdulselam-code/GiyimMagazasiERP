using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Muhasebe")]
public class FinansHareketleriController : Controller
{
    private readonly AppDbContext _context;

    public FinansHareketleriController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        string? arama,
        string hareketTipi = "Tumu",
        string kategori = "Tumu",
        DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null,
        int page = 1,
        int pageSize = 10)
    {
        var izinliPageSizeDegerleri = new[] { 10, 25, 50, 100 };

        if (page < 1)
            page = 1;

        if (!izinliPageSizeDegerleri.Contains(pageSize))
            pageSize = 10;

        if (hareketTipi is not ("Tumu" or "Gelir" or "Gider"))
            hareketTipi = "Tumu";

        arama = string.IsNullOrWhiteSpace(arama)
            ? null
            : arama.Trim();

        kategori = string.IsNullOrWhiteSpace(kategori)
            ? "Tumu"
            : kategori.Trim();

        var bugun = DateTime.Today;
        var yarin = bugun.AddDays(1);

        var ozetler = await _context.FinansHareketleri
            .AsNoTracking()
            .GroupBy(x => 1)
            .Select(g => new
            {
                ToplamGelir = g
                    .Where(x => x.HareketTipi == "Gelir")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                ToplamGider = g
                    .Where(x => x.HareketTipi == "Gider")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                SatisIadeleri = g
                    .Where(x =>
                        x.HareketTipi == "Gider" &&
                        x.Kategori == "Satış İadesi")
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                BugunkuGelir = g
                    .Where(x =>
                        x.HareketTipi == "Gelir" &&
                        x.Tarih >= bugun &&
                        x.Tarih < yarin)
                    .Sum(x => (decimal?)x.Tutar) ?? 0,
                BugunkuGider = g
                    .Where(x =>
                        x.HareketTipi == "Gider" &&
                        x.Tarih >= bugun &&
                        x.Tarih < yarin)
                    .Sum(x => (decimal?)x.Tutar) ?? 0
            })
            .FirstOrDefaultAsync();

        var kategoriler = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.Kategori != null && x.Kategori != "")
            .Select(x => x.Kategori)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var query = _context.FinansHareketleri
            .AsNoTracking()
            .Include(x => x.Kullanici)
            .AsQueryable();

        if (arama is not null)
        {
            query = query.Where(x =>
                x.HareketTipi.Contains(arama) ||
                x.Kategori.Contains(arama) ||
                (x.Aciklama != null &&
                 x.Aciklama.Contains(arama)) ||
                (x.SatisId.HasValue &&
                 x.SatisId.Value.ToString().Contains(arama)) ||
                x.Kullanici.KullaniciAdi.Contains(arama) ||
                (x.Kullanici.AdSoyad != null &&
                 x.Kullanici.AdSoyad.Contains(arama)));
        }

        if (hareketTipi != "Tumu")
            query = query.Where(x => x.HareketTipi == hareketTipi);

        if (kategori != "Tumu")
            query = query.Where(x => x.Kategori == kategori);

        if (baslangicTarihi.HasValue)
        {
            var baslangic = baslangicTarihi.Value.Date;
            query = query.Where(x => x.Tarih >= baslangic);
        }

        if (bitisTarihi.HasValue)
        {
            var bitisExclusive = bitisTarihi.Value.Date.AddDays(1);
            query = query.Where(x => x.Tarih < bitisExclusive);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var hareketListesi = await query
            .OrderByDescending(x => x.Tarih)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var sayfaliHareketler =
            new PagedResultViewModel<FinansHareketi>
        {
            Items = hareketListesi,
            Arama = arama,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        var model = new FinansHareketleriIndexViewModel
        {
            Hareketler = sayfaliHareketler,
            Arama = arama,
            HareketTipi = hareketTipi,
            Kategori = kategori,
            BaslangicTarihi = baslangicTarihi,
            BitisTarihi = bitisTarihi,
            Kategoriler = kategoriler,
            ToplamGelir = ozetler?.ToplamGelir ?? 0,
            ToplamGider = ozetler?.ToplamGider ?? 0,
            SatisIadeleriToplami = ozetler?.SatisIadeleri ?? 0,
            BugunkuNet =
                (ozetler?.BugunkuGelir ?? 0) -
                (ozetler?.BugunkuGider ?? 0)
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
