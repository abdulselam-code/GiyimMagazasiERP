# Giyim Mağazası ERP

ASP.NET Core MVC ve SQL Server ile geliştirilen; satış, stok, müşteri, finans,
fatura, toptan satış ve iade süreçlerini yöneten mağaza ERP sistemidir.

## Kullanılan Teknolojiler

- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- Bootstrap
- Chart.js
- Razor View
- C#

## Ana Modüller

- Role göre Dashboard ve ortak Ana Sayfa
- Perakende satış
- Fatura görüntüleme ve yazdırma
- Toptan satış talep, yönetici onayı ve muhasebe tamamlama süreci
- İade / Değişim Modülü
- İade belgeleri
- Finans ve stok hareketleri
- Ürün, kategori ve alt kategori yönetimi
- Tedarikçi ve tedarikçi-alt kategori yönetimi
- Ürün bazlı çoklu tedarikçi, maliyet, indirim ve teslim süresi karşılaştırması
- Müşteri ve personel yönetimi
- Personel izin talebi, onay/red ve yıllık izin bakiyesi yönetimi
- Personel mesai ve vardiya planlama, talep ve onay yönetimi
- Aylık otomatik puantaj raporu
- Depo ürün sipariş ve stok tamamlama talebi
- Kasa kapanışı / gün sonu mutabakatı
- Tarih filtreli raporlar
- Kritik stok takibi
- Proje yönetimi, ekip/efor raporu, Gantt şemaları ve kritik yol analizi
- Proje geliştirme bütçesi ve veri tabanı dokümantasyonu

## Roller

- **Admin:** Tüm işletme süreçlerini ve teknik yönetim panellerini kullanır.
- **Yonetici:** Satış, stok, personel, rapor ve onay süreçlerini yönetir.
- **Muhasebe:** Finans, fatura, rapor, toptan satış ve iade tamamlama süreçlerini izler.
- **Kasiyer:** Perakende satış yapar; kendi satış, fatura ve taleplerini takip eder.
- **Personel:** Kendisine ait toptan satış ve iade/değişim taleplerini yönetir.
- **Depo:** Ürün, kategori, tedarikçi, stok hareketi ve iadeden dönen ürünleri yönetir.
- **InsanKaynaklari:** Personel bilgileri ve personel özetlerini takip eder.

## Öne Çıkan İş Akışları

1. Perakende satış tamamlanır, stok düşer, finans hareketi ve fatura oluşur.
2. Toptan satış talebi oluşturulur, yönetici onayı ve muhasebe kontrolünden geçer.
3. İade/değişim talebi yönetici tarafından değerlendirilir ve muhasebe tarafından tamamlanır.
4. Tamamlanan iadeler stok ve finans hareketlerine yansır.
5. İade belgesi uygulama içinden görüntülenir ve yazdırılır.
6. Finans ekranında satış gelirleri, giderler ve satış iadeleri izlenir.
7. Kritik stok ürünleri Dashboard ve ürün listesi üzerinden takip edilir.
8. Personel izin talepleri rol bazlı olarak oluşturulur, profesyonel onay ekranlarıyla yönetilir ve yıllık izin bakiyesine göre kontrol edilir.
9. Kasiyer gün sonu sayımını girer; beklenen ve sayılan tutarlar Yönetici/Muhasebe tarafından mutabakata alınır.
10. Ürün kartı master data olarak ayrı yönetilir; mevcut ürünün stok tamamlaması
    Depo Sipariş Talebi üzerinden seçilen ürün tedarikçisiyle yürütülür.
11. Proje görevleri ekip, süre, efor, bütçe ve bağımlılık bilgileriyle izlenir;
    Gantt ve kritik yol ekranları teslim planını görünür hale getirir.

## Ürün Kartı ve Depo Sipariş Talebi

**Yeni Ürün** ekranı barkod, kategori, fiyat ve stok gibi ürün kartı bilgilerini
tanımlar; satın alma veya stok giriş işlemi oluşturmaz. Bir ürünün birden fazla
tedarikçisi ürün detayındaki **Bağlı Tedarikçiler** bölümünden maliyet, indirim,
minimum sipariş ve teslim süresiyle tanımlanabilir.

**Depo Sipariş Talebi** sistemde kayıtlı bir ürünün stoğunu tamamlamak içindir.
Ürün seçildiğinde yalnız o ürüne bağlı aktif tedarikçiler karşılaştırılır.
Tedarik tercihi **En Uygun Fiyat**, **En Hızlı Teslimat** veya **Dengeli
Seçim** olarak belirlenebilir. Bu tercih karar desteği ve otomatik öneri
sağlar; kullanıcı tedarikçiyi elle değiştirebilir.
Talep ve yönetici onayı stoğu değiştirmez; stok yalnız teslim alma işleminde
artırılır ve stok giriş hareketi oluşur.

`023_seed_alternatif_urun_tedarikcileri.sql` demo ortamında seçili tekstil,
çocuk ve aksesuar ürünlerine gerçekçi alternatif tedarikçiler ekler. Mevcut
ürün-tedarikçi maliyetlerini değiştirmez; sıfır teslim sürelerini tamamlar ve
karşılaştırma ekranı için hızlı/uygun maliyetli alternatifler hazırlar.

## Kurulum

1. `appsettings.json` içindeki `DefaultConnection` bağlantısını SQL Server ortamınıza göre düzenleyin.
2. SQL Server üzerinde `GiyimMagazasiERP` veritabanını oluşturun.
3. Önce temel veritabanı scriptini çalıştırın.
4. `GiyimMagazasiERP/Database/Updates` klasöründeki scriptleri dosya adı
   numarasına göre artan sırayla çalıştırın. Demo verileri hazırlayan `015`
   scriptinden sonra da `016-023` güncellemelerine sırayla devam edin.
5. Son modüller için sıra özellikle şöyledir:
   `019_create_personel_mesai_kayitlari.sql`,
   `020_fix_personel_mesai_fazla_mesai.sql`,
   `021_create_depo_siparis_talepleri.sql`,
   `022_create_urun_tedarikcileri.sql`,
   `023_seed_alternatif_urun_tedarikcileri.sql`,
   `024_create_proje_yonetimi.sql`.
6. Visual Studio ile çözümü açın, NuGet paketlerini geri yükleyin ve projeyi çalıştırın.

## Proje Yönetimi ve Veri Tabanı Dokümantasyonu

`/ProjeYonetimi` ekranı proje tamamlanma oranı, görev durumu, ekip iş yükü ve
geliştirme bütçesini gösterir. Göreve ve ekip üyesine göre Gantt ekranları,
görev bağımlılıklarından hesaplanan kritik yol/bolluk süresi ve modül bazlı
raporlar teslim kapsamındadır.

Proje çalışma takvimi **21.05.2026 - 20.06.2026** aralığıdır. Toplam takvim
süresi başlangıç ve bitiş günleri dahil **31 gün** olarak gösterilir. Kritik yol
analizi, bu takvim içindeki görev bağımlılıklarını ve sıfır bolluklu işleri
işaretler.

ER Diyagramı: [Docs/ER_Diyagrami.md](GiyimMagazasiERP/Docs/ER_Diyagrami.md)

## Demo Kullanıcıları

`015` scripti şu kullanıcıları hazırlar:

`admin`, `yonetici1`, `muhasebe1`, `kasa1`, `kasa2`, `personel1`,
`personel2`, `depo1`, `ik1`

Demo parolası `Erp2026!` olarak hazırlanmıştır. Gerçek kullanım öncesinde bütün
demo parolaları değiştirilmelidir. Parolalar veritabanında açık metin olarak
tutulmaz.

## Önemli Not

Bu sistem eğitim/proje amaçlı geliştirilmiş bir ERP uygulamasıdır. Resmi
e-Fatura/e-Arşiv veya GİB entegrasyonu içermez. Uygulamadaki İade Belgesi,
kurum içi operasyon ve takip belgesidir; resmi mali belge değildir.
