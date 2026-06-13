namespace GiyimMagazasiERP.Models;

public class ProjeGorevBagimliligi
{
    public int Id { get; set; }
    public int GorevId { get; set; }
    public int BagimliOlduguGorevId { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public ProjeGorevi Gorev { get; set; } = null!;
    public ProjeGorevi BagimliOlduguGorev { get; set; } = null!;
}
