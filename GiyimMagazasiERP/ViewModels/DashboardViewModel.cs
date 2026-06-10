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
    public int ToplamTedarikci { get; set; }

    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKazanc { get; set; }

    public int BugunkuSatisSayisi { get; set; }
    public decimal BugunkuSatisGeliri { get; set; }
    public decimal BugunkuGelir { get; set; }
    public decimal AylikGelir { get; set; }
    public decimal AylikGider { get; set; }
    public decimal OrtalamaMaas { get; set; }
    public decimal EnYuksekGider { get; set; }

    public DateTime BugununTarihi { get; set; }
    public DateTime BuAyBaslangicTarihi { get; set; }
    public DateTime BuAyBitisTarihi { get; set; }

    public DateTime? IsletmeKurulusTarihi { get; set; }
    public DateTime? IlkSatisTarihi { get; set; }
    public DateTime? SonSatisTarihi { get; set; }
    public DateTime? SonStokHareketiTarihi { get; set; }
    public DateTime? SonFinansHareketiTarihi { get; set; }

    public int AylikSatisSayisi { get; set; }
    public decimal AylikSatisGeliri { get; set; }
    public decimal BugunkuGider { get; set; }
    public decimal BugunkuNet { get; set; }
    public decimal BugunkuIadeTutari { get; set; }
    public decimal SatisIadeleriToplami { get; set; }
    public decimal NetKarZarar { get; set; }

    public int BekleyenToptanTalepSayisi { get; set; }
    public int BekleyenIadeTalepSayisi { get; set; }
    public int MuhasebeOnayiBekleyenToptanSayisi { get; set; }
    public int MuhasebeOnayiBekleyenIadeSayisi { get; set; }
    public int TamamlananIadeBelgesiSayisi { get; set; }
    public int BenimBekleyenToptanTalepSayisi { get; set; }
    public int BenimBekleyenIadeTalepSayisi { get; set; }

    public int BugunkuStokGirisAdedi { get; set; }
    public int BugunkuStokCikisAdedi { get; set; }
    public int BugunkuIadeGirisAdedi { get; set; }
    public int HasarliIncelemeIadeUrunSayisi { get; set; }

    public decimal EnYuksekMaas { get; set; }
    public decimal EnDusukMaas { get; set; }

    public string EnCokSatanUrunAdi { get; set; } = "Henüz satış yok";
    public string EnCokHarcamaYapanMusteriAdi { get; set; } = "Henüz müşteri yok";

    public bool KasiyerPersonelEslesmesiVarMi { get; set; } = true;
    public string? UyariMesaji { get; set; }

    public List<DashboardOdemeTipiGelirViewModel> OdemeTipineGoreGelirler { get; set; } = new();

    public List<DashboardQuickActionViewModel> HizliIslemler { get; set; } = new();
    public List<DashboardSonSatisViewModel> SonSatislar { get; set; } = new();
    public List<DashboardKritikStokViewModel> KritikStokUrunleri { get; set; } = new();
    public List<DashboardStokHareketiViewModel> SonStokHareketleri { get; set; } = new();
    public List<DashboardGiderViewModel> EnYuksekGiderler { get; set; } = new();
    public List<DashboardFinansHareketiViewModel> SonFinansHareketleri { get; set; } = new();
    public List<DashboardPersonelOzetViewModel> PersonelOzeti { get; set; } = new();
    public List<DashboardIadeBelgesiViewModel> SonIadeBelgeleri { get; set; } = new();
    public List<DashboardTalepOzetViewModel> BekleyenOnaylar { get; set; } = new();
    public List<DashboardTalepOzetViewModel> SonIadeTalepleri { get; set; } = new();
    public List<DashboardTalepOzetViewModel> SonToptanTalepleri { get; set; } = new();
    public List<DashboardIadeUrunViewModel> SorunluIadeUrunleri { get; set; } = new();

    public List<string> GunlukSatisLabels { get; set; } = new();
    public List<decimal> GunlukSatisValues { get; set; } = new();

    public List<string> GelirGiderLabels { get; set; } = new();
    public List<decimal> GelirGiderValues { get; set; } = new();

    public List<string> KategoriSatisLabels { get; set; } = new();
    public List<decimal> KategoriSatisValues { get; set; } = new();

    public List<string> EnCokSatilanUrunLabels { get; set; } = new();
    public List<int> EnCokSatilanUrunValues { get; set; } = new();

    public List<string> KritikStokLabels { get; set; } = new();
    public List<int> KritikStokValues { get; set; } = new();

    public List<string> AylikGelirGiderLabels { get; set; } = new();
    public List<decimal> AylikGelirValues { get; set; } = new();
    public List<decimal> AylikGiderValues { get; set; } = new();

    public List<string> StokHareketLabels { get; set; } = new();
    public List<int> StokGirisValues { get; set; } = new();
    public List<int> StokCikisValues { get; set; } = new();
    public List<int> IadeGirisValues { get; set; } = new();

    public List<string> IadeTrendLabels { get; set; } = new();
    public List<decimal> IadeTrendValues { get; set; } = new();
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

public class DashboardFinansHareketiViewModel
{
    public DateTime Tarih { get; set; }
    public string HareketTipi { get; set; } = null!;
    public string Kategori { get; set; } = null!;
    public decimal Tutar { get; set; }
}

public class DashboardPersonelOzetViewModel
{
    public string AdSoyad { get; set; } = null!;
    public string Pozisyon { get; set; } = null!;
    public string Departman { get; set; } = null!;
    public decimal Maas { get; set; }
}
public class DashboardOdemeTipiGelirViewModel
{
    public string OdemeTipi { get; set; } = "-";
    public int SatisSayisi { get; set; }
    public decimal ToplamGelir { get; set; }
}

public class DashboardIadeBelgesiViewModel
{
    public int Id { get; set; }
    public string BelgeNo { get; set; } = "-";
    public string TalepNo { get; set; } = "-";
    public DateTime Tarih { get; set; }
    public string MusteriAdi { get; set; } = "Nihai Tüketici";
    public decimal Tutar { get; set; }
}

public class DashboardTalepOzetViewModel
{
    public int Id { get; set; }
    public string Modul { get; set; } = "";
    public string TalepNo { get; set; } = "-";
    public string Durum { get; set; } = "-";
    public DateTime Tarih { get; set; }
    public decimal Tutar { get; set; }
}

public class DashboardIadeUrunViewModel
{
    public int TalepId { get; set; }
    public string TalepNo { get; set; } = "-";
    public string UrunAdi { get; set; } = "-";
    public string UrunDurumu { get; set; } = "-";
    public int Adet { get; set; }
}
