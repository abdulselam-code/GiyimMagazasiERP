using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class ProjeGorevi
{
    public int Id { get; set; }
    public int ProjeId { get; set; }
    public int? SorumluEkipUyesiId { get; set; }
    [MaxLength(150)] public string GorevAdi { get; set; } = "";
    [MaxLength(500)] public string? Aciklama { get; set; }
    [MaxLength(100)] public string ModulAdi { get; set; } = "";
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public decimal PlanlananSaat { get; set; }
    public decimal GerceklesenSaat { get; set; }
    [MaxLength(30)] public string Durum { get; set; } = "Planlandı";
    [MaxLength(20)] public string Oncelik { get; set; } = "Normal";
    public int TamamlanmaYuzdesi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }
    public Proje Proje { get; set; } = null!;
    public ProjeEkipUyesi? SorumluEkipUyesi { get; set; }
    public ICollection<ProjeGorevBagimliligi> Bagimliliklar { get; set; } =
        new List<ProjeGorevBagimliligi>();
    public ICollection<ProjeGorevBagimliligi> BagimliGorevler { get; set; } =
        new List<ProjeGorevBagimliligi>();
}
