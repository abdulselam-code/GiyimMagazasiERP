namespace GiyimMagazasiERP.ViewModels;

public class FaturaListeViewModel
{
    public int SatisId { get; set; }
    public string FaturaNo { get; set; } = null!;
    public DateTime SatisTarihi { get; set; }
    public string MusteriAdi { get; set; } = null!;
    public string SatisTuru { get; set; } = "Perakende";
    public string OdemeTipi { get; set; } = null!;
    public decimal ToplamTutar { get; set; }
}