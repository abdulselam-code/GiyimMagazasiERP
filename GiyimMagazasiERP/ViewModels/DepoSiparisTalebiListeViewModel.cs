using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GiyimMagazasiERP.ViewModels;

public class DepoSiparisTalebiListeViewModel
{
    public string? Arama { get; set; }
    public string Durum { get; set; } = "Tumu";
    public string Oncelik { get; set; } = "Tumu";
    public int? UrunId { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<DepoSiparisTalebi> Items { get; set; } = new();
    public List<SelectListItem> Urunler { get; set; } = new();
    public DepoSiparisTalebiOzetViewModel Ozet { get; set; } = new();
}
