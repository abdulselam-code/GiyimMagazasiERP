namespace GiyimMagazasiERP.ViewModels;

public class DashboardViewModel
{
    // Mevcut özet kartları
    public int ToplamUrunSayisi { get; set; }
    public int ToplamMusteriSayisi { get; set; }
    public int ToplamPersonelSayisi { get; set; }
    public int ToplamSatisSayisi { get; set; }
    public decimal ToplamGelir { get; set; }
    public decimal ToplamGider { get; set; }
    public int KritikStokSayisi { get; set; }

    // Günlük satış grafiği
    public List<string> GunlukSatisEtiketleri { get; set; } = new();
    public List<decimal> GunlukSatisTutarlar { get; set; } = new();

    // Gelir gider grafiği
    public List<string> GelirGiderEtiketleri { get; set; } = new();
    public List<decimal> GelirGiderTutarlar { get; set; } = new();

    // Kategori bazlı satış grafiği
    public List<string> KategoriSatisEtiketleri { get; set; } = new();
    public List<decimal> KategoriSatisTutarlar { get; set; } = new();

    // En çok satılan ürün grafiği
    public List<string> EnCokSatilanUrunEtiketleri { get; set; } = new();
    public List<int> EnCokSatilanUrunAdetleri { get; set; } = new();

    // Kritik stok grafiği
    public List<string> KritikStokEtiketleri { get; set; } = new();
    public List<int> KritikStokMiktarlari { get; set; } = new();
}