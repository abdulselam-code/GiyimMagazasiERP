using GiyimMagazasiERP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GiyimMagazasiERP.ViewModels;

public class PersonelMesaiListeViewModel
{
    public string? Arama { get; set; }
    public string Durum { get; set; } = "Tumu";
    public string MesaiTuru { get; set; } = "Tumu";
    public int? PersonelId { get; set; }
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool PersonelEslesmesiVarMi { get; set; } = true;
    public bool SadeceOnayliKayitlar { get; set; }
    public List<SelectListItem> Personeller { get; set; } = new();
    public List<PersonelMesaiKaydi> Items { get; set; } = new();
    public PersonelMesaiOzetViewModel Ozet { get; set; } = new();

    public bool OncekiSayfaVarMi => Page > 1;
    public bool SonrakiSayfaVarMi => Page < TotalPages;
}
