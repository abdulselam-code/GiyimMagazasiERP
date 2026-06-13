namespace GiyimMagazasiERP.ViewModels;

public class ProjeGanttViewModel
{
    public string Baslik { get; set; } = "";
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public List<ProjeGanttSatirViewModel> Satirlar { get; set; } = new();
}

public class ProjeGanttSatirViewModel
{
    public string Baslik { get; set; } = "";
    public string AltBaslik { get; set; } = "";
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public string Durum { get; set; } = "";
    public string Oncelik { get; set; } = "";
    public int TamamlanmaYuzdesi { get; set; }
    public decimal SolYuzde { get; set; }
    public decimal GenislikYuzde { get; set; }
}
