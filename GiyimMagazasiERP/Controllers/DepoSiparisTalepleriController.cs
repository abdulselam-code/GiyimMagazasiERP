using System.Security.Claims;
using GiyimMagazasiERP.Data;
using GiyimMagazasiERP.Models;
using GiyimMagazasiERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Controllers;

[Authorize(Roles = "Admin,Yonetici,Depo,Muhasebe")]
public class DepoSiparisTalepleriController : Controller
{
    private static readonly string[] Durumlar =
    {
        DepoSiparisTalebi.DurumOnayBekliyor,
        DepoSiparisTalebi.DurumOnaylandi,
        DepoSiparisTalebi.DurumReddedildi,
        DepoSiparisTalebi.DurumIptalEdildi,
        DepoSiparisTalebi.DurumTeslimAlindi
    };

    private readonly AppDbContext _context;
    public DepoSiparisTalepleriController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index(
        string? arama, string durum = "Tumu", string oncelik = "Tumu",
        int? urunId = null, DateTime? baslangicTarihi = null,
        DateTime? bitisTarihi = null, int page = 1, int pageSize = 10)
    {
        var kullaniciId = KullaniciId();
        if (!kullaniciId.HasValue) return Forbid();

        var query = TalepSorgusu();
        if (User.IsInRole("Depo"))
            query = query.Where(x => x.TalepEdenKullaniciId == kullaniciId.Value);

        query = Filtrele(query, arama, durum, oncelik, urunId, baslangicTarihi, bitisTarihi);
        var model = await Sayfala(query, arama, durum, oncelik, urunId, baslangicTarihi, bitisTarihi, page, pageSize);
        model.Urunler = await UrunSecenekleri();

        var ozetQuery = _context.DepoSiparisTalepleri.AsNoTracking();
        if (User.IsInRole("Depo"))
            ozetQuery = ozetQuery.Where(x => x.TalepEdenKullaniciId == kullaniciId.Value);
        model.Ozet = new DepoSiparisTalebiOzetViewModel
        {
            OnayBekleyen = await ozetQuery.CountAsync(x => x.Durum == DepoSiparisTalebi.DurumOnayBekliyor),
            Onaylanan = await ozetQuery.CountAsync(x => x.Durum == DepoSiparisTalebi.DurumOnaylandi),
            TeslimAlinan = await ozetQuery.CountAsync(x => x.Durum == DepoSiparisTalebi.DurumTeslimAlindi),
            KritikOncelikli = await ozetQuery.CountAsync(x =>
                x.Oncelik == "Kritik" &&
                x.Durum != DepoSiparisTalebi.DurumReddedildi &&
                x.Durum != DepoSiparisTalebi.DurumIptalEdildi &&
                x.Durum != DepoSiparisTalebi.DurumTeslimAlindi)
        };
        return View(model);
    }

    public Task<IActionResult> BenimTaleplerim(
        string? arama, string durum = "Tumu", string oncelik = "Tumu",
        int page = 1, int pageSize = 10)
    {
        return Index(arama, durum, oncelik, null, null, null, page, pageSize);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Yonetici,Depo")]
    public async Task<IActionResult> Create()
    {
        await CreateVerileriniDoldur();
        return View(new DepoSiparisTalebiOlusturViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Yonetici,Depo")]
    public async Task<IActionResult> Create(DepoSiparisTalebiOlusturViewModel model)
    {
        var kullanici = await GirisYapanKullanici();
        if (kullanici is null) return Forbid();
        if (!DepoSiparisTalebi.Oncelikler.Contains(model.Oncelik))
            ModelState.AddModelError(nameof(model.Oncelik), "Geçerli bir öncelik seçiniz.");
        if (!DepoSiparisTalebiOlusturViewModel.TedarikTercihleri.Contains(model.TedarikTercihi))
            ModelState.AddModelError(
                nameof(model.TedarikTercihi),
                "Geçerli bir tedarik tercihi seçiniz.");

        model.Kalemler = model.Kalemler
            .Where(x => x.UrunId.HasValue || x.TalepAdedi > 1 || !string.IsNullOrWhiteSpace(x.Aciklama))
            .ToList();
        if (model.Kalemler.Count == 0)
            ModelState.AddModelError("", "Talep en az bir ürün kalemi içermelidir.");
        if (model.Kalemler.Where(x => x.UrunId.HasValue).GroupBy(x => x.UrunId).Any(x => x.Count() > 1))
            ModelState.AddModelError("", "Aynı ürün bir talepte yalnızca bir kez bulunabilir.");

        var urunIds = model.Kalemler.Where(x => x.UrunId.HasValue).Select(x => x.UrunId!.Value).Distinct().ToList();
        var urunler = await _context.Urunler.AsNoTracking()
            .Where(x => urunIds.Contains(x.Id) && x.AktifMi).ToListAsync();
        if (urunler.Count != urunIds.Count)
            ModelState.AddModelError("", "Seçilen ürünlerden biri bulunamadı veya aktif değil.");

        var iliskiIds = model.Kalemler
            .Where(x => x.UrunTedarikciId.HasValue)
            .Select(x => x.UrunTedarikciId!.Value)
            .Distinct()
            .ToList();

        var urunTedarikcileri = await _context.UrunTedarikcileri
            .AsNoTracking()
            .Include(x => x.Tedarikci)
            .Where(x => iliskiIds.Contains(x.Id))
            .ToListAsync();

        var urunlerinAktifIliskileri = await _context.UrunTedarikcileri
            .AsNoTracking()
            .Include(x => x.Tedarikci)
            .Where(x =>
                urunIds.Contains(x.UrunId) &&
                x.AktifMi &&
                x.Tedarikci.AktifMi)
            .ToListAsync();

        foreach (var satir in model.Kalemler.Where(x => x.UrunId.HasValue))
        {
            var urunId = satir.UrunId!.Value;
            var aktifIliskiler = urunlerinAktifIliskileri
                .Where(x => x.UrunId == urunId)
                .ToList();

            if (satir.UrunTedarikciId.HasValue)
            {
                var secilen = urunTedarikcileri.FirstOrDefault(x =>
                    x.Id == satir.UrunTedarikciId.Value &&
                    x.UrunId == urunId &&
                    x.AktifMi &&
                    x.Tedarikci.AktifMi);

                if (secilen is null)
                {
                    ModelState.AddModelError(
                        "",
                        "Seçilen tedarikçi bu ürün için aktif veya geçerli değil.");
                }
                else if (satir.TalepAdedi < secilen.MinimumSiparisAdedi)
                {
                    ModelState.AddModelError(
                        "",
                        $"Seçilen tedarikçi için minimum sipariş adedi {secilen.MinimumSiparisAdedi}'dur.");
                }
            }
            else if (aktifIliskiler.Count > 0)
            {
                ModelState.AddModelError("", "Her ürün kalemi için bir tedarikçi seçiniz.");
            }
            else if (User.IsInRole("Depo"))
            {
                ModelState.AddModelError(
                    "",
                    "Bu ürün için tanımlı tedarikçi bulunamadı. Lütfen ürün detayından tedarikçi tanımlayın.");
            }
        }

        if (!ModelState.IsValid)
        {
            if (model.Kalemler.Count == 0) model.Kalemler.Add(new());
            await CreateVerileriniDoldur();
            return View(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var simdi = DateTime.Now;
            var talep = new DepoSiparisTalebi
            {
                TalepNo = "TMP-" + Guid.NewGuid().ToString("N")[..20],
                TalepEdenKullaniciId = kullanici.Id,
                TalepEdenPersonelId = kullanici.PersonelId,
                TalepTarihi = simdi,
                Durum = DepoSiparisTalebi.DurumOnayBekliyor,
                Oncelik = model.Oncelik,
                Aciklama = model.Aciklama?.Trim(),
                OlusturmaTarihi = simdi
            };
            _context.DepoSiparisTalepleri.Add(talep);
            await _context.SaveChangesAsync();
            talep.TalepNo = $"DST-{talep.Id:D6}";

            foreach (var satir in model.Kalemler)
            {
                var urun = urunler.First(x => x.Id == satir.UrunId);
                var secilenIliski = satir.UrunTedarikciId.HasValue
                    ? urunTedarikcileri.FirstOrDefault(x =>
                        x.Id == satir.UrunTedarikciId.Value &&
                        x.UrunId == urun.Id &&
                        x.AktifMi)
                    : null;

                _context.DepoSiparisTalepKalemleri.Add(new DepoSiparisTalepKalemi
                {
                    DepoSiparisTalebiId = talep.Id,
                    UrunId = urun.Id,
                    TedarikciId = secilenIliski?.TedarikciId ?? urun.TedarikciId,
                    UrunTedarikciId = secilenIliski?.Id,
                    MevcutStok = urun.StokMiktari,
                    MinimumStok = urun.MinimumStok,
                    TalepAdedi = satir.TalepAdedi,
                    TahminiBirimMaliyet = secilenIliski?.NetBirimMaliyet ?? urun.AlisFiyati,
                    TahminiIndirimOrani = secilenIliski?.IndirimOrani ?? 0,
                    TahminiTeslimSuresiGun = secilenIliski?.TeslimSuresiGun,
                    Aciklama = satir.Aciklama?.Trim(),
                    OlusturmaTarihi = simdi
                });
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Basari"] = $"{talep.TalepNo} numaralı ürün sipariş talebi oluşturuldu. Stok henüz değişmedi.";
            return RedirectToAction(nameof(Details), new { id = talep.Id });
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", "Talep kaydedilirken beklenmeyen bir hata oluştu.");
            await CreateVerileriniDoldur();
            return View(model);
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var kullaniciId = KullaniciId();
        if (!kullaniciId.HasValue) return Forbid();
        var talep = await TalepSorgusu().FirstOrDefaultAsync(x => x.Id == id);
        if (talep is null) return NotFound();
        if (User.IsInRole("Depo") && talep.TalepEdenKullaniciId != kullaniciId.Value)
            return Forbid();

        return View(new DepoSiparisTalebiDetayViewModel
        {
            Talep = talep,
            TalepSahibiMi = talep.TalepEdenKullaniciId == kullaniciId.Value,
            OnaylayabilirMi = User.IsInRole("Admin") || User.IsInRole("Yonetici"),
            TeslimAlabilirMi = User.IsInRole("Admin") || User.IsInRole("Yonetici") || User.IsInRole("Depo")
        });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Onayla(int id, string rowVersion)
    {
        var talep = await _context.DepoSiparisTalepleri.Include(x => x.Kalemler).FirstOrDefaultAsync(x => x.Id == id);
        if (talep is null) return NotFound();
        if (talep.Durum != DepoSiparisTalebi.DurumOnayBekliyor)
            return Hata(id, "Yalnızca onay bekleyen talepler onaylanabilir.");
        if (!RowVersionAyarla(talep, rowVersion)) return Hata(id, "Talep sürüm bilgisi geçersiz.");
        foreach (var kalem in talep.Kalemler) kalem.OnaylananAdet = kalem.TalepAdedi;
        talep.Durum = DepoSiparisTalebi.DurumOnaylandi;
        talep.OnaylayanKullaniciId = KullaniciId();
        talep.OnayTarihi = DateTime.Now;
        talep.GuncellemeTarihi = DateTime.Now;
        if (!await Kaydet()) return RedirectToAction(nameof(Details), new { id });
        TempData["Basari"] = "Talep onaylandı. Ürün teslim alınana kadar stok değişmez.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Yonetici")]
    public async Task<IActionResult> Reddet(int id, string? redNedeni, string rowVersion)
    {
        var talep = await _context.DepoSiparisTalepleri.FirstOrDefaultAsync(x => x.Id == id);
        if (talep is null) return NotFound();
        if (talep.Durum != DepoSiparisTalebi.DurumOnayBekliyor) return Hata(id, "Yalnızca onay bekleyen talepler reddedilebilir.");
        if (string.IsNullOrWhiteSpace(redNedeni)) return Hata(id, "Lütfen red nedenini yazınız.");
        if (!RowVersionAyarla(talep, rowVersion)) return Hata(id, "Talep sürüm bilgisi geçersiz.");
        talep.Durum = DepoSiparisTalebi.DurumReddedildi;
        talep.RedNedeni = redNedeni.Trim();
        talep.OnaylayanKullaniciId = KullaniciId();
        talep.OnayTarihi = DateTime.Now;
        talep.GuncellemeTarihi = DateTime.Now;
        if (!await Kaydet()) return RedirectToAction(nameof(Details), new { id });
        TempData["Basari"] = "Talep reddedildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Yonetici,Depo")]
    public async Task<IActionResult> IptalEt(int id, string rowVersion)
    {
        var kullaniciId = KullaniciId();
        var talep = await _context.DepoSiparisTalepleri.FirstOrDefaultAsync(x => x.Id == id);
        if (talep is null) return NotFound();
        var yonetici = User.IsInRole("Admin") || User.IsInRole("Yonetici");
        if (!yonetici && talep.TalepEdenKullaniciId != kullaniciId) return Forbid();
        if (talep.Durum != DepoSiparisTalebi.DurumOnayBekliyor) return Hata(id, "Yalnızca onay bekleyen talepler iptal edilebilir.");
        if (!RowVersionAyarla(talep, rowVersion)) return Hata(id, "Talep sürüm bilgisi geçersiz.");
        talep.Durum = DepoSiparisTalebi.DurumIptalEdildi;
        talep.IptalTarihi = DateTime.Now;
        talep.GuncellemeTarihi = DateTime.Now;
        if (!await Kaydet()) return RedirectToAction(nameof(Details), new { id });
        TempData["Basari"] = "Talep iptal edildi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin,Yonetici,Depo")]
    public async Task<IActionResult> TeslimAl(int id, string rowVersion)
    {
        var kullaniciId = KullaniciId();
        if (!kullaniciId.HasValue) return Forbid();
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var talep = await _context.DepoSiparisTalepleri
                .Include(x => x.Kalemler).ThenInclude(x => x.Urun)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (talep is null) return NotFound();
            if (User.IsInRole("Depo") && talep.TalepEdenKullaniciId != kullaniciId.Value) return Forbid();
            if (talep.Durum != DepoSiparisTalebi.DurumOnaylandi)
                return Hata(id, "Yalnızca onaylanmış talepler teslim alınabilir.");
            if (!RowVersionAyarla(talep, rowVersion)) return Hata(id, "Talep sürüm bilgisi geçersiz.");
            var simdi = DateTime.Now;
            foreach (var kalem in talep.Kalemler)
            {
                if (kalem.OnaylananAdet <= 0) throw new InvalidOperationException("Onaylanan adet geçersiz.");
                kalem.TeslimAlinanAdet = kalem.OnaylananAdet;
                kalem.Urun.StokMiktari += kalem.TeslimAlinanAdet;
                _context.StokHareketleri.Add(new StokHareketi
                {
                    UrunId = kalem.UrunId,
                    HareketTipi = "Giris",
                    Miktar = kalem.TeslimAlinanAdet,
                    Tarih = simdi,
                    Aciklama = $"Depo sipariş talebi teslim alındı. Talep No: {talep.TalepNo}"
                });
            }
            talep.Durum = DepoSiparisTalebi.DurumTeslimAlindi;
            talep.TeslimAlanKullaniciId = kullaniciId.Value;
            talep.TeslimAlmaTarihi = simdi;
            talep.GuncellemeTarihi = simdi;
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            TempData["Basari"] = "Ürünler teslim alındı; stoklar ve stok hareketleri güncellendi.";
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            TempData["Hata"] = "Talep başka bir kullanıcı tarafından güncellendi.";
        }
        catch
        {
            await transaction.RollbackAsync();
            TempData["Hata"] = "Teslim alma işlemi tamamlanamadı; stok değişikliği geri alındı.";
        }
        return RedirectToAction(nameof(Details), new { id });
    }

    private IQueryable<DepoSiparisTalebi> TalepSorgusu() => _context.DepoSiparisTalepleri
        .AsNoTracking().Include(x => x.TalepEdenKullanici).Include(x => x.TalepEdenPersonel)
        .Include(x => x.OnaylayanKullanici).Include(x => x.TeslimAlanKullanici)
        .Include(x => x.Kalemler).ThenInclude(x => x.Urun)
        .Include(x => x.Kalemler).ThenInclude(x => x.Tedarikci)
        .Include(x => x.Kalemler).ThenInclude(x => x.UrunTedarikci);

    private static IQueryable<DepoSiparisTalebi> Filtrele(
        IQueryable<DepoSiparisTalebi> q, string? arama, string durum, string oncelik,
        int? urunId, DateTime? baslangic, DateTime? bitis)
    {
        if (!string.IsNullOrWhiteSpace(arama)) { var m = arama.Trim(); q = q.Where(x => x.TalepNo.Contains(m) || x.TalepEdenKullanici.KullaniciAdi.Contains(m) || x.Kalemler.Any(k => k.Urun.UrunAdi.Contains(m))); }
        if (Durumlar.Contains(durum)) q = q.Where(x => x.Durum == durum);
        if (DepoSiparisTalebi.Oncelikler.Contains(oncelik)) q = q.Where(x => x.Oncelik == oncelik);
        if (urunId.HasValue) q = q.Where(x => x.Kalemler.Any(k => k.UrunId == urunId));
        if (baslangic.HasValue) q = q.Where(x => x.TalepTarihi >= baslangic.Value.Date);
        if (bitis.HasValue) q = q.Where(x => x.TalepTarihi < bitis.Value.Date.AddDays(1));
        return q;
    }

    private static async Task<DepoSiparisTalebiListeViewModel> Sayfala(
        IQueryable<DepoSiparisTalebi> q, string? arama, string durum, string oncelik,
        int? urunId, DateTime? baslangic, DateTime? bitis, int page, int pageSize)
    {
        if (!new[] { 10, 25, 50, 100 }.Contains(pageSize)) pageSize = 10;
        page = Math.Max(1, page);
        var count = await q.CountAsync();
        var pages = Math.Max(1, (int)Math.Ceiling(count / (double)pageSize));
        page = Math.Min(page, pages);
        return new DepoSiparisTalebiListeViewModel
        {
            Arama = arama, Durum = Durumlar.Contains(durum) ? durum : "Tumu",
            Oncelik = DepoSiparisTalebi.Oncelikler.Contains(oncelik) ? oncelik : "Tumu",
            UrunId = urunId, BaslangicTarihi = baslangic, BitisTarihi = bitis,
            Page = page, PageSize = pageSize, TotalCount = count, TotalPages = pages,
            Items = await q.OrderByDescending(x => x.TalepTarihi).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync()
        };
    }

    private async Task CreateVerileriniDoldur()
    {
        var urunler = await _context.Urunler.AsNoTracking().Where(x => x.AktifMi)
            .OrderBy(x => x.StokMiktari > x.MinimumStok).ThenBy(x => x.UrunAdi)
            .Select(x => new { x.Id, Etiket = x.UrunAdi + " | Stok: " + x.StokMiktari + " | Min: " + x.MinimumStok }).ToListAsync();
        ViewData["Urunler"] = new SelectList(urunler, "Id", "Etiket");
        ViewData["KritikUrunler"] = await _context.Urunler.AsNoTracking().Where(x => x.AktifMi && x.StokMiktari <= x.MinimumStok).OrderBy(x => x.StokMiktari).Take(10).ToListAsync();

        ViewData["UrunBilgileriJson"] = await _context.Urunler
            .AsNoTracking()
            .Where(x => x.AktifMi)
            .OrderBy(x => x.UrunAdi)
            .Select(x => new
            {
                x.Id,
                x.UrunAdi,
                x.StokMiktari,
                x.MinimumStok,
                KritikMi = x.StokMiktari <= x.MinimumStok
            })
            .ToListAsync();

        ViewData["UrunTedarikcileriJson"] = await _context.UrunTedarikcileri
            .AsNoTracking()
            .Where(x => x.AktifMi && x.Tedarikci.AktifMi && x.Urun.AktifMi)
            .OrderBy(x => x.UrunId)
            .ThenByDescending(x => x.VarsayilanMi)
            .ThenBy(x => x.NetBirimMaliyet <= 0)
            .ThenBy(x => x.NetBirimMaliyet)
            .Select(x => new
            {
                x.Id,
                x.UrunId,
                x.TedarikciId,
                TedarikciAdi = x.Tedarikci.FirmaAdi,
                x.BirimMaliyet,
                x.IndirimOrani,
                x.NetBirimMaliyet,
                x.MinimumSiparisAdedi,
                x.TeslimSuresiGun,
                x.VarsayilanMi
            })
            .ToListAsync();
    }

    private async Task<List<SelectListItem>> UrunSecenekleri() => await _context.Urunler.AsNoTracking().Where(x => x.AktifMi).OrderBy(x => x.UrunAdi).Select(x => new SelectListItem(x.UrunAdi, x.Id.ToString())).ToListAsync();
    private int? KullaniciId() => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    private async Task<Kullanici?> GirisYapanKullanici() { var id = KullaniciId(); return id.HasValue ? await _context.Kullanicilar.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.AktifMi) : null; }
    private IActionResult Hata(int id, string mesaj) { TempData["Hata"] = mesaj; return RedirectToAction(nameof(Details), new { id }); }
    private bool RowVersionAyarla(DepoSiparisTalebi talep, string value) { try { _context.Entry(talep).Property(x => x.RowVersion).OriginalValue = Convert.FromBase64String(value); return true; } catch { return false; } }
    private async Task<bool> Kaydet() { try { await _context.SaveChangesAsync(); return true; } catch (DbUpdateConcurrencyException) { TempData["Hata"] = "Talep başka bir kullanıcı tarafından güncellendi."; return false; } }
}
