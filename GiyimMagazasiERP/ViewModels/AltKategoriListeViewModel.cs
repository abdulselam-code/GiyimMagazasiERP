using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GiyimMagazasiERP.ViewModels;

public class AltKategoriListeViewModel
{
    public string? Arama { get; set; }
    public int? KategoriId { get; set; }
    public string Durum { get; set; } = "Tumu";

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public List<AltKategori> Items { get; set; } = new();
    public List<SelectListItem> Kategoriler { get; set; } = new();
}