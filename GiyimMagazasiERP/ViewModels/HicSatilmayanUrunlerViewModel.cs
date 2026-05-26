namespace GiyimMagazasiERP.ViewModels;

public class HicSatilmayanUrunlerViewModel
{
    public string? Arama { get; set; }
    public int Sayfa { get; set; } = 1;
    public int KayitSayisi { get; set; } = 20;
    public int ToplamKayit { get; set; }
    public int ToplamSayfa { get; set; }

    public List<HicSatilmayanUrunDetayViewModel> Urunler { get; set; } = new();
}