using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class Proje
{
    public int Id { get; set; }
    [MaxLength(150)] public string ProjeAdi { get; set; } = "";
    [MaxLength(500)] public string? Aciklama { get; set; }
    [MaxLength(30)] public string Durum { get; set; } = "Devam Ediyor";
    public DateTime BaslangicTarihi { get; set; }
    public DateTime PlanlananBitisTarihi { get; set; }
    public decimal PlanlananButce { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public ICollection<ProjeGorevi> Gorevler { get; set; } = new List<ProjeGorevi>();
    public ICollection<ProjeEkipUyesi> EkipUyeleri { get; set; } = new List<ProjeEkipUyesi>();
    public ICollection<ProjeButceKalemi> ButceKalemleri { get; set; } = new List<ProjeButceKalemi>();
}
