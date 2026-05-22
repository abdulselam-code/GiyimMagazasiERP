using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

public class FaturalarController : Controller
{
    private readonly AppDbContext _context;

    public FaturalarController(AppDbContext context)
    {
        _context = context;
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

        var viewModel = new FaturaDetayViewModel
        {
            SatisId = satis.Id,
            SatisTarihi = satis.SatisTarihi,
            OdemeTipi = satis.OdemeTipi,

            KayitliMusteriMi = satis.Musteri is not null,
            MusteriAdi = satis.Musteri?.AdSoyad ?? "Kayıtsız Müşteri",
            MusteriTelefon = satis.Musteri?.Telefon,
            MusteriEmail = satis.Musteri?.Email,

            PersonelAdi = satis.Personel.AdSoyad,
            PersonelPozisyonu = satis.Personel.Pozisyon,

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
}