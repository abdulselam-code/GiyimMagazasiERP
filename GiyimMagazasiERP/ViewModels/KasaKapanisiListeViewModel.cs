using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GiyimMagazasiERP.ViewModels;

public class KasaKapanisiListeViewModel
{
    public string? Arama { get; set; }
    public string Durum { get; set; } = "Tumu";
    public int? KasaPersonelId { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public decimal BugunkuBeklenenToplam { get; set; }
    public decimal BugunkuSayilanToplam { get; set; }
    public decimal BugunkuFark { get; set; }
    public int OnayBekleyenKapanis { get; set; }
    public List<KasaKapanisi> Items { get; set; } = new();
    public List<SelectListItem> Kasiyerler { get; set; } = new();

    public bool OncekiSayfaVarMi => Page > 1;
    public bool SonrakiSayfaVarMi => Page < TotalPages;
}
