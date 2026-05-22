using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class SatisIslemleriController : Controller
{
    private readonly AppDbContext _context;

    public SatisIslemleriController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Create()
    {
        await DropdownlariDoldur();

        return View(new SatisOlusturViewModel
        {
            Adet = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SatisOlusturViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.UrunId, model.OdemeTipi);
            return View(model);
        }

        var urun = await _context.Urunler
            .FirstOrDefaultAsync(x => x.Id == model.UrunId && x.AktifMi);

        if (urun is null)
        {
            ModelState.AddModelError("", "Seçilen ürün bulunamadı veya pasif durumda.");
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.UrunId, model.OdemeTipi);
            return View(model);
        }

        if (urun.StokMiktari < model.Adet)
        {
            ModelState.AddModelError("", $"Stok yetersiz. Mevcut stok: {urun.StokMiktari}");
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.UrunId, model.OdemeTipi);
            return View(model);
        }

        var personelVarMi = await _context.Personeller
            .AnyAsync(x => x.Id == model.PersonelId && x.AktifMi);

        if (!personelVarMi)
        {
            ModelState.AddModelError("", "Seçilen personel bulunamadı veya pasif durumda.");
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.UrunId, model.OdemeTipi);
            return View(model);
        }

        Musteri? musteri = null;

        if (model.MusteriId.HasValue)
        {
            musteri = await _context.Musteriler
                .FirstOrDefaultAsync(x => x.Id == model.MusteriId.Value);

            if (musteri is null)
            {
                ModelState.AddModelError("", "Seçilen müşteri bulunamadı.");
                await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.UrunId, model.OdemeTipi);
                return View(model);
            }
        }

        var finansKullanicisi = await _context.Kullanicilar
            .Where(x => x.AktifMi)
            .OrderBy(x => x.Id)
            .FirstOrDefaultAsync();

        if (finansKullanicisi is null)
        {
            ModelState.AddModelError("", "Finans hareketi oluşturmak için aktif kullanıcı bulunamadı.");
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.UrunId, model.OdemeTipi);
            return View(model);
        }

        var toplamTutar = urun.SatisFiyati * model.Adet;
        var indirimOrani = musteri?.IndirimOrani ?? 0;
        var indirimTutari = toplamTutar * indirimOrani / 100;
        var netTutar = toplamTutar - indirimTutari;

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var satis = new Satis
            {
                MusteriId = model.MusteriId,
                PersonelId = model.PersonelId,
                SatisTarihi = DateTime.Now,
                ToplamTutar = toplamTutar,
                IndirimTutari = indirimTutari,
                NetTutar = netTutar,
                OdemeTipi = model.OdemeTipi
            };

            _context.Satislar.Add(satis);
            await _context.SaveChangesAsync();

            var satisDetayi = new SatisDetayi
            {
                SatisId = satis.Id,
                UrunId = urun.Id,
                Adet = model.Adet,
                BirimFiyat = urun.SatisFiyati,
                ToplamTutar = toplamTutar
            };

            _context.SatisDetaylari.Add(satisDetayi);

            urun.StokMiktari -= model.Adet;

            var stokHareketi = new StokHareketi
            {
                UrunId = urun.Id,
                HareketTipi = "SatisCikis",
                Miktar = model.Adet,
                Tarih = DateTime.Now,
                Aciklama = $"Satış No: {satis.Id}"
            };

            _context.StokHareketleri.Add(stokHareketi);

            var finansHareketi = new FinansHareketi
            {
                SatisId = satis.Id,
                KullaniciId = finansKullanicisi.Id,
                HareketTipi = "Gelir",
                Kategori = "Satis Geliri",
                Tutar = netTutar,
                Tarih = DateTime.Now,
                Aciklama = $"Satış No: {satis.Id} satış geliri"
            };

            _context.FinansHareketleri.Add(finansHareketi);

            if (musteri is not null)
            {
                musteri.ToplamHarcama += netTutar;

                var kazanilanPuan = Math.Max(1, (int)(netTutar / 100));
                musteri.SadakatPuani += kazanilanPuan;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(Success), new { id = satis.Id });
        }
        catch
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError("", "Satış kaydedilirken hata oluştu. İşlem tamamlanmadı.");
            await DropdownlariDoldur(model.MusteriId, model.PersonelId, model.UrunId, model.OdemeTipi);
            return View(model);
        }
    }

    public async Task<IActionResult> Success(int id)
    {
        var satis = await _context.Satislar
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .Include(x => x.SatisDetaylari)
                .ThenInclude(x => x.Urun)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (satis is null)
            return NotFound();

        return View(satis);
    }

    private async Task DropdownlariDoldur(
        int? musteriId = null,
        int? personelId = null,
        int? urunId = null,
        string? odemeTipi = null)
    {
        var musteriler = await _context.Musteriler
            .OrderBy(x => x.AdSoyad)
            .Select(x => new
            {
                x.Id,
                x.AdSoyad,
                x.IndirimOrani
            })
            .ToListAsync();

        ViewData["MusteriId"] = new SelectList(
            musteriler,
            "Id",
            "AdSoyad",
            musteriId);

        ViewData["MusteriIndirimOranlari"] = musteriler
            .ToDictionary(x => x.Id, x => x.IndirimOrani);

        ViewData["PersonelId"] = new SelectList(
            await _context.Personeller
                .Where(x => x.AktifMi)
                .OrderBy(x => x.AdSoyad)
                .ToListAsync(),
            "Id",
            "AdSoyad",
            personelId);

        var urunler = await _context.Urunler
            .Where(x => x.AktifMi)
            .OrderBy(x => x.UrunAdi)
            .Select(x => new
            {
                x.Id,
                x.SatisFiyati,
                x.StokMiktari,
                Gorunum = x.UrunAdi
                    + " | Barkod: " + x.Barkod
                    + " | Stok: " + x.StokMiktari
                    + " | Fiyat: " + x.SatisFiyati + " TL"
            })
            .ToListAsync();

        ViewData["UrunId"] = new SelectList(
            urunler,
            "Id",
            "Gorunum",
            urunId);

        ViewData["UrunFiyatlari"] = urunler
            .ToDictionary(x => x.Id, x => x.SatisFiyati);

        ViewData["UrunStoklari"] = urunler
            .ToDictionary(x => x.Id, x => x.StokMiktari);

        ViewData["OdemeTipleri"] = new SelectList(
            new[] { "Nakit", "KrediKarti", "BankaKarti", "Havale" },
            odemeTipi);
    }
}