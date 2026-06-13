namespace GiyimMagazasiERP.ViewModels;

public class ProjeKritikYolViewModel
{
    public bool KritikYolVarMi { get; set; }
    public DateTime ProjeBaslangicTarihi { get; set; }
    public DateTime ProjeBitisTarihi { get; set; }
    public decimal ProjeSuresiGun { get; set; }
    public string KritikYolMetni { get; set; } = "";
    public List<ProjeKritikYolSatiriViewModel> Gorevler { get; set; } = new();
}

public class ProjeKritikYolSatiriViewModel
{
    public int GorevId { get; set; }
    public string GorevAdi { get; set; } = "";
    public decimal PlanlananSureGun { get; set; }
    public decimal EnErkenBaslangic { get; set; }
    public decimal EnErkenBitis { get; set; }
    public decimal EnGecBaslangic { get; set; }
    public decimal EnGecBitis { get; set; }
    public decimal BollukSuresi { get; set; }
    public bool KritikMi { get; set; }
}
