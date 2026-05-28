using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Depo")]

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
        if (id is null)
            return NotFound();

        var tedarikci = await _context.Tedarikciler
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id.Value);

        if (tedarikci is null)
            return NotFound();

        var urunler = await _context.Urunler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Include(x => x.AltKategori)
            .Where(x => x.TedarikciId == id.Value)
            .OrderBy(x => x.UrunAdi)
            .ToListAsync();

        var ilkUrunKayitTarihi = urunler.Any()
            ? urunler.Min(x => (DateTime?)x.OlusturmaTarihi)
            : null;

        var sonUrunKayitTarihi = urunler.Any()
            ? urunler.Max(x => (DateTime?)x.OlusturmaTarihi)
            : null;

        var sonStokHareketiTarihi = await _context.StokHareketleri
            .AsNoTracking()
            .Where(x => x.Urun.TedarikciId == id.Value)
            .OrderByDescending(x => x.Tarih)
            .Select(x => (DateTime?)x.Tarih)
            .FirstOrDefaultAsync();

        var toplamStokAlisDegeri = urunler.Sum(x => x.StokMiktari * x.AlisFiyati);
        var toplamStokSatisDegeri = urunler.Sum(x => x.StokMiktari * x.SatisFiyati);

        var ortalamaAlisFiyati = urunler.Any()
            ? urunler.Average(x => x.AlisFiyati)
            : 0;

        var ortalamaSatisFiyati = urunler.Any()
            ? urunler.Average(x => x.SatisFiyati)
            : 0;

        var ortalamaKarMarji = ortalamaAlisFiyati > 0
            ? ((ortalamaSatisFiyati - ortalamaAlisFiyati) / ortalamaAlisFiyati) * 100
            : 0;

        var model = new TedarikciDetayViewModel
        {
            Tedarikci = tedarikci,

            ToplamUrunCesidi = urunler.Count,
            ToplamStokAdedi = urunler.Sum(x => x.StokMiktari),
            KritikStokSayisi = urunler.Count(x => x.StokMiktari <= x.MinimumStok),

            ToplamStokAlisDegeri = toplamStokAlisDegeri,
            ToplamStokSatisDegeri = toplamStokSatisDegeri,
            ToplamMaliyet = toplamStokAlisDegeri,
            OrtalamaAlisFiyati = ortalamaAlisFiyati,
            OrtalamaSatisFiyati = ortalamaSatisFiyati,
            OrtalamaKarMarji = ortalamaKarMarji,
            TedarikciIndirimOrani = tedarikci.IndirimOrani,

            IlkUrunKayitTarihi = ilkUrunKayitTarihi,
            SonUrunKayitTarihi = sonUrunKayitTarihi,
            SonStokHareketiTarihi = sonStokHareketiTarihi,
            YaklasikCalismaSuresi = CalismaSuresiHesapla(ilkUrunKayitTarihi),

            Urunler = urunler.Select(x => new TedarikciUrunDetayViewModel
            {
                Id = x.Id,
                UrunAdi = x.UrunAdi,
                Barkod = x.Barkod,
                AnaKategori = x.Kategori.KategoriAdi,
                AltKategori = x.AltKategori != null ? x.AltKategori.AltKategoriAdi : "-",
                Beden = x.Beden,
                Renk = x.Renk,
                AlisFiyati = x.AlisFiyati,
                SatisFiyati = x.SatisFiyati,
                KarMarji = x.AlisFiyati > 0
                    ? ((x.SatisFiyati - x.AlisFiyati) / x.AlisFiyati) * 100
                    : null,
                StokMiktari = x.StokMiktari,
                MinimumStok = x.MinimumStok,
                AktifMi = x.AktifMi
            }).ToList()
        };

        ViewData["Kategoriler"] = await _context.Kategoriler
    .AsNoTracking()
    .OrderBy(x => x.KategoriAdi)
    .ToListAsync();

        ViewData["AltKategoriler"] = await _context.AltKategoriler
            .AsNoTracking()
            .Include(x => x.Kategori)
            .Where(x => x.AktifMi)
            .OrderBy(x => x.Kategori.KategoriAdi)
            .ThenBy(x => x.AltKategoriAdi)
            .ToListAsync();

        ViewData["TedarikciAltKategoriler"] = await _context.TedarikciAltKategoriler
            .AsNoTracking()
            .Include(x => x.AltKategori)
                .ThenInclude(x => x.Kategori)
            .Where(x => x.TedarikciId == id.Value)
            .OrderBy(x => x.AltKategori.Kategori.KategoriAdi)
            .ThenBy(x => x.AltKategori.AltKategoriAdi)
            .ToListAsync();


        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AltKategoriEkle(int tedarikciId, int altKategoriId)
    {
        var tedarikciVarMi = await _context.Tedarikciler
            .AnyAsync(x => x.Id == tedarikciId);

        if (!tedarikciVarMi)
            return NotFound();

        var altKategoriVarMi = await _context.AltKategoriler
            .AnyAsync(x => x.Id == altKategoriId);

        if (!altKategoriVarMi)
            return NotFound();

        var mevcutIliski = await _context.TedarikciAltKategoriler
            .FirstOrDefaultAsync(x =>
                x.TedarikciId == tedarikciId &&
                x.AltKategoriId == altKategoriId);

        if (mevcutIliski is not null)
        {
            if (!mevcutIliski.AktifMi)
            {
                mevcutIliski.AktifMi = true;
                await _context.SaveChangesAsync();
                TempData["Basari"] = "Alt kategori tekrar aktif hale getirildi.";
            }
            else
            {
                TempData["Hata"] = "Bu alt kategori zaten tedarikçiye tanımlı.";
            }

            return RedirectToAction(nameof(Details), new { id = tedarikciId });
        }

        _context.TedarikciAltKategoriler.Add(new TedarikciAltKategori
        {
            TedarikciId = tedarikciId,
            AltKategoriId = altKategoriId,
            AktifMi = true,
            OlusturmaTarihi = DateTime.Now
        });

        await _context.SaveChangesAsync();

        TempData["Basari"] = "Alt kategori tedarikçiye eklendi.";
        return RedirectToAction(nameof(Details), new { id = tedarikciId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AltKategoriDurumDegistir(int id)
    {
        var iliski = await _context.TedarikciAltKategoriler
            .FirstOrDefaultAsync(x => x.Id == id);

        if (iliski is null)
            return NotFound();

        iliski.AktifMi = !iliski.AktifMi;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = iliski.TedarikciId });
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
    private static string CalismaSuresiHesapla(DateTime? baslangicTarihi)
    {
        if (!baslangicTarihi.HasValue)
            return "Hesaplanamadı";

        var bugun = DateTime.Today;
        var baslangic = baslangicTarihi.Value.Date;

        if (baslangic > bugun)
            return "Hesaplanamadı";

        var toplamAy = ((bugun.Year - baslangic.Year) * 12) + bugun.Month - baslangic.Month;

        if (bugun.Day < baslangic.Day)
            toplamAy--;

        if (toplamAy < 1)
            return "1 aydan az";

        var yil = toplamAy / 12;
        var ay = toplamAy % 12;

        if (yil > 0 && ay > 0)
            return $"{yil} yıl {ay} ay";

        if (yil > 0)
            return $"{yil} yıl";

        return $"{ay} ay";
    }
}