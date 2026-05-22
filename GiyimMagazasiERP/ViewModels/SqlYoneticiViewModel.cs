namespace GiyimMagazasiERP.ViewModels;

public class SqlYoneticiViewModel
{
    public string? SeciliRaporKodu { get; set; }

    public List<HazirRaporSecenegiViewModel> HazirRaporlar { get; set; } = new();

    public DinamikRaporTablosuViewModel? SonucTablosu { get; set; }
}

public class HazirRaporSecenegiViewModel
{
    public string Kod { get; set; } = null!;
    public string Baslik { get; set; } = null!;
    public string Aciklama { get; set; } = null!;
}

public class DinamikRaporTablosuViewModel
{
    public string Baslik { get; set; } = null!;
    public string Aciklama { get; set; } = null!;

    public List<string> Sutunlar { get; set; } = new();

    public List<Dictionary<string, string>> Satirlar { get; set; } = new();
}