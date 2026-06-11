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
- Müşteri ve personel yönetimi
- Personel izin talebi, onay/red ve yıllık izin bakiyesi yönetimi
- Tarih filtreli raporlar
- Kritik stok takibi

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

## Kurulum

1. `appsettings.json` içindeki `DefaultConnection` bağlantısını SQL Server ortamınıza göre düzenleyin.
2. SQL Server üzerinde `GiyimMagazasiERP` veritabanını oluşturun.
3. `GiyimMagazasiERP/Database/Updates` klasöründeki scriptleri numara sırasıyla çalıştırın.
4. Demo veriler için en son `015_seed_demo_users_products_customers.sql` scriptini çalıştırın.
5. Visual Studio ile çözümü açın, NuGet paketlerini geri yükleyin ve projeyi çalıştırın.

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
