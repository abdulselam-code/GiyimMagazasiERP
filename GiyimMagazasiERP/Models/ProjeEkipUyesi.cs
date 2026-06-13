using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class ProjeEkipUyesi
{
    public int Id { get; set; }
    public int ProjeId { get; set; }
    [MaxLength(100)] public string AdSoyad { get; set; } = "";
    [MaxLength(100)] public string Rol { get; set; } = "";
    public bool AktifMi { get; set; } = true;
    public DateTime OlusturmaTarihi { get; set; }
    public Proje Proje { get; set; } = null!;
    public ICollection<ProjeGorevi> Gorevler { get; set; } = new List<ProjeGorevi>();
}
