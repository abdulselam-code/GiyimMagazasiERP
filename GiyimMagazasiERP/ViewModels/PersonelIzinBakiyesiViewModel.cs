namespace GiyimMagazasiERP.ViewModels;

public class PersonelIzinBakiyesiViewModel
{
    public int PersonelId { get; set; }
    public string PersonelAdi { get; set; } = "";
    public string Pozisyon { get; set; } = "";
    public string Departman { get; set; } = "";
    public int Yil { get; set; }
    public decimal YillikIzinHakki { get; set; }
    public decimal DevredenIzinGunu { get; set; }
    public decimal KullanilanIzinGunu { get; set; }
    public decimal KalanIzinGunu { get; set; }
    public decimal DigerOnayliIzinGunu { get; set; }
    public decimal ToplamIzinHakki => YillikIzinHakki + DevredenIzinGunu;
}
