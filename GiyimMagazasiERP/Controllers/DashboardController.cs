using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Kasiyer,Depo,Muhasebe,InsanKaynaklari,Personel")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var bugun = DateTime.Today;
        var yarin = bugun.AddDays(1);
        var ayBaslangici = new DateTime(bugun.Year, bugun.Month, 1);
        var sonrakiAyBaslangici = ayBaslangici.AddMonths(1);
        var ayBitisi = sonrakiAyBaslangici.AddDays(-1);
        var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "Personel";
        var kullaniciIdMetni =
            User.FindFirstValue(ClaimTypes.NameIdentifier);
        int? kullaniciId = int.TryParse(kullaniciIdMetni, out var id)
            ? id
            : null;
        int? personelId = null;

        if (kullaniciId.HasValue)
        {
            personelId = await _context.Kullanicilar
                .AsNoTracking()
                .Where(x => x.Id == kullaniciId.Value)
                .Select(x => x.PersonelId)
                .FirstOrDefaultAsync();
        }

        var toplamGelir = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gelir")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var toplamGider = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        var model = new DashboardViewModel
        {
            KullaniciAdi = User.Identity?.Name ?? "Kullanıcı",
            Rol = rol,

            BugununTarihi = bugun,
            BuAyBaslangicTarihi = ayBaslangici,
            BuAyBitisTarihi = ayBitisi,

            IlkSatisTarihi = await _context.Satislar
    .AsNoTracking()
    .OrderBy(x => x.SatisTarihi)
    .Select(x => (DateTime?)x.SatisTarihi)
    .FirstOrDefaultAsync(),

            SonSatisTarihi = await _context.Satislar
    .AsNoTracking()
    .OrderByDescending(x => x.SatisTarihi)
    .Select(x => (DateTime?)x.SatisTarihi)
    .FirstOrDefaultAsync(),

            SonStokHareketiTarihi = await _context.StokHareketleri
    .AsNoTracking()
    .OrderByDescending(x => x.Tarih)
    .Select(x => (DateTime?)x.Tarih)
    .FirstOrDefaultAsync(),

            SonFinansHareketiTarihi = await _context.FinansHareketleri
    .AsNoTracking()
    .OrderByDescending(x => x.Tarih)
    .Select(x => (DateTime?)x.Tarih)
    .FirstOrDefaultAsync(),

            AylikSatisSayisi = await _context.Satislar
    .CountAsync(x => x.SatisTarihi >= ayBaslangici && x.SatisTarihi < sonrakiAyBaslangici),

            AylikSatisGeliri = await _context.Satislar
    .Where(x => x.SatisTarihi >= ayBaslangici && x.SatisTarihi < sonrakiAyBaslangici)
    .SumAsync(x => (decimal?)x.NetTutar) ?? 0,

            BugunkuGider = await _context.FinansHareketleri
    .Where(x => x.HareketTipi == "Gider" && x.Tarih >= bugun && x.Tarih < yarin)
    .SumAsync(x => (decimal?)x.Tutar) ?? 0,

            NetKarZarar = toplamGelir - toplamGider,

            EnYuksekMaas = await _context.Personeller
    .Where(x => x.AktifMi)
    .MaxAsync(x => (decimal?)x.Maas) ?? 0,

            EnDusukMaas = await _context.Personeller
    .Where(x => x.AktifMi)
    .MinAsync(x => (decimal?)x.Maas) ?? 0,

            ToplamUrunCesidi = await _context.Urunler.CountAsync(),
            ToplamStokAdedi = await _context.Urunler.SumAsync(x => (int?)x.StokMiktari) ?? 0,
            ToplamMusteri = await _context.Musteriler.CountAsync(),
            ToplamPersonel = await _context.Personeller.CountAsync(),
            AktifPersonel = await _context.Personeller.CountAsync(x => x.AktifMi),
            ToplamSatis = await _context.Satislar.CountAsync(),
            ToplamTedarikci = await _context.Tedarikciler.CountAsync(),
            KritikStokSayisi = await _context.Urunler.CountAsync(x => x.AktifMi && x.StokMiktari <= x.MinimumStok),

            ToplamGelir = toplamGelir,
            ToplamGider = toplamGider,
            NetKazanc = toplamGelir - toplamGider,

            BugunkuSatisSayisi = await _context.Satislar.CountAsync(x => x.SatisTarihi >= bugun && x.SatisTarihi < yarin),
            BugunkuSatisGeliri = await _context.Satislar
                .Where(x => x.SatisTarihi >= bugun && x.SatisTarihi < yarin)
                .SumAsync(x => (decimal?)x.NetTutar) ?? 0,

            BugunkuGelir = await _context.FinansHareketleri
                .Where(x => x.HareketTipi == "Gelir" && x.Tarih >= bugun && x.Tarih < yarin)
                .SumAsync(x => (decimal?)x.Tutar) ?? 0,

            AylikGelir = await _context.FinansHareketleri
                .Where(x => x.HareketTipi == "Gelir" && x.Tarih >= ayBaslangici)
                .SumAsync(x => (decimal?)x.Tutar) ?? 0,

            AylikGider = await _context.FinansHareketleri
                .Where(x => x.HareketTipi == "Gider" && x.Tarih >= ayBaslangici)
                .SumAsync(x => (decimal?)x.Tutar) ?? 0,

            OrtalamaMaas = await _context.Personeller
                .Where(x => x.AktifMi)
                .AverageAsync(x => (decimal?)x.Maas) ?? 0,

            EnYuksekGider = await _context.FinansHareketleri
                .Where(x => x.HareketTipi == "Gider")
                .MaxAsync(x => (decimal?)x.Tutar) ?? 0
               
        };

        model.BugunkuNet = model.BugunkuGelir - model.BugunkuGider;
        model.SatisIadeleriToplami = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x =>
                x.HareketTipi == "Gider" &&
                x.Kategori == "Satış İadesi")
            .SumAsync(x => (decimal?)x.Tutar) ?? 0;

        model.BugunkuIadeTutari = await _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Where(x =>
                x.Durum == IadeDegisimTalebi.DurumTamamlandi &&
                x.TamamlanmaTarihi >= bugun &&
                x.TamamlanmaTarihi < yarin)
            .SumAsync(x => (decimal?)x.ToplamIadeTutari) ?? 0;

        model.BekleyenToptanTalepSayisi =
            await _context.ToptanSatisTalepleri
                .AsNoTracking()
                .CountAsync(x =>
                    x.Durum ==
                        ToptanSatisTalebi.DurumYoneticiOnayiBekliyor ||
                    x.Durum ==
                        ToptanSatisTalebi.DurumMuhasebeOnayiBekliyor);

        model.BekleyenIadeTalepSayisi =
            await _context.IadeDegisimTalepleri
                .AsNoTracking()
                .CountAsync(x =>
                    x.Durum ==
                        IadeDegisimTalebi.DurumYoneticiOnayiBekliyor ||
                    x.Durum ==
                        IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor);

        model.MuhasebeOnayiBekleyenToptanSayisi =
            await _context.ToptanSatisTalepleri
                .AsNoTracking()
                .CountAsync(x =>
                    x.Durum ==
                    ToptanSatisTalebi.DurumMuhasebeOnayiBekliyor);

        model.MuhasebeOnayiBekleyenIadeSayisi =
            await _context.IadeDegisimTalepleri
                .AsNoTracking()
                .CountAsync(x =>
                    x.Durum ==
                    IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor);

        model.TamamlananIadeBelgesiSayisi =
            await _context.IadeDegisimTalepleri
                .AsNoTracking()
                .CountAsync(x =>
                    x.Durum == IadeDegisimTalebi.DurumTamamlandi &&
                    x.IadeBelgeNo != null &&
                    x.IadeBelgeNo != "");

        model.BugunkuStokGirisAdedi = await _context.StokHareketleri
            .AsNoTracking()
            .Where(x =>
                x.Tarih >= bugun &&
                x.Tarih < yarin &&
                x.HareketTipi == "Giris")
            .SumAsync(x => (int?)x.Miktar) ?? 0;

        model.BugunkuStokCikisAdedi = await _context.StokHareketleri
            .AsNoTracking()
            .Where(x =>
                x.Tarih >= bugun &&
                x.Tarih < yarin &&
                (x.HareketTipi == "Cikis" ||
                 x.HareketTipi == "SatisCikis" ||
                 x.HareketTipi == "FireCikis"))
            .SumAsync(x => (int?)x.Miktar) ?? 0;

        model.BugunkuIadeGirisAdedi = await _context.StokHareketleri
            .AsNoTracking()
            .Where(x =>
                x.Tarih >= bugun &&
                x.Tarih < yarin &&
                x.HareketTipi == "IadeGiris")
            .SumAsync(x => (int?)x.Miktar) ?? 0;

        model.HasarliIncelemeIadeUrunSayisi =
            await _context.IadeDegisimTalepDetaylari
                .AsNoTracking()
                .CountAsync(x =>
                    x.IadeDegisimTalebi.Durum ==
                        IadeDegisimTalebi.DurumTamamlandi &&
                    (x.UrunDurumu ==
                        IadeDegisimTalepDetayi.UrunDurumuHasarli ||
                     x.UrunDurumu ==
                        IadeDegisimTalepDetayi
                            .UrunDurumuIncelemeGerekli));

        if (kullaniciId.HasValue)
        {
            model.BenimBekleyenToptanTalepSayisi =
                await _context.ToptanSatisTalepleri
                    .AsNoTracking()
                    .CountAsync(x =>
                        (x.TalepEdenKullaniciId == kullaniciId.Value ||
                         (personelId.HasValue &&
                          x.TalepEdenPersonelId == personelId.Value)) &&
                        (x.Durum ==
                            ToptanSatisTalebi
                                .DurumYoneticiOnayiBekliyor ||
                         x.Durum ==
                            ToptanSatisTalebi
                                .DurumMuhasebeOnayiBekliyor));

            model.BenimBekleyenIadeTalepSayisi =
                await _context.IadeDegisimTalepleri
                    .AsNoTracking()
                    .CountAsync(x =>
                        (x.TalepEdenKullaniciId == kullaniciId.Value ||
                         (personelId.HasValue &&
                          x.TalepEdenPersonelId == personelId.Value)) &&
                        (x.Durum ==
                            IadeDegisimTalebi
                                .DurumYoneticiOnayiBekliyor ||
                         x.Durum ==
                            IadeDegisimTalebi
                                .DurumMuhasebeOnayiBekliyor));
        }

        model.HizliIslemler = HizliIslemleriGetir();

        model.IsletmeKurulusTarihi = await _context.MagazaBilgileri
    .AsNoTracking()
    .Where(x => x.AktifMi)
    .Select(x => x.KurulusTarihi)
    .FirstOrDefaultAsync();

        model.EnCokSatanUrunAdi = await _context.SatisDetaylari
            .AsNoTracking()
            .GroupBy(x => x.Urun.UrunAdi)
            .Select(g => new
            {
                UrunAdi = g.Key,
                Adet = g.Sum(x => x.Adet)
            })
            .OrderByDescending(x => x.Adet)
            .Select(x => x.UrunAdi)
            .FirstOrDefaultAsync() ?? "Henüz satış yok";

        model.EnCokHarcamaYapanMusteriAdi = await _context.Satislar
            .AsNoTracking()
            .Where(x => x.Musteri != null)
            .GroupBy(x => x.Musteri!.AdSoyad)
            .Select(g => new
            {
                MusteriAdi = g.Key,
                Harcama = g.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.Harcama)
            .Select(x => x.MusteriAdi)
            .FirstOrDefaultAsync() ?? "Henüz müşteri yok";

        model.OdemeTipineGoreGelirler = await _context.Satislar
            .AsNoTracking()
            .GroupBy(x => x.OdemeTipi)
            .Select(g => new DashboardOdemeTipiGelirViewModel
            {
                OdemeTipi = g.Key,
                SatisSayisi = g.Count(),
                ToplamGelir = g.Sum(x => x.NetTutar)
            })
            .OrderByDescending(x => x.ToplamGelir)
            .ToListAsync();

        var satisSorgusu = _context.Satislar
    .AsNoTracking()
    .Include(x => x.Musteri)
    .AsQueryable();

        if (User.IsInRole("Kasiyer") || User.IsInRole("Personel"))
        {
            if (personelId.HasValue)
            {
                satisSorgusu = satisSorgusu
                    .Where(x => x.PersonelId == personelId.Value);

                model.BugunkuSatisSayisi = await satisSorgusu
                        .CountAsync(x => x.SatisTarihi >= bugun && x.SatisTarihi < yarin);

                    model.BugunkuSatisGeliri = await satisSorgusu
                        .Where(x => x.SatisTarihi >= bugun && x.SatisTarihi < yarin)
                        .SumAsync(x => (decimal?)x.NetTutar) ?? 0;

                    model.AylikSatisSayisi = await satisSorgusu
                        .CountAsync(x => x.SatisTarihi >= ayBaslangici && x.SatisTarihi < sonrakiAyBaslangici);

                model.AylikSatisGeliri = await satisSorgusu
                        .Where(x => x.SatisTarihi >= ayBaslangici && x.SatisTarihi < sonrakiAyBaslangici)
                        .SumAsync(x => (decimal?)x.NetTutar) ?? 0;
            }
            else
            {
                model.KasiyerPersonelEslesmesiVarMi = false;
                model.UyariMesaji = "Bu kullanıcıya bağlı personel kaydı bulunamadı.";
                satisSorgusu = satisSorgusu.Where(x => false);
            }
        }

        model.SonSatislar = await satisSorgusu
    .OrderByDescending(x => x.SatisTarihi)
    .Take(8)
    .Select(x => new DashboardSonSatisViewModel
    {
        SatisId = x.Id,
        SatisTarihi = x.SatisTarihi,
        MusteriAdi = x.Musteri != null ? x.Musteri.AdSoyad : "Nihai Tüketici",
        NetTutar = x.NetTutar,
        OdemeTipi = x.OdemeTipi
    })
    .ToListAsync();

        model.KritikStokUrunleri = await _context.Urunler
            .AsNoTracking()
            .Where(x => x.AktifMi && x.StokMiktari <= x.MinimumStok)
            .OrderBy(x => x.StokMiktari)
            .Take(8)
            .Select(x => new DashboardKritikStokViewModel
            {
                UrunAdi = x.UrunAdi,
                StokMiktari = x.StokMiktari,
                MinimumStok = x.MinimumStok
            })
            .ToListAsync();

        model.SonStokHareketleri = await _context.StokHareketleri
            .AsNoTracking()
            .Include(x => x.Urun)
            .OrderByDescending(x => x.Tarih)
            .Take(8)
            .Select(x => new DashboardStokHareketiViewModel
            {
                Tarih = x.Tarih,
                UrunAdi = x.Urun.UrunAdi,
                HareketTipi = x.HareketTipi,
                Miktar = x.Miktar
            })
            .ToListAsync();

        model.SonFinansHareketleri = await _context.FinansHareketleri
            .AsNoTracking()
            .OrderByDescending(x => x.Tarih)
            .Take(8)
            .Select(x => new DashboardFinansHareketiViewModel
            {
                Tarih = x.Tarih,
                HareketTipi = x.HareketTipi,
                Kategori = x.Kategori,
                Tutar = x.Tutar
            })
            .ToListAsync();

        model.EnYuksekGiderler = await _context.FinansHareketleri
            .AsNoTracking()
            .Where(x => x.HareketTipi == "Gider")
            .OrderByDescending(x => x.Tutar)
            .Take(8)
            .Select(x => new DashboardGiderViewModel
            {
                Tarih = x.Tarih,
                Kategori = x.Kategori,
                Aciklama = x.Aciklama,
                Tutar = x.Tutar
            })
            .ToListAsync();

        model.PersonelOzeti = await _context.Personeller
            .AsNoTracking()
            .Where(x => x.AktifMi)
            .OrderByDescending(x => x.IseBaslamaTarihi)
            .Take(8)
            .Select(x => new DashboardPersonelOzetViewModel
            {
                AdSoyad = x.AdSoyad,
                Pozisyon = x.Pozisyon,
                Departman = x.Departman,
                Maas = x.Maas
            })
            .ToListAsync();

        model.SonIadeBelgeleri = await _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Where(x =>
                x.Durum == IadeDegisimTalebi.DurumTamamlandi &&
                x.IadeBelgeNo != null &&
                x.IadeBelgeNo != "")
            .OrderByDescending(x => x.TamamlanmaTarihi)
            .Take(5)
            .Select(x => new DashboardIadeBelgesiViewModel
            {
                Id = x.Id,
                BelgeNo = x.IadeBelgeNo!,
                TalepNo = x.TalepNo,
                Tarih = x.TamamlanmaTarihi ?? x.TalepTarihi,
                MusteriAdi = x.Musteri != null
                    ? x.Musteri.AdSoyad
                    : "Nihai Tüketici",
                Tutar = x.ToplamIadeTutari
            })
            .ToListAsync();

        var bekleyenToptan = await _context.ToptanSatisTalepleri
            .AsNoTracking()
            .Where(x =>
                x.Durum ==
                    ToptanSatisTalebi.DurumYoneticiOnayiBekliyor ||
                x.Durum ==
                    ToptanSatisTalebi.DurumMuhasebeOnayiBekliyor)
            .OrderByDescending(x => x.TalepTarihi)
            .Take(5)
            .Select(x => new DashboardTalepOzetViewModel
            {
                Id = x.Id,
                Modul = "Toptan",
                TalepNo = x.TalepNo,
                Durum = x.Durum,
                Tarih = x.TalepTarihi,
                Tutar = x.NetTutar
            })
            .ToListAsync();

        var bekleyenIade = await _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Where(x =>
                x.Durum ==
                    IadeDegisimTalebi.DurumYoneticiOnayiBekliyor ||
                x.Durum ==
                    IadeDegisimTalebi.DurumMuhasebeOnayiBekliyor)
            .OrderByDescending(x => x.TalepTarihi)
            .Take(5)
            .Select(x => new DashboardTalepOzetViewModel
            {
                Id = x.Id,
                Modul = "İade",
                TalepNo = x.TalepNo,
                Durum = x.Durum,
                Tarih = x.TalepTarihi,
                Tutar = x.ToplamIadeTutari
            })
            .ToListAsync();

        model.BekleyenOnaylar = bekleyenToptan
            .Concat(bekleyenIade)
            .OrderByDescending(x => x.Tarih)
            .Take(6)
            .ToList();

        if (kullaniciId.HasValue)
        {
            model.SonIadeTalepleri =
                await _context.IadeDegisimTalepleri
                    .AsNoTracking()
                    .Where(x =>
                        x.TalepEdenKullaniciId == kullaniciId.Value ||
                        (personelId.HasValue &&
                         x.TalepEdenPersonelId == personelId.Value))
                    .OrderByDescending(x => x.TalepTarihi)
                    .Take(5)
                    .Select(x => new DashboardTalepOzetViewModel
                    {
                        Id = x.Id,
                        Modul = "İade",
                        TalepNo = x.TalepNo,
                        Durum = x.Durum,
                        Tarih = x.TalepTarihi,
                        Tutar = x.ToplamIadeTutari
                    })
                    .ToListAsync();

            model.SonToptanTalepleri =
                await _context.ToptanSatisTalepleri
                    .AsNoTracking()
                    .Where(x =>
                        x.TalepEdenKullaniciId == kullaniciId.Value ||
                        (personelId.HasValue &&
                         x.TalepEdenPersonelId == personelId.Value))
                    .OrderByDescending(x => x.TalepTarihi)
                    .Take(5)
                    .Select(x => new DashboardTalepOzetViewModel
                    {
                        Id = x.Id,
                        Modul = "Toptan",
                        TalepNo = x.TalepNo,
                        Durum = x.Durum,
                        Tarih = x.TalepTarihi,
                        Tutar = x.NetTutar
                    })
                    .ToListAsync();
        }

        model.SorunluIadeUrunleri =
            await _context.IadeDegisimTalepDetaylari
                .AsNoTracking()
                .Where(x =>
                    x.IadeDegisimTalebi.Durum ==
                        IadeDegisimTalebi.DurumTamamlandi &&
                    (x.UrunDurumu ==
                        IadeDegisimTalepDetayi.UrunDurumuHasarli ||
                     x.UrunDurumu ==
                        IadeDegisimTalepDetayi
                            .UrunDurumuIncelemeGerekli))
                .OrderByDescending(x =>
                    x.IadeDegisimTalebi.TamamlanmaTarihi)
                .Take(6)
                .Select(x => new DashboardIadeUrunViewModel
                {
                    TalepId = x.IadeDegisimTalebiId,
                    TalepNo = x.IadeDegisimTalebi.TalepNo,
                    UrunAdi = x.UrunAdiSnapshot,
                    UrunDurumu = x.UrunDurumu,
                    Adet = x.IadeAdedi
                })
                .ToListAsync();

        await GrafikVerileriniDoldur(model, satisSorgusu);

        return View(model);
    }

    private async Task GrafikVerileriniDoldur(
     DashboardViewModel model,
     IQueryable<GiyimMagazasiERP.Models.Satis> satisSorgusu)
    {
        var baslangic = DateTime.Today.AddDays(-6);
        var bitis = DateTime.Today.AddDays(1);

        var gunlukSatisHamVeri = await satisSorgusu
      .Where(x => x.SatisTarihi >= baslangic && x.SatisTarihi < bitis)
      .GroupBy(x => new
      {
          Yil = x.SatisTarihi.Year,
          Ay = x.SatisTarihi.Month,
          Gun = x.SatisTarihi.Day
      })
      .Select(g => new
      {
          g.Key.Yil,
          g.Key.Ay,
          g.Key.Gun,
          NetTutar = g.Sum(x => x.NetTutar)
      })
      .OrderBy(x => x.Yil)
      .ThenBy(x => x.Ay)
      .ThenBy(x => x.Gun)
      .ToListAsync();

        var gunlukSatislar = gunlukSatisHamVeri
            .Select(x => new
            {
                Gun = new DateTime(x.Yil, x.Ay, x.Gun),
                x.NetTutar
            })
            .ToList();

        model.GunlukSatisLabels = gunlukSatislar
            .Select(x => x.Gun.ToString("dd/MM"))
            .ToList();

        model.GunlukSatisValues = gunlukSatislar
            .Select(x => x.NetTutar)
            .ToList();

        model.GelirGiderLabels = new() { "Gelir", "Gider" };
        model.GelirGiderValues = new() { model.ToplamGelir, model.ToplamGider };

        var kategoriSatis = await _context.SatisDetaylari
            .AsNoTracking()
            .Include(x => x.Urun)
                .ThenInclude(x => x.Kategori)
            .GroupBy(x => x.Urun.Kategori.KategoriAdi)
            .Select(g => new
            {
                Kategori = g.Key,
                Tutar = g.Sum(x => x.ToplamTutar)
            })
            .OrderByDescending(x => x.Tutar)
            .Take(8)
            .ToListAsync();

        model.KategoriSatisLabels = kategoriSatis.Select(x => x.Kategori).ToList();
        model.KategoriSatisValues = kategoriSatis.Select(x => x.Tutar).ToList();

        var enCokSatilan = await _context.SatisDetaylari
            .AsNoTracking()
            .Include(x => x.Urun)
            .GroupBy(x => x.Urun.UrunAdi)
            .Select(g => new
            {
                Urun = g.Key,
                Adet = g.Sum(x => x.Adet)
            })
            .OrderByDescending(x => x.Adet)
            .Take(8)
            .ToListAsync();

        model.EnCokSatilanUrunLabels = enCokSatilan.Select(x => x.Urun).ToList();
        model.EnCokSatilanUrunValues = enCokSatilan.Select(x => x.Adet).ToList();

        model.KritikStokLabels = model.KritikStokUrunleri.Select(x => x.UrunAdi).ToList();
        model.KritikStokValues = model.KritikStokUrunleri.Select(x => x.StokMiktari).ToList();

        var aylikFinans = await _context.FinansHareketleri
            .AsNoTracking()
            .GroupBy(x => new { x.Tarih.Year, x.Tarih.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Gelir = g.Where(x => x.HareketTipi == "Gelir").Sum(x => (decimal?)x.Tutar) ?? 0,
                Gider = g.Where(x => x.HareketTipi == "Gider").Sum(x => (decimal?)x.Tutar) ?? 0
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Take(6)
            .ToListAsync();

        aylikFinans = aylikFinans
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();

        model.AylikGelirGiderLabels = aylikFinans.Select(x => $"{x.Month:00}/{x.Year}").ToList();
        model.AylikGelirValues = aylikFinans.Select(x => x.Gelir).ToList();
        model.AylikGiderValues = aylikFinans.Select(x => x.Gider).ToList();

        var stokBaslangic = DateTime.Today.AddDays(-6);
        var stokBitis = DateTime.Today.AddDays(1);
        var stokHam = await _context.StokHareketleri
            .AsNoTracking()
            .Where(x => x.Tarih >= stokBaslangic && x.Tarih < stokBitis)
            .GroupBy(x => new
            {
                x.Tarih.Year,
                x.Tarih.Month,
                x.Tarih.Day
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                g.Key.Day,
                Giris = g.Where(x => x.HareketTipi == "Giris")
                    .Sum(x => (int?)x.Miktar) ?? 0,
                Cikis = g.Where(x =>
                        x.HareketTipi == "Cikis" ||
                        x.HareketTipi == "SatisCikis" ||
                        x.HareketTipi == "FireCikis")
                    .Sum(x => (int?)x.Miktar) ?? 0,
                Iade = g.Where(x => x.HareketTipi == "IadeGiris")
                    .Sum(x => (int?)x.Miktar) ?? 0
            })
            .ToListAsync();

        for (var gun = stokBaslangic; gun < stokBitis; gun = gun.AddDays(1))
        {
            var satir = stokHam.FirstOrDefault(x =>
                x.Year == gun.Year &&
                x.Month == gun.Month &&
                x.Day == gun.Day);

            model.StokHareketLabels.Add(gun.ToString("dd/MM"));
            model.StokGirisValues.Add(satir?.Giris ?? 0);
            model.StokCikisValues.Add(satir?.Cikis ?? 0);
            model.IadeGirisValues.Add(satir?.Iade ?? 0);
        }

        var iadeTrend = await _context.IadeDegisimTalepleri
            .AsNoTracking()
            .Where(x =>
                x.Durum == IadeDegisimTalebi.DurumTamamlandi &&
                x.TamamlanmaTarihi.HasValue)
            .GroupBy(x => new
            {
                x.TamamlanmaTarihi!.Value.Year,
                x.TamamlanmaTarihi.Value.Month
            })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Tutar = g.Sum(x => x.ToplamIadeTutari)
            })
            .OrderByDescending(x => x.Year)
            .ThenByDescending(x => x.Month)
            .Take(6)
            .ToListAsync();

        iadeTrend = iadeTrend
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();

        model.IadeTrendLabels = iadeTrend
            .Select(x => $"{x.Month:00}/{x.Year}")
            .ToList();
        model.IadeTrendValues = iadeTrend
            .Select(x => x.Tutar)
            .ToList();
    }

    private List<DashboardQuickActionViewModel> HizliIslemleriGetir()
    {
        if (User.IsInRole("Admin"))
        {
            return new()
            {
                Link("Satış Yap", "SatisIslemleri", "Create", "success"),
                Link("Ürünleri Yönet", "Urunler"),
                Link("Müşteriler", "Musteriler"),
                Link("Mağaza Bilgileri", "MagazaBilgileri", "Index", "secondary"),
                Link("Finans Hareketleri", "FinansHareketleri", "Index", "warning"),
                Link("Faturalar", "Faturalar", "Index", "info"),
                Link("İade Talepleri", "IadeDegisimTalepleri", "Index", "warning"),
                Link("İade Belgeleri", "IadeDegisimTalepleri", "IadeBelgeleri", "secondary"),
                Link("Toptan Talepler", "ToptanSatisTalepleri", "Index", "primary"),
                Link("Personel İzinleri", "PersonelIzinleri", "Index", "secondary"),
                Link("Raporlar", "Raporlar", "Index", "info"),
                Link("SQL Panel", "SqlYonetici", "Index", "dark"),
                Link("DB Panel", "VeritabaniYonetici", "Index", "dark")
            };
        }

        if (User.IsInRole("Yonetici"))
        {
            return new()
            {
                Link("Satış Yap", "SatisIslemleri", "Create", "success"),
                Link("Ürünleri Yönet", "Urunler"),
                Link("Müşteriler", "Musteriler"),
                Link("Mağaza Bilgileri", "MagazaBilgileri", "Index", "secondary"),
                Link("Personeller", "Personeller"),
                Link("Raporlar", "Raporlar", "Index", "info"),
                Link("Faturalar", "Faturalar", "Index", "info"),
                Link("İade Talepleri", "IadeDegisimTalepleri", "Index", "warning"),
                Link("Toptan Talepler", "ToptanSatisTalepleri", "Index", "primary"),
                Link("Personel İzinleri", "PersonelIzinleri", "Index", "secondary")
            };
        }

        if (User.IsInRole("Kasiyer"))
        {
            return new()
            {
                Link("Satış Yap", "SatisIslemleri", "Create", "success"),
                Link("Satışlarım", "Satislar"),
                Link("Kendi Faturalarım", "Faturalar"),
                Link("Benim İade Taleplerim", "IadeDegisimTalepleri", "BenimTaleplerim", "warning"),
                Link("Benim Toptan Taleplerim", "ToptanSatisTalepleri", "BenimTaleplerim", "primary"),
                Link("Toptan Talep Oluştur", "ToptanSatisTalepleri", "Create", "secondary"),
                Link("Benim İzinlerim", "PersonelIzinleri", "BenimIzinlerim", "info"),
                Link("İzin Talebi Oluştur", "PersonelIzinleri", "Create", "secondary")
            };
        }

        if (User.IsInRole("Personel"))
        {
            return new()
            {
                Link("Benim İade Taleplerim", "IadeDegisimTalepleri", "BenimTaleplerim", "warning"),
                Link("Benim Toptan Taleplerim", "ToptanSatisTalepleri", "BenimTaleplerim", "primary"),
                Link("Toptan Talep Oluştur", "ToptanSatisTalepleri", "Create", "secondary"),
                Link("Benim İzinlerim", "PersonelIzinleri", "BenimIzinlerim", "info"),
                Link("İzin Talebi Oluştur", "PersonelIzinleri", "Create", "secondary")
            };
        }

        if (User.IsInRole("Depo"))
        {
            return new()
            {
                Link("Ürünler", "Urunler"),
                Link("Kategoriler", "Kategoriler"),
                Link("Tedarikçiler", "Tedarikciler"),
                Link("Stok Hareketleri", "StokHareketleri", "Index", "warning"),
                Link("İade Belgeleri", "IadeDegisimTalepleri", "IadeBelgeleri", "secondary"),
                Link("Benim İzinlerim", "PersonelIzinleri", "BenimIzinlerim", "info"),
                Link("İzin Talebi Oluştur", "PersonelIzinleri", "Create", "secondary")
            };
        }

        if (User.IsInRole("Muhasebe"))
        {
            return new()
            {
                Link("Finans Hareketleri", "FinansHareketleri", "Index", "warning"),
                Link("Satışlar", "Satislar"),
                Link("Faturalar", "Faturalar", "Index", "info"),
                Link("Raporlar", "Raporlar", "Index", "info"),
                Link("İade Talepleri", "IadeDegisimTalepleri", "Index", "warning"),
                Link("İade Belgeleri", "IadeDegisimTalepleri", "IadeBelgeleri", "secondary"),
                Link("Toptan Talepler", "ToptanSatisTalepleri", "Index", "primary"),
                Link("Benim İzinlerim", "PersonelIzinleri", "BenimIzinlerim", "info"),
                Link("İzin Talebi Oluştur", "PersonelIzinleri", "Create", "secondary")
            };
        }

        if (User.IsInRole("InsanKaynaklari"))
        {
            return new()
            {
                Link("Personeller", "Personeller"),
                Link("Personel İzinleri", "PersonelIzinleri", "Index", "primary"),
                Link("İzin Talebi Oluştur", "PersonelIzinleri", "Create", "secondary")
            };
        }

        return new();
    }

    private static DashboardQuickActionViewModel Link(
        string baslik,
        string controller,
        string action = "Index",
        string stil = "primary")
    {
        return new DashboardQuickActionViewModel
        {
            Baslik = baslik,
            Controller = controller,
            Action = action,
            Stil = stil
        };
    }
}
