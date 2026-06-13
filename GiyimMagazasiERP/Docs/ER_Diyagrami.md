# Giyim Mağazası ERP - ER Diyagramı

Bu diyagram sistemin ana operasyonel tablolarını ve proje yönetimi tablolarını
özetler. Projede ayrı bir `Faturalar` tablosu bulunmaz; fatura numarası, seri,
tarih ve durum alanları `Satislar` tablosunda tutulur.

Proje yönetimi örnek verisi `21.05.2026 - 20.06.2026` tarih aralığını kullanır.
Bu aralık başlangıç ve bitiş günleri dahil 31 günlük teslim takvimidir.

```mermaid
erDiagram
    Kategoriler ||--o{ AltKategoriler : kapsar
    Kategoriler ||--o{ Urunler : siniflandirir
    AltKategoriler ||--o{ Urunler : detaylandirir
    Tedarikciler ||--o{ Urunler : ana_tedarikci
    Urunler ||--o{ UrunTedarikcileri : tedarik_edilir
    Tedarikciler ||--o{ UrunTedarikcileri : saglar

    Musteriler ||--o{ Satislar : satin_alir
    Personeller ||--o{ Satislar : gerceklestirir
    Satislar ||--|{ SatisDetaylari : icerir
    Urunler ||--o{ SatisDetaylari : satilir
    Satislar ||--o{ FinansHareketleri : gelir_olusturur
    Kullanicilar ||--o{ FinansHareketleri : kaydeder

    Satislar ||--o{ IadeDegisimTalepleri : kaynak_satis
    Musteriler ||--o{ IadeDegisimTalepleri : talep_sahibi
    IadeDegisimTalepleri ||--|{ IadeDegisimTalepDetaylari : icerir
    Urunler ||--o{ IadeDegisimTalepDetaylari : iade_edilir
    IadeDegisimTalepleri ||--o| FinansHareketleri : iade_hareketi

    Urunler ||--o{ StokHareketleri : stok_izi
    Kullanicilar ||--o| Personeller : personel_hesabi
    Personeller ||--o{ PersonelIzinleri : izin_kaydi
    Personeller ||--o{ PersonelMesaiKayitlari : mesai_kaydi
    Personeller ||--o{ KasaKapanislari : kasa_sayimi

    Kullanicilar ||--o{ DepoSiparisTalepleri : talep_eder
    Personeller ||--o{ DepoSiparisTalepleri : depo_personeli
    DepoSiparisTalepleri ||--|{ DepoSiparisTalepKalemleri : icerir
    Urunler ||--o{ DepoSiparisTalepKalemleri : siparis_edilir
    Tedarikciler ||--o{ DepoSiparisTalepKalemleri : tedarik_eder
    UrunTedarikcileri ||--o{ DepoSiparisTalepKalemleri : secilen_teklif

    Projeler ||--o{ ProjeGorevleri : gorevlere_sahip
    Projeler ||--o{ ProjeEkipUyeleri : ekibe_sahip
    Projeler ||--o{ ProjeButceKalemleri : butceye_sahip
    ProjeEkipUyeleri ||--o{ ProjeGorevleri : sorumludur
    ProjeGorevleri ||--o{ ProjeGorevBagimliliklari : bagimli_gorev
    ProjeGorevleri ||--o{ ProjeGorevBagimliliklari : oncul_gorev
```

## Ana İlişki Notları

- `Satislar` satış başlığı ve fatura alanlarını, `SatisDetaylari` ürün
  satırlarını tutar.
- `UrunTedarikcileri`, bir ürünün birden fazla tedarikçiyle maliyet ve teslim
  süresi bazında ilişkilendirilmesini sağlar.
- `IadeDegisimTalepleri` tamamlandığında ürün stokları ve ilgili
  `FinansHareketleri` kayıtları etkilenebilir.
- `Puantaj` ayrı bir tablo değildir; `PersonelIzinleri` ve
  `PersonelMesaiKayitlari` üzerinden anlık hesaplanan rapordur.
- `DepoSiparisTalepleri` yalnız teslim alma aşamasında `Urunler.StokMiktari`
  değerini artırır ve `StokHareketleri` kaydı oluşturur.
- `ProjeGorevBagimliliklari`, kritik yol ve bolluk süresi hesabında kullanılan
  görev öncüllerini saklar.
