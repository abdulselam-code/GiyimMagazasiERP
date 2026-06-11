namespace GiyimMagazasiERP.ViewModels;

public class KasaKapanisiOzetViewModel
{
    public decimal BeklenenNakit { get; set; }
    public decimal BeklenenKrediKarti { get; set; }
    public decimal BeklenenHavale { get; set; }
    public decimal BeklenenToplam { get; set; }
    public int SatisSayisi { get; set; }
    public int IadeSayisi { get; set; }
    public decimal IadeToplami { get; set; }
    public decimal DagitilamayanIadeToplami { get; set; }
}
