namespace GiyimMagazasiERP.ViewModels;

public class VeritabaniYoneticiViewModel
{
    public string SqlSorgusu { get; set; } = "SELECT * FROM Urunler";
    public string? HataMesaji { get; set; }
    public string? BasariMesaji { get; set; }

    public VeritabaniIstatistikViewModel Istatistikler { get; set; } = new();

    public List<SemaTabloViewModel> SemaTablolari { get; set; } = new();
    public List<KayitliSorguViewModel> KayitliSorgular { get; set; } = new();
    public List<string> TabloTarayiciTablolari { get; set; } = new();

    public DinamikSonucTablosuViewModel? SonucTablosu { get; set; }
}

public class VeritabaniIstatistikViewModel
{
    public int ToplamTabloSayisi { get; set; }
    public int ToplamUrunSayisi { get; set; }
    public int ToplamStokAdedi { get; set; }
    public int ToplamMusteriSayisi { get; set; }
    public int ToplamPersonelSayisi { get; set; }
    public int ToplamSatisSayisi { get; set; }
    public int KritikStokSayisi { get; set; }

    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public decimal NetKazanc { get; set; }
}

public class SemaTabloViewModel
{
    public string TabloAdi { get; set; } = null!;
    public List<SemaAlanViewModel> Alanlar { get; set; } = new();
}

public class SemaAlanViewModel
{
    public string AlanAdi { get; set; } = null!;
    public string VeriTipi { get; set; } = null!;
    public bool PrimaryKeyMi { get; set; }
    public bool ForeignKeyMi { get; set; }
    public bool NotNullMi { get; set; }
}

public class KayitliSorguViewModel
{
    public string Kod { get; set; } = null!;
    public string Baslik { get; set; } = null!;
    public string Aciklama { get; set; } = null!;
    public string Kategori { get; set; } = null!;
}

public class DinamikSonucTablosuViewModel
{
    public string Baslik { get; set; } = null!;
    public string BosKayitMesaji { get; set; } = "Bu sorgu için kayıt bulunamadı.";

    public List<string> Sutunlar { get; set; } = new();
    public List<Dictionary<string, string>> Satirlar { get; set; } = new();

    public int KayitSayisi => Satirlar.Count;
}