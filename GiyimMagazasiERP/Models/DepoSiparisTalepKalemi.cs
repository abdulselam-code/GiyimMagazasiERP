using System.ComponentModel.DataAnnotations;

namespace GiyimMagazasiERP.Models;

public class DepoSiparisTalepKalemi
{
    public int Id { get; set; }
    public int DepoSiparisTalebiId { get; set; }
    public int UrunId { get; set; }
    public int? TedarikciId { get; set; }
    public int? UrunTedarikciId { get; set; }
    public int MevcutStok { get; set; }
    public int MinimumStok { get; set; }
    public int TalepAdedi { get; set; }
    public int OnaylananAdet { get; set; }
    public int TeslimAlinanAdet { get; set; }
    public decimal? TahminiBirimMaliyet { get; set; }
    public decimal? TahminiIndirimOrani { get; set; }
    public int? TahminiTeslimSuresiGun { get; set; }
    [MaxLength(300)] public string? Aciklama { get; set; }
    public DateTime OlusturmaTarihi { get; set; }

    public DepoSiparisTalebi DepoSiparisTalebi { get; set; } = null!;
    public Urun Urun { get; set; } = null!;
    public Tedarikci? Tedarikci { get; set; }
    public UrunTedarikci? UrunTedarikci { get; set; }
}
