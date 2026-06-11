using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,InsanKaynaklari,Muhasebe")]
public class PuantajController : Controller
{
    private readonly AppDbContext _context;

    public PuantajController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
        int? ay,
        int? yil,
        string? arama,
        string? departman,
        int page = 1,
        int pageSize = 10)
    {
        var bugun = DateTime.Today;
        var seciliAy = ay is >= 1 and <= 12 ? ay.Value : bugun.Month;
        var seciliYil = yil is >= 2000 and <= 2100 ? yil.Value : bugun.Year;
        var ayBaslangici = new DateTime(seciliYil, seciliAy, 1);
        var sonrakiAy = ayBaslangici.AddMonths(1);

        var personelQuery = _context.Personeller
            .AsNoTracking()
            .Where(x => x.AktifMi);

        if (!string.IsNullOrWhiteSpace(arama))
        {
            var metin = arama.Trim();
            personelQuery = personelQuery.Where(x =>
                x.AdSoyad.Contains(metin) ||
                x.Pozisyon.Contains(metin));
        }

        if (!string.IsNullOrWhiteSpace(departman))
            personelQuery = personelQuery.Where(x => x.Departman == departman);

        var personeller = await personelQuery
            .OrderBy(x => x.AdSoyad)
            .Select(x => new { x.Id, x.AdSoyad, x.Departman, x.Pozisyon })
            .ToListAsync();
        var ids = personeller.Select(x => x.Id).ToList();

        var mesailer = await _context.PersonelMesaiKayitlari
            .AsNoTracking()
            .Where(x => ids.Contains(x.PersonelId) &&
                        x.Tarih >= ayBaslangici &&
                        x.Tarih < sonrakiAy)
            .ToListAsync();

        var izinler = await _context.PersonelIzinleri
            .AsNoTracking()
            .Where(x => ids.Contains(x.PersonelId) &&
                        x.Durum == PersonelIzni.DurumOnaylandi &&
                        x.BaslangicTarihi < sonrakiAy &&
                        x.BitisTarihi >= ayBaslangici)
            .ToListAsync();

        var tumSatirlar = personeller.Select(personel =>
        {
            var personelMesaileri = mesailer.Where(x => x.PersonelId == personel.Id).ToList();
            var onayli = personelMesaileri
                .Where(x => x.Durum == PersonelMesaiKaydi.DurumOnaylandi)
                .ToList();
            var fazlaMesailer = onayli
                .Where(x => PersonelMesaiKaydi.FazlaMesaiKapsamindaMi(x.MesaiTuru))
                .ToList();
            var personelIzinleri = izinler.Where(x => x.PersonelId == personel.Id).ToList();
            var yillik = personelIzinleri
                .Where(x => x.IzinTuru == "Yıllık İzin")
                .Sum(x => AydakiIzinGunu(x, ayBaslangici, sonrakiAy));
            var diger = personelIzinleri
                .Where(x => x.IzinTuru != "Yıllık İzin")
                .Sum(x => AydakiIzinGunu(x, ayBaslangici, sonrakiAy));
            var bekleyen = personelMesaileri.Count(x =>
                x.Durum == PersonelMesaiKaydi.DurumOnayBekliyor);
            var fazlaSaat = fazlaMesailer.Sum(x => x.GosterilecekFazlaMesaiSaati);

            return new PuantajPersonelSatirViewModel
            {
                PersonelId = personel.Id,
                PersonelAdi = personel.AdSoyad,
                Departman = personel.Departman,
                Pozisyon = personel.Pozisyon,
                PlanlananSaat = onayli.Sum(x => x.PlanlananSaat),
                GerceklesenSaat = onayli.Sum(x =>
                    x.GerceklesenSaat > 0 ? x.GerceklesenSaat : x.PlanlananSaat),
                OnayliFazlaMesai = fazlaSaat,
                NormalVardiyaSayisi = onayli.Count(x => x.MesaiTuru == "Normal Vardiya"),
                FazlaMesaiKayitSayisi = fazlaMesailer.Count,
                BekleyenMesaiSayisi = bekleyen,
                YillikIzinGunu = yillik,
                DigerIzinGunu = diger,
                DurumNotu = DurumNotuGetir(
                    bekleyen,
                    fazlaSaat,
                    yillik + diger,
                    personelMesaileri.Count + personelIzinleri.Count)
            };
        }).ToList();

        var gecerliBoyutlar = new[] { 10, 25, 50, 100 };
        if (!gecerliBoyutlar.Contains(pageSize))
            pageSize = 10;
        page = Math.Max(1, page);
        var totalPages = Math.Max(1, (int)Math.Ceiling(tumSatirlar.Count / (double)pageSize));
        page = Math.Min(page, totalPages);

        var model = new PuantajIndexViewModel
        {
            Ay = seciliAy,
            Yil = seciliYil,
            Arama = arama,
            Departman = departman,
            Page = page,
            PageSize = pageSize,
            TotalCount = tumSatirlar.Count,
            TotalPages = totalPages,
            Departmanlar = await _context.Personeller.AsNoTracking()
                .Where(x => x.AktifMi)
                .Select(x => x.Departman)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(),
            Items = tumSatirlar.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
            ToplamPlanlananSaat = tumSatirlar.Sum(x => x.PlanlananSaat),
            ToplamGerceklesenSaat = tumSatirlar.Sum(x => x.GerceklesenSaat),
            ToplamOnayliFazlaMesai = tumSatirlar.Sum(x => x.OnayliFazlaMesai),
            ToplamIzinGunu = tumSatirlar.Sum(x => x.ToplamIzinGunu),
            BekleyenMesaiTalebi = tumSatirlar.Sum(x => x.BekleyenMesaiSayisi)
        };

        return View(model);
    }

    private static int AydakiIzinGunu(
        PersonelIzni izin,
        DateTime ayBaslangici,
        DateTime sonrakiAy)
    {
        var baslangic = izin.BaslangicTarihi.Date < ayBaslangici
            ? ayBaslangici
            : izin.BaslangicTarihi.Date;
        var bitis = izin.BitisTarihi.Date >= sonrakiAy
            ? sonrakiAy.AddDays(-1)
            : izin.BitisTarihi.Date;
        return bitis < baslangic ? 0 : (bitis - baslangic).Days + 1;
    }

    private static string DurumNotuGetir(
        int bekleyen,
        decimal fazlaMesai,
        int izinGunu,
        int kayitSayisi)
    {
        if (bekleyen > 0) return "Onay bekleyen mesai var";
        if (fazlaMesai > 0) return "Fazla mesai mevcut";
        if (izinGunu > 0) return "İzin kaydı var";
        return kayitSayisi == 0 ? "Kayıt bulunmuyor" : "Normal";
    }
}
