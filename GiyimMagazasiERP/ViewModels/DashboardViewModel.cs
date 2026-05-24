namespace GiyimMagazasiERP.ViewModels;

public class DashboardViewModel
{
    public string KullaniciAdi { get; set; } = null!;
    public string Rol { get; set; } = null!;

    public int ToplamUrunCesidi { get; set; }
    public int ToplamStokAdedi { get; set; }
    public int ToplamMusteri { get; set; }
    public int ToplamPersonel { get; set; }
    public int AktifPersonel { get; set; }
    public int ToplamSatis { get; set; }
    public int KritikStokSayisi { get; set; }

    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKazanc { get; set; }

    public int BugunkuSatisSayisi { get; set; }
    public decimal BugunkuSatisGeliri { get; set; }
    public decimal BugunkuGelir { get; set; }

    public decimal AylikGelir { get; set; }
    public decimal AylikGider { get; set; }

    public decimal OrtalamaMaas { get; set; }

    public List<DashboardQuickActionViewModel> HizliIslemler { get; set; } = new();
    public List<DashboardSonSatisViewModel> SonSatislar { get; set; } = new();
    public List<DashboardKritikStokViewModel> KritikStokUrunleri { get; set; } = new();
    public List<DashboardStokHareketiViewModel> SonStokHareketleri { get; set; } = new();
    public List<DashboardGiderViewModel> EnYuksekGiderler { get; set; } = new();
    public List<DashboardPersonelOzetViewModel> PersonelOzeti { get; set; } = new();
}

public class DashboardQuickActionViewModel
{
    public string Baslik { get; set; } = null!;
    public string Controller { get; set; } = null!;
    public string Action { get; set; } = "Index";
    public string Stil { get; set; } = "primary";
}

public class DashboardSonSatisViewModel
{
    public int SatisId { get; set; }
    public DateTime SatisTarihi { get; set; }
    public string MusteriAdi { get; set; } = null!;
    public decimal NetTutar { get; set; }
    public string OdemeTipi { get; set; } = null!;
}

public class DashboardKritikStokViewModel
{
    public string UrunAdi { get; set; } = null!;
    public int StokMiktari { get; set; }
    public int MinimumStok { get; set; }
}

public class DashboardStokHareketiViewModel
{
    public DateTime Tarih { get; set; }
    public string UrunAdi { get; set; } = null!;
    public string HareketTipi { get; set; } = null!;
    public int Miktar { get; set; }
}

public class DashboardGiderViewModel
{
    public DateTime Tarih { get; set; }
    public string Kategori { get; set; } = null!;
    public string? Aciklama { get; set; }
    public decimal Tutar { get; set; }
}

public class DashboardPersonelOzetViewModel
{
    public string AdSoyad { get; set; } = null!;
    public string Pozisyon { get; set; } = null!;
    public string Departman { get; set; } = null!;
    public decimal Maas { get; set; }
}