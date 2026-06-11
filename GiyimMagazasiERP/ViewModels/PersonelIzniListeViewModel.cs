using GiyimMagazasiERP.Models;

namespace GiyimMagazasiERP.ViewModels;

public class PersonelIzniListeViewModel
{
    public string? Arama { get; set; }
    public string Durum { get; set; } = "Tumu";
    public string IzinTuru { get; set; } = "Tumu";
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool PersonelEslesmesiVarMi { get; set; } = true;
    public List<PersonelIzni> Items { get; set; } = new();
    public PersonelIzinBakiyesiViewModel? IzinBakiyesi { get; set; }
    public decimal ToplamYillikIzinHakki { get; set; }
    public decimal ToplamKullanilanIzinGunu { get; set; }
    public decimal ToplamKalanIzinGunu { get; set; }
    public decimal DigerOnayliIzinGunu { get; set; }
    public int OnayBekleyenIzinSayisi { get; set; }

    public bool OncekiSayfaVarMi => Page > 1;
    public bool SonrakiSayfaVarMi => Page < TotalPages;
}
