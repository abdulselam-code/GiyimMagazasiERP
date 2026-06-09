using GiyimMagazasiERP.Models;

namespace GiyimMagazasiERP.ViewModels;

public class IadeDegisimTalepOlusturViewModel
{
    public int? SatisId { get; set; }
    public string? Aciklama { get; set; }
    public string IslemTipi { get; set; } = IadeDegisimTalebi.IslemTipiIade;

    public List<IadeDegisimTalepUrunViewModel> Urunler { get; set; } = new();
    public List<int> SecilenSatisDetayiIdleri { get; set; } = new();
    public Dictionary<int, int> IadeAdetleri { get; set; } = new();
    public Dictionary<int, string?> IadeNedenleri { get; set; } = new();
    public Dictionary<int, string> UrunDurumlari { get; set; } = new();
    public Dictionary<int, bool> StogaGeriAlinsinMi { get; set; } = new();
}

public class IadeDegisimTalepUrunViewModel
{
    public int SatisDetayiId { get; set; }
    public int UrunId { get; set; }
    public string UrunAdi { get; set; } = "";
    public string Barkod { get; set; } = "";
    public string Beden { get; set; } = "";
    public string Renk { get; set; } = "";

    public int SatilanAdet { get; set; }
    public int DahaOnceIadeEdilenAdet { get; set; }
    public int AktifBekleyenIadeAdedi { get; set; }
    public int IadeEdilebilirAdet { get; set; }

    public decimal BirimFiyat { get; set; }
    public decimal KdvOrani { get; set; }
    public decimal SatirIndirimTutari { get; set; }
    public decimal KdvTutari { get; set; }
    public decimal VergiHaricTutar { get; set; }
    public decimal VergiDahilTutar { get; set; }
}
