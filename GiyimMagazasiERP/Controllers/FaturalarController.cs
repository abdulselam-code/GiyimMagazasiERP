using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer,Muhasebe")]
public class FaturalarController : Controller
{
    private readonly AppDbContext _context;

    public FaturalarController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? arama)
    {
        var query = _context.Satislar
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .AsQueryable();

        if (User.IsInRole("Kasiyer"))
        {
            var personelId = await GirisYapanKullanicininPersonelIdGetir();

            if (!personelId.HasValue)
            {
                query = query.Where(x => false);
                ViewData["KasiyerUyari"] = "Bu kullanıcıya bağlı personel kaydı bulunamadı.";
            }
            else
            {
                query = query.Where(x => x.PersonelId == personelId.Value);
            }
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            if (arama.StartsWith("FAT-", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(arama.Replace("FAT-", "", StringComparison.OrdinalIgnoreCase), out var faturaId))
            {
                query = query.Where(x => x.Id == faturaId);
            }
            else
            {
                query = query.Where(x =>
                    (x.Musteri != null && x.Musteri.AdSoyad.Contains(arama)) ||
                    x.OdemeTipi.Contains(arama));
            }
        }

        var faturalar = await query
            .OrderByDescending(x => x.SatisTarihi)
            .Select(x => new FaturaListeViewModel
            {
                SatisId = x.Id,
                FaturaNo = "FAT-" + x.Id.ToString("D6"),
                SatisTarihi = x.SatisTarihi,
                MusteriAdi = x.Musteri != null
                    ? x.Musteri.AdSoyad
                    : "Nihai Tüketici",
                SatisTuru = string.IsNullOrWhiteSpace(x.SatisTuru)
                    ? "Perakende"
                    : x.SatisTuru,
                OdemeTipi = x.OdemeTipi,
                ToplamTutar = x.NetTutar
            })
            .ToListAsync();

        ViewData["Arama"] = arama;

        return View(faturalar);
    }

    public async Task<IActionResult> Detay(int id)
    {
        var satis = await _context.Satislar
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .Include(x => x.SatisDetaylari)
                .ThenInclude(x => x.Urun)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (satis is null)
            return NotFound();
        if (User.IsInRole("Kasiyer"))
        {
            var personelId = await GirisYapanKullanicininPersonelIdGetir();

            if (!personelId.HasValue || satis.PersonelId != personelId.Value)
                return Forbid();
        }

        ViewBag.Magaza = await _context.MagazaBilgileri
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AktifMi);

        ViewBag.SatisTuru = string.IsNullOrWhiteSpace(satis.SatisTuru)
            ? "Perakende"
            : satis.SatisTuru;

        var viewModel = new FaturaDetayViewModel
        {
            SatisId = satis.Id,
            SatisTarihi = satis.SatisTarihi,
            OdemeTipi = satis.OdemeTipi,

            KayitliMusteriMi = satis.Musteri is not null,
            MusteriAdi = satis.Musteri?.AdSoyad ?? "Nihai Tüketici",
            MusteriTelefon = satis.Musteri?.Telefon,
            MusteriEmail = satis.Musteri?.Email,

            PersonelAdi = satis.Personel?.AdSoyad ?? "-",
            PersonelPozisyonu = satis.Personel?.Pozisyon ?? "-",

            ToplamTutar = satis.ToplamTutar,
            IndirimTutari = satis.IndirimTutari,
            NetTutar = satis.NetTutar,

            Kalemler = satis.SatisDetaylari
                .Select(x => new FaturaKalemiViewModel
                {
                    UrunAdi = x.Urun.UrunAdi,
                    Barkod = x.Urun.Barkod,
                    Beden = x.Urun.Beden,
                    Renk = x.Urun.Renk,
                    Adet = x.Adet,
                    BirimFiyat = x.BirimFiyat,
                    ToplamTutar = x.ToplamTutar
                })
                .ToList()
        };

        return View(viewModel);
    }
    private async Task<int?> GirisYapanKullanicininPersonelIdGetir()
    {
        var kullaniciIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(kullaniciIdText, out var kullaniciId))
            return null;

        return await _context.Kullanicilar
            .AsNoTracking()
            .Where(x => x.Id == kullaniciId && x.AktifMi)
            .Select(x => x.PersonelId)
            .FirstOrDefaultAsync();
    }
}
