using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class ProjeButceKalemi
{
    public int Id { get; set; }
    public int ProjeId { get; set; }
    [MaxLength(150)] public string KalemAdi { get; set; } = "";
    [MaxLength(20)] public string KalemTuru { get; set; } = "Gider";
    [MaxLength(50)] public string Kategori { get; set; } = "";
    public decimal PlanlananTutar { get; set; }
    public decimal GerceklesenTutar { get; set; }
    [MaxLength(300)] public string? Aciklama { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public Proje Proje { get; set; } = null!;
}
