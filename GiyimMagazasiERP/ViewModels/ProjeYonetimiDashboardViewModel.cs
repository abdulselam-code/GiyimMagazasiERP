using GiyimMagazasiERP.Models;

namespace GiyimMagazasiERP.ViewModels;

public class ProjeYonetimiDashboardViewModel
{
    public Proje? Proje { get; set; }
    public int ToplamGorev { get; set; }
    public int TamamlananGorev { get; set; }
    public int DevamEdenGorev { get; set; }
    public int KalanGorev { get; set; }
    public int KritikGorev { get; set; }
    public decimal TamamlanmaOrani { get; set; }
    public decimal PlanlananButce { get; set; }
    public decimal GerceklesenGider { get; set; }
    public decimal NetButce { get; set; }
    public List<ProjeGorevi> SonGorevler { get; set; } = new();
}
