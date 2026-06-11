namespace GiyimMagazasiERP.ViewModels;

public class PuantajPersonelSatirViewModel
{
    public int PersonelId { get; set; }
    public string PersonelAdi { get; set; } = "";
    public string Departman { get; set; } = "";
    public string Pozisyon { get; set; } = "";
    public decimal PlanlananSaat { get; set; }
    public decimal GerceklesenSaat { get; set; }
    public decimal OnayliFazlaMesai { get; set; }
    public int NormalVardiyaSayisi { get; set; }
    public int FazlaMesaiKayitSayisi { get; set; }
    public int BekleyenMesaiSayisi { get; set; }
    public int YillikIzinGunu { get; set; }
    public int DigerIzinGunu { get; set; }
    public int ToplamIzinGunu => YillikIzinGunu + DigerIzinGunu;
    public string DurumNotu { get; set; } = "Normal";
}
