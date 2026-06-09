using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici")]
public class MagazaBilgileriController : Controller
{
    private readonly AppDbContext _context;

    public MagazaBilgileriController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var kayitlar = await _context.MagazaBilgileri
            .AsNoTracking()
            .OrderByDescending(x => x.AktifMi)
            .ThenBy(x => x.MagazaAdi)
            .ToListAsync();

        return View(kayitlar);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue)
            return NotFound();

        var model = await _context.MagazaBilgileri
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id.Value);

        return model is null ? NotFound() : View(model);
    }

    public IActionResult Create()
    {
        return View(new MagazaBilgileri
        {
            AktifMi = true,
            KurulusTarihi = DateTime.Today
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MagazaAdi,TicariUnvan,Adres,Il,Ilce,Telefon,Email,WebAdresi,VergiDairesi,VergiNo,MersisNo,TicaretSicilNo,KurulusTarihi,AktifMi")]
        MagazaBilgileri model)
    {
        if (!ModelState.IsValid)
            return View(model);

        if (model.AktifMi)
        {
            var aktifKayitlar = await _context.MagazaBilgileri
                .Where(x => x.AktifMi)
                .ToListAsync();

            aktifKayitlar.ForEach(x => x.AktifMi = false);
        }

        _context.MagazaBilgileri.Add(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (!id.HasValue)
            return NotFound();

        var model = await _context.MagazaBilgileri.FindAsync(id.Value);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,MagazaAdi,TicariUnvan,Adres,Il,Ilce,Telefon,Email,WebAdresi,VergiDairesi,VergiNo,MersisNo,TicaretSicilNo,KurulusTarihi,AktifMi")]
        MagazaBilgileri model)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        if (model.AktifMi)
        {
            var digerAktifKayitlar = await _context.MagazaBilgileri
                .Where(x => x.AktifMi && x.Id != id)
                .ToListAsync();

            digerAktifKayitlar.ForEach(x => x.AktifMi = false);
        }

        _context.Update(model);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (!id.HasValue)
            return NotFound();

        var model = await _context.MagazaBilgileri
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id.Value);

        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var model = await _context.MagazaBilgileri.FindAsync(id);

        if (model is not null)
        {
            model.AktifMi = false;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}