namespace GiyimMagazasiERP.ViewModels;

public class PuantajIndexViewModel
{
    public int Ay { get; set; }
    public int Yil { get; set; }
    public string? Arama { get; set; }
    public string? Departman { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public List<string> Departmanlar { get; set; } = new();
    public List<PuantajPersonelSatirViewModel> Items { get; set; } = new();
    public decimal ToplamPlanlananSaat { get; set; }
    public decimal ToplamGerceklesenSaat { get; set; }
    public decimal ToplamOnayliFazlaMesai { get; set; }
    public int ToplamIzinGunu { get; set; }
    public int BekleyenMesaiTalebi { get; set; }
}
