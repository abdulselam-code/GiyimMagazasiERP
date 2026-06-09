using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    public async Task<IActionResult> Index(
       string? arama,
       DateTime? baslangicTarihi,
       DateTime? bitisTarihi,
       string? satisTuru,
       string? odemeTipi,
       int? personelId,
       int page = 1,
       int pageSize = 10)
    {
        var izinliPageSize = new[] { 10, 25, 50, 100 };

        if (!izinliPageSize.Contains(pageSize))
            pageSize = 10;

        if (page < 1)
            page = 1;

        var kasiyerMi = User.IsInRole("Kasiyer");

        var query = _context.Satislar
            .AsNoTracking()
            .Include(x => x.Musteri)
            .Include(x => x.Personel)
            .AsQueryable();

        if (kasiyerMi)
        {
            var girisYapanPersonelId = await GirisYapanKullanicininPersonelIdGetir();

            if (!girisYapanPersonelId.HasValue)
            {
                query = query.Where(x => false);
                ViewData["KasiyerUyari"] = "Bu kullanıcıya bağlı personel kaydı bulunamadı.";
            }
            else
            {
                query = query.Where(x => x.PersonelId == girisYapanPersonelId.Value);
            }

            personelId = null;
        }
        else if (personelId.HasValue)
        {
            query = query.Where(x => x.PersonelId == personelId.Value);
        }

        if (!string.IsNullOrWhiteSpace(arama))
        {
            arama = arama.Trim();

            int? arananSatisId = null;

            if (arama.StartsWith("FAT-", StringComparison.OrdinalIgnoreCase))
            {
                var faturaNo = arama.Replace("FAT-", "", StringComparison.OrdinalIgnoreCase);
                if (int.TryParse(faturaNo, out var parsedId))
                    arananSatisId = parsedId;
            }
            else if (int.TryParse(arama, out var normalId))
            {
                arananSatisId = normalId;
            }

            query = query.Where(x =>
                (arananSatisId.HasValue && x.Id == arananSatisId.Value) ||
              (x.Musteri != null && x.Musteri.AdSoyad.Contains(arama)) ||
               x.FaturaNo.Contains(arama) ||
               x.OdemeTipi.Contains(arama) ||
              (x.SatisTuru != null && x.SatisTuru.Contains(arama)));
        }

        if (baslangicTarihi.HasValue)
        {
            var baslangic = baslangicTarihi.Value.Date;
            query = query.Where(x => x.SatisTarihi >= baslangic);
        }

        if (bitisTarihi.HasValue)
        {
            var bitisExclusive = bitisTarihi.Value.Date.AddDays(1);
            query = query.Where(x => x.SatisTarihi < bitisExclusive);
        }

        if (!string.IsNullOrWhiteSpace(satisTuru) && satisTuru != "Tumu")
        {
            if (satisTuru == "Perakende")
            {
                query = query.Where(x => x.SatisTuru == null || x.SatisTuru == "" || x.SatisTuru == "Perakende");
            }
            else if (satisTuru == "Toptan")
            {
                query = query.Where(x => x.SatisTuru == "Toptan");
            }
        }

        if (!string.IsNullOrWhiteSpace(odemeTipi) && odemeTipi != "Tumu")
        {
            query = query.Where(x => x.OdemeTipi == odemeTipi);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        if (totalPages > 0 && page > totalPages)
            page = totalPages;

        var faturalar = await query
            .OrderByDescending(x => x.SatisTarihi)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FaturaListeViewModel
            {
                SatisId = x.Id,
                FaturaNo = string.IsNullOrWhiteSpace(x.FaturaNo)
                ? "FAT-" + x.Id.ToString("D6")
                : x.FaturaNo,
                SatisTarihi = x.SatisTarihi,
                MusteriAdi = x.Musteri != null ? x.Musteri.AdSoyad : "Nihai Tüketici",
                PersonelAdi = x.Personel != null ? x.Personel.AdSoyad : "-",
                SatisTuru = string.IsNullOrWhiteSpace(x.SatisTuru) ? "Perakende" : x.SatisTuru,
                OdemeTipi = x.OdemeTipi,
                ToplamTutar = x.NetTutar
            })
            .ToListAsync();

        ViewData["Arama"] = arama;
        ViewData["BaslangicTarihi"] = baslangicTarihi?.ToString("yyyy-MM-dd");
        ViewData["BitisTarihi"] = bitisTarihi?.ToString("yyyy-MM-dd");
        ViewData["SatisTuru"] = string.IsNullOrWhiteSpace(satisTuru) ? "Tumu" : satisTuru;
        ViewData["OdemeTipi"] = string.IsNullOrWhiteSpace(odemeTipi) ? "Tumu" : odemeTipi;
        ViewData["PersonelId"] = personelId;
        ViewData["KasiyerMi"] = kasiyerMi;

        ViewData["Personeller"] = await _context.Personeller
    .AsNoTracking()
    .Where(x => x.AktifMi)
    .OrderBy(x => x.AdSoyad)
    .Select(x => new SelectListItem
    {
        Value = x.Id.ToString(),
        Text = x.AdSoyad + " - " + x.Pozisyon,
        Selected = personelId.HasValue && x.Id == personelId.Value
    })
    .ToListAsync();

        ViewData["OdemeTipleri"] = await _context.Satislar
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.OdemeTipi))
            .Select(x => x.OdemeTipi)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var model = new PagedResultViewModel<FaturaListeViewModel>
        {
            Items = faturalar,
            Arama = arama,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return View(model);
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
          .Where(x => x.AktifMi)
          .OrderByDescending(x => x.Id)
          .FirstOrDefaultAsync();

        ViewBag.SatisTuru = string.IsNullOrWhiteSpace(satis.SatisTuru)
            ? "Perakende"
            : satis.SatisTuru;

        var viewModel = new FaturaDetayViewModel
        {
            SatisId = satis.Id,
            FaturaNo = string.IsNullOrWhiteSpace(satis.FaturaNo)
                   ? "FAT-" + satis.Id.ToString("D6")
                   : satis.FaturaNo,
            SatisTarihi = satis.SatisTarihi,
            FaturaTarihi = satis.FaturaTarihi == default ? satis.SatisTarihi : satis.FaturaTarihi,
            SatisTuru = string.IsNullOrWhiteSpace(satis.SatisTuru) ? "Perakende" : satis.SatisTuru,
            BelgeTuru = string.IsNullOrWhiteSpace(satis.BelgeTuru) ? "SatisBelgesi" : satis.BelgeTuru,
            FaturaDurumu = string.IsNullOrWhiteSpace(satis.FaturaDurumu) ? "Olusturuldu" : satis.FaturaDurumu,
            UUID = satis.UUID,
            OdemeTipi = satis.OdemeTipi,

            KayitliMusteriMi = satis.Musteri is not null,
            MusteriAdi = satis.Musteri?.AdSoyad ?? "Nihai Tüketici",
            MusteriTelefon = satis.Musteri?.Telefon,
            MusteriEmail = satis.Musteri?.Email,

            MusteriTipi = satis.Musteri?.MusteriTipi ?? "Bireysel",
            KurumsalUnvan = satis.Musteri?.KurumsalUnvan,
            MusteriAdres = satis.Musteri?.Adres,
            MusteriIl = satis.Musteri?.Il,
            MusteriIlce = satis.Musteri?.Ilce,
            MusteriTCKN = satis.Musteri?.TCKN,
            MusteriVKN = satis.Musteri?.VKN,
            MusteriVergiDairesi = satis.Musteri?.VergiDairesi,

            PersonelAdi = satis.Personel?.AdSoyad ?? "-",
            PersonelPozisyonu = satis.Personel?.Pozisyon ?? "-",

            ToplamTutar = satis.ToplamTutar,
            IndirimTutari = satis.IndirimTutari,
            NetTutar = satis.NetTutar,
            ToplamKdvTutari = satis.ToplamKdvTutari,
            VergiHaricToplam = satis.VergiHaricToplam,
            VergiDahilToplam = satis.VergiDahilToplam == 0
                ? satis.NetTutar
                : satis.VergiDahilToplam,

            Kalemler = satis.SatisDetaylari
    .Select(x => new FaturaKalemiViewModel
    {
        UrunAdi = string.IsNullOrWhiteSpace(x.UrunAdiSnapshot)
            ? x.Urun.UrunAdi
            : x.UrunAdiSnapshot,

        Barkod = string.IsNullOrWhiteSpace(x.BarkodSnapshot)
            ? x.Urun.Barkod
            : x.BarkodSnapshot,

        Beden = string.IsNullOrWhiteSpace(x.BedenSnapshot)
            ? x.Urun.Beden
            : x.BedenSnapshot,

        Renk = string.IsNullOrWhiteSpace(x.RenkSnapshot)
            ? x.Urun.Renk
            : x.RenkSnapshot,

        Adet = x.Adet,
        BirimFiyat = x.BirimFiyat,
        IndirimTutari = x.SatirIndirimTutari,
        KdvOrani = x.KdvOrani,
        KdvTutari = x.KdvTutari,
        VergiHaricTutar = x.VergiHaricTutar,
        VergiDahilTutar = x.VergiDahilTutar == 0
            ? x.ToplamTutar
            : x.VergiDahilTutar,
        ToplamTutar = x.VergiDahilTutar == 0
            ? x.ToplamTutar
            : x.VergiDahilTutar
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
