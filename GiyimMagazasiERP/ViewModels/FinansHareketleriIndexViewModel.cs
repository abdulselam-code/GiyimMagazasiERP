using GiyimMagazasiERP.Models;

namespace GiyimMagazasiERP.ViewModels;

public class FinansHareketleriIndexViewModel
{
    public PagedResultViewModel<FinansHareketi> Hareketler { get; set; }
        = new();

    public string? Arama { get; set; }
    public string HareketTipi { get; set; } = "Tumu";
    public string Kategori { get; set; } = "Tumu";
    public DateTime? BaslangicTarihi { get; set; }
    public DateTime? BitisTarihi { get; set; }

    public List<string> Kategoriler { get; set; } = new();

    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetTutar => ToplamGelir - ToplamGider;
    public decimal SatisIadeleriToplami { get; set; }
    public decimal BugunkuNet { get; set; }
}
