using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Depo")]
public class UrunTedarikcileriController : Controller
{
    private readonly AppDbContext _context;

    public UrunTedarikcileriController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Create(int urunId)
    {
        var urun = await _context.Urunler.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == urunId);

        if (urun is null)
            return NotFound();

        await FormVerileriniDoldur(urunId);

        return View(new UrunTedarikci
        {
            UrunId = urunId,
            BirimMaliyet = urun.AlisFiyati,
            NetBirimMaliyet = urun.AlisFiyati,
            MinimumSiparisAdedi = 1,
            AktifMi = true,
            OlusturmaTarihi = DateTime.Now
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Create(
        [Bind("UrunId,TedarikciId,TedarikciUrunKodu,BirimMaliyet,IndirimOrani,MinimumSiparisAdedi,TeslimSuresiGun,VarsayilanMi,AktifMi,Aciklama")]
        UrunTedarikci model)
    {
        ModelState.Remove(nameof(model.Urun));
        ModelState.Remove(nameof(model.Tedarikci));
        ModelState.Remove(nameof(model.RowVersion));
        Dogrula(model);

        if (await _context.UrunTedarikcileri.AnyAsync(x =>
                x.UrunId == model.UrunId &&
                x.TedarikciId == model.TedarikciId))
        {
            ModelState.AddModelError(
                nameof(model.TedarikciId),
                "Bu tedarikçi ürünle daha önce ilişkilendirilmiş.");
        }

        if (!await _context.Urunler.AnyAsync(x => x.Id == model.UrunId))
            return NotFound();

        if (!await _context.Tedarikciler.AnyAsync(x => x.Id == model.TedarikciId))
            ModelState.AddModelError(nameof(model.TedarikciId), "Tedarikçi bulunamadı.");

        if (!ModelState.IsValid)
        {
            await FormVerileriniDoldur(model.UrunId, model.TedarikciId);
            return View(model);
        }

        await VarsayilaniTemizle(model.UrunId, model.VarsayilanMi);
        model.NetBirimMaliyet = NetMaliyetHesapla(model.BirimMaliyet, model.IndirimOrani);
        model.OlusturmaTarihi = DateTime.Now;

        _context.UrunTedarikcileri.Add(model);
        await _context.SaveChangesAsync();

        TempData["Basari"] = "Tedarikçi ürünle ilişkilendirildi.";
        return RedirectToAction("Details", "Urunler", new { id = model.UrunId });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _context.UrunTedarikcileri
            .AsNoTracking()
            .Include(x => x.Urun)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (model is null)
            return NotFound();

        await FormVerileriniDoldur(model.UrunId, model.TedarikciId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Edit(
        int id,
        string rowVersion,
        [Bind("Id,UrunId,TedarikciId,TedarikciUrunKodu,BirimMaliyet,IndirimOrani,MinimumSiparisAdedi,TeslimSuresiGun,VarsayilanMi,AktifMi,Aciklama,OlusturmaTarihi")]
        UrunTedarikci model)
    {
        if (id != model.Id)
            return NotFound();

        ModelState.Remove(nameof(model.Urun));
        ModelState.Remove(nameof(model.Tedarikci));
        ModelState.Remove(nameof(model.RowVersion));
        Dogrula(model);

        if (await _context.UrunTedarikcileri.AnyAsync(x =>
                x.Id != id &&
                x.UrunId == model.UrunId &&
                x.TedarikciId == model.TedarikciId))
        {
            ModelState.AddModelError(
                nameof(model.TedarikciId),
                "Bu tedarikçi ürünle daha önce ilişkilendirilmiş.");
        }

        if (!ModelState.IsValid)
        {
            await FormVerileriniDoldur(model.UrunId, model.TedarikciId);
            return View(model);
        }

        var mevcut = await _context.UrunTedarikcileri.FirstOrDefaultAsync(x => x.Id == id);
        if (mevcut is null)
            return NotFound();

        try
        {
            _context.Entry(mevcut).Property(x => x.RowVersion).OriginalValue =
                Convert.FromBase64String(rowVersion);
        }
        catch
        {
            ModelState.AddModelError("", "Kayıt sürüm bilgisi geçersiz.");
            await FormVerileriniDoldur(model.UrunId, model.TedarikciId);
            return View(model);
        }

        await VarsayilaniTemizle(model.UrunId, model.VarsayilanMi, model.Id);

        mevcut.TedarikciId = model.TedarikciId;
        mevcut.TedarikciUrunKodu = model.TedarikciUrunKodu?.Trim();
        mevcut.BirimMaliyet = model.BirimMaliyet;
        mevcut.IndirimOrani = model.IndirimOrani;
        mevcut.NetBirimMaliyet = NetMaliyetHesapla(model.BirimMaliyet, model.IndirimOrani);
        mevcut.MinimumSiparisAdedi = model.MinimumSiparisAdedi;
        mevcut.TeslimSuresiGun = model.TeslimSuresiGun;
        mevcut.VarsayilanMi = model.VarsayilanMi;
        mevcut.AktifMi = model.AktifMi;
        mevcut.Aciklama = model.Aciklama?.Trim();
        mevcut.GuncellemeTarihi = DateTime.Now;

        try
        {
            await _context.SaveChangesAsync();
            TempData["Basari"] = "Ürün tedarikçi bilgisi güncellendi.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Hata"] = "Kayıt başka bir kullanıcı tarafından güncellendi. Lütfen tekrar deneyin.";
        }

        return RedirectToAction("Details", "Urunler", new { id = model.UrunId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> DurumDegistir(int id, string rowVersion)
    {
        var model = await _context.UrunTedarikcileri.FirstOrDefaultAsync(x => x.Id == id);
        if (model is null)
            return NotFound();

        try
        {
            _context.Entry(model).Property(x => x.RowVersion).OriginalValue =
                Convert.FromBase64String(rowVersion);
            model.AktifMi = !model.AktifMi;
            model.GuncellemeTarihi = DateTime.Now;

            if (!model.AktifMi)
                model.VarsayilanMi = false;

            await _context.SaveChangesAsync();
            TempData["Basari"] = model.AktifMi
                ? "Tedarikçi bağlantısı aktifleştirildi."
                : "Tedarikçi bağlantısı pasifleştirildi.";
        }
        catch (FormatException)
        {
            TempData["Hata"] = "Kayıt sürüm bilgisi geçersiz.";
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Hata"] = "Kayıt başka bir kullanıcı tarafından güncellendi.";
        }

        return RedirectToAction("Details", "Urunler", new { id = model.UrunId });
    }

    private async Task FormVerileriniDoldur(int urunId, int? tedarikciId = null)
    {
        ViewData["Urun"] = await _context.Urunler.AsNoTracking()
            .FirstAsync(x => x.Id == urunId);

        ViewData["TedarikciId"] = new SelectList(
            await _context.Tedarikciler.AsNoTracking()
                .Where(x => x.AktifMi || x.Id == tedarikciId)
                .OrderBy(x => x.FirmaAdi)
                .ToListAsync(),
            "Id",
            "FirmaAdi",
            tedarikciId);
    }

    private void Dogrula(UrunTedarikci model)
    {
        if (model.BirimMaliyet < 0)
            ModelState.AddModelError(nameof(model.BirimMaliyet), "Birim maliyet negatif olamaz.");

        if (model.IndirimOrani is < 0 or > 100)
            ModelState.AddModelError(nameof(model.IndirimOrani), "İndirim oranı 0 ile 100 arasında olmalıdır.");

        if (model.MinimumSiparisAdedi < 1)
            ModelState.AddModelError(nameof(model.MinimumSiparisAdedi), "Minimum sipariş adedi en az 1 olmalıdır.");

        if (model.TeslimSuresiGun < 0)
            ModelState.AddModelError(nameof(model.TeslimSuresiGun), "Teslim süresi negatif olamaz.");
    }

    private async Task VarsayilaniTemizle(int urunId, bool varsayilanMi, int? haricId = null)
    {
        if (!varsayilanMi)
            return;

        var digerleri = await _context.UrunTedarikcileri
            .Where(x => x.UrunId == urunId &&
                        x.VarsayilanMi &&
                        (!haricId.HasValue || x.Id != haricId.Value))
            .ToListAsync();

        foreach (var diger in digerleri)
        {
            diger.VarsayilanMi = false;
            diger.GuncellemeTarihi = DateTime.Now;
        }
    }

    private static decimal NetMaliyetHesapla(decimal birimMaliyet, decimal indirimOrani)
    {
        return Math.Round(
            birimMaliyet * (1 - (indirimOrani / 100m)),
            2,
            MidpointRounding.AwayFromZero);
    }
}
