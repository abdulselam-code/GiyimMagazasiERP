namespace GiyimMagazasiERP.ViewModels;

public class HomeIndexViewModel
{
    public int ToplamUrunSayisi { get; set; }
    public int ToplamMusteriSayisi { get; set; }
    public int ToplamPersonelSayisi { get; set; }
    public int ToplamSatisSayisi { get; set; }
    public int KritikStokSayisi { get; set; }

    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKazanc { get; set; }
}