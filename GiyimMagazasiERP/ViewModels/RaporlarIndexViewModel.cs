namespace GiyimMagazasiERP.ViewModels;

public class RaporlarIndexViewModel
{
    public GenelFinansOzetiViewModel GenelFinansOzeti { get; set; } = new();

    public List<GunlukSatisOzetiViewModel> GunlukSatisOzetleri { get; set; } = new();
    public List<AylikSatisOzetiViewModel> AylikSatisOzetleri { get; set; } = new();
    public List<YillikSatisOzetiViewModel> YillikSatisOzetleri { get; set; } = new();

    public List<AylikGelirGiderRaporuViewModel> AylikGelirGiderRaporu { get; set; } = new();
    public List<YillikGelirGiderRaporuViewModel> YillikGelirGiderRaporu { get; set; } = new();
    public List<GiderKategorisiRaporuViewModel> GiderKategorileriRaporu { get; set; } = new();
    public List<EnYuksekGiderViewModel> EnYuksekGiderler { get; set; } = new();

    public List<EnCokSatilanUrunViewModel> EnCokSatilanUrunler { get; set; } = new();
    public List<HicSatilmayanUrunViewModel> HicSatilmayanUrunler { get; set; } = new();
    public List<KritikStokRaporuViewModel> KritikStokRaporu { get; set; } = new();
    public List<EnCokAlisverisYapanMusteriViewModel> EnCokAlisverisYapanMusteriler { get; set; } = new();
    public List<MusteriUrunAnaliziViewModel> MusteriUrunAnalizi { get; set; } = new();
    public List<PersonelSatisPerformansiViewModel> PersonelSatisPerformansi { get; set; } = new();
    public List<TedarikciUrunIndirimRaporuViewModel> TedarikciUrunIndirimRaporu { get; set; } = new();
    public List<KategoriBazliSatisRaporuViewModel> KategoriBazliSatisRaporu { get; set; } = new();

    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string Donem { get; set; } = "BuAy";

    public int DonemToplamSatisSayisi { get; set; }
    public decimal DonemToplamSatisTutari { get; set; }
    public decimal DonemToplamIndirim { get; set; }
    public decimal DonemToplamNetSatis { get; set; }
    public decimal DonemOrtalamaSatisTutari { get; set; }
    public decimal DonemNetKarZarar { get; set; }

    public List<OdemeTipiGelirRaporuViewModel> OdemeTipineGoreGelirler { get; set; } = new();
}

public class GenelFinansOzetiViewModel
{
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKazanc { get; set; }
    public decimal BaslangicSermayesi { get; set; }
}

public class GunlukSatisOzetiViewModel
{
    public DateTime Gun { get; set; }
    public int SatisSayisi { get; set; }
    public decimal ToplamNetSatis { get; set; }
}

public class AylikSatisOzetiViewModel
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public int SatisSayisi { get; set; }
    public decimal ToplamSatisTutari { get; set; }
    public decimal ToplamIndirim { get; set; }
    public decimal NetSatis { get; set; }

    public string AyEtiketi => $"{Yil}-{Ay:D2}";
}

public class YillikSatisOzetiViewModel
{
    public int Yil { get; set; }
    public int SatisSayisi { get; set; }
    public decimal ToplamSatisTutari { get; set; }
    public decimal ToplamIndirim { get; set; }
    public decimal NetSatis { get; set; }
}

public class AylikGelirGiderRaporuViewModel
{
    public int Yil { get; set; }
    public int Ay { get; set; }
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKazanc => ToplamGelir - ToplamGider;

    public string AyEtiketi => $"{Yil}-{Ay:D2}";
}

public class YillikGelirGiderRaporuViewModel
{
    public int Yil { get; set; }
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKazanc => ToplamGelir - ToplamGider;
}

public class GiderKategorisiRaporuViewModel
{
    public string GiderKategorisi { get; set; } = null!;
    public decimal ToplamTutar { get; set; }
    public int HareketSayisi { get; set; }
}

public class EnYuksekGiderViewModel
{
    public DateTime Tarih { get; set; }
    public string Kategori { get; set; } = null!;
    public string? Aciklama { get; set; }
    public decimal Tutar { get; set; }
}

public class EnCokSatilanUrunViewModel
{
    public string UrunAdi { get; set; } = null!;
    public int ToplamSatilanAdet { get; set; }
    public decimal ToplamSatisTutari { get; set; }
}

public class HicSatilmayanUrunViewModel
{
    public string UrunAdi { get; set; } = null!;
    public string Barkod { get; set; } = null!;
    public int StokMiktari { get; set; }
}

public class KritikStokRaporuViewModel
{
    public string UrunAdi { get; set; } = null!;
    public int StokMiktari { get; set; }
    public int MinimumStok { get; set; }
    public string KategoriAdi { get; set; } = null!;
}

public class EnCokAlisverisYapanMusteriViewModel
{
    public string MusteriAdi { get; set; } = null!;
    public decimal ToplamHarcama { get; set; }
    public int SadakatPuani { get; set; }
    public decimal IndirimOrani { get; set; }
}

public class MusteriUrunAnaliziViewModel
{
    public string MusteriAdi { get; set; } = null!;
    public string UrunAdi { get; set; } = null!;
    public int ToplamAdet { get; set; }
}

public class PersonelSatisPerformansiViewModel
{
    public string PersonelAdi { get; set; } = null!;
    public int SatisSayisi { get; set; }
    public decimal ToplamSatisTutari { get; set; }
    public decimal PrimOrani { get; set; }
}

public class TedarikciUrunIndirimRaporuViewModel
{
    public string FirmaAdi { get; set; } = null!;
    public int UrunSayisi { get; set; }
    public decimal IndirimOrani { get; set; }
}

public class KategoriBazliSatisRaporuViewModel
{
    public string KategoriAdi { get; set; } = null!;
    public int ToplamSatilanAdet { get; set; }
    public decimal ToplamSatisTutari { get; set; }
}
public class OdemeTipiGelirRaporuViewModel
{
    public string OdemeTipi { get; set; } = "-";
    public int SatisSayisi { get; set; }
    public decimal ToplamGelir { get; set; }
}
public class HicSatilmayanUrunDetayViewModel
{
    public string UrunAdi { get; set; } = null!;
    public string Barkod { get; set; } = null!;
    public string KategoriAdi { get; set; } = "-";
    public string TedarikciAdi { get; set; } = "-";
    public int StokMiktari { get; set; }
    public int MinimumStok { get; set; }
    public decimal SatisFiyati { get; set; }
    public bool AktifMi { get; set; }
}