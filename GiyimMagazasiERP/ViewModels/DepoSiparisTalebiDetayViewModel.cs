using GiyimMagazasiERP.Models;

namespace GiyimMagazasiERP.ViewModels;

public class DepoSiparisTalebiDetayViewModel
{
    public DepoSiparisTalebi Talep { get; set; } = null!;
    public bool TalepSahibiMi { get; set; }
    public bool OnaylayabilirMi { get; set; }
    public bool TeslimAlabilirMi { get; set; }
}
