using GiyimMagazasiERP.Models;

namespace GiyimMagazasiERP.ViewModels;

public class ProjeRaporViewModel
{
    public int ToplamGorev { get; set; }
    public int Tamamlanan { get; set; }
    public int DevamEden { get; set; }
    public int Testte { get; set; }
    public int Geciken { get; set; }
    public decimal PlanlananButce { get; set; }
    public decimal GerceklesenGider { get; set; }
    public decimal KalanButce { get; set; }
    public decimal NetButce { get; set; }
    public List<ProjeEkipRaporSatiriViewModel> EkipRaporu { get; set; } = new();
    public List<ProjeModulRaporSatiriViewModel> ModulRaporu { get; set; } = new();
    public List<ProjeGorevi> KritikGorevler { get; set; } = new();
    public List<ProjeGorevi> GecikenGorevler { get; set; } = new();
    public List<ProjeButceKalemi> ButceAsanKalemler { get; set; } = new();
}

public class ProjeEkipRaporSatiriViewModel
{
    public string EkipUyesi { get; set; } = "";
    public string Rol { get; set; } = "";
    public int AtananGorev { get; set; }
    public int TamamlananGorev { get; set; }
    public int DevamEdenGorev { get; set; }
    public decimal PlanlananSaat { get; set; }
    public decimal GerceklesenSaat { get; set; }
    public decimal IsYukuYuzdesi { get; set; }
}

public class ProjeModulRaporSatiriViewModel
{
    public string ModulAdi { get; set; } = "";
    public int GorevSayisi { get; set; }
    public decimal TamamlanmaOrani { get; set; }
    public string TestDurumu { get; set; } = "";
}

public class ProjeButceViewModel
{
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetButce { get; set; }
    public decimal ButceKullanimOrani { get; set; }
    public List<ProjeButceKalemi> Kalemler { get; set; } = new();
}
