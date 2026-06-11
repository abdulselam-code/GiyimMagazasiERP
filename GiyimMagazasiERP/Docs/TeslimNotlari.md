# Teslim Notları

## Projenin Genel Durumu

Giyim Mağazası ERP; tek mağaza operasyonunda satış, stok, müşteri, finans,
fatura, toptan satış, iade/değişim ve raporlama süreçlerini rol bazlı olarak
yönetebilecek durumdadır.

## Tamamlanan Modüller

- Rol bazlı giriş, yetkilendirme ve Dashboard
- Perakende satış, stok düşümü, finans kaydı ve fatura
- KDV dahil fiyat ve karma KDV hesaplamaları
- Toptan satış talep ve onay akışı
- İade / Değişim talep, onay ve tamamlama akışı
- İade belgeleri
- Finans ve stok hareketleri
- Ürün, ana kategori, alt kategori ve tedarikçi yönetimi
- Tedarikçi-alt kategori ilişkileri
- Müşteri ve personel yönetimi
- Tarih filtreli raporlar
- Responsive Ana Sayfa ve Dashboard
- Kasa kapanışı / gün sonu mutabakatı, onay ve red süreci

## Demo Veriler

`Database/Updates/015_seed_demo_users_products_customers.sql` scripti:

- Dokuz temel rol hesabını hazırlar.
- Kullanıcıları uygun personel kayıtlarıyla ilişkilendirir.
- Şanlıurfa ağırlıklı bireysel ve kurumsal demo müşteriler ekler.
- Eksik müşteri iletişim alanlarını yalnızca boş olduklarında tamamlar.
- Aktif alt kategorilerde en az iki ürün bulunmasını sağlar.
- Hiç satılmamış sıfır stoklu ürünleri stok hareketi oluşturarak düzeltir.
- Mevcut satış, fatura, finans ve iade geçmişini silmez.

Demo kullanıcı parolaları gerçek kullanımdan önce değiştirilmelidir. Demo stok
ve müşteri verileri kurumun gerçek verileriyle değiştirilmelidir.

## Bilinen Sınırlamalar

- Resmi e-Fatura/e-Arşiv ve GİB entegrasyonu yoktur.
- İade Belgesi uygulama içi operasyon belgesidir; resmi mali belge değildir.
- Tek mağaza senaryosu esas alınmıştır.
- Depo ve karantina stokları ayrı fiziksel lokasyonlar olarak yönetilmemektedir.
- E-posta ve SMS bildirim altyapısı bulunmamaktadır.

## Teslim Öncesi Güvenlik

- Demo kullanıcı parolalarını değiştirin.
- SQL Server bağlantı bilgisini üretim ortamına göre güncelleyin.
- `appsettings*.json` dosyalarında gerçek parola veya gizli anahtar bırakmayın.
- Veritabanının tam yedeğini alın.
- Rol ve AccessDenied testlerini tekrar çalıştırın.

## Temizlik İncelemesi

- `.vs`, `bin` ve `obj` klasörleri `.gitignore` kapsamındadır ve repoda izlenmemektedir.
- Boş `NewFolder`, `stage`, `final` veya geçici teslim klasörü bulunmamıştır.
- `Views` altında 23 adet `*.cshtml.cs` dosyası bulunmaktadır.
- Bu dosyalar yalnızca boş `PageModel/OnGet` iskeletidir ve MVC controller/view
  akışından referans almamaktadır.
- Teslimden önce ayrı bir committe silinmeleri önerilir. Bu hazırlık aşamasında
  kullanıcı onayı olmadan silinmemişlerdir.

## Gelecek Geliştirmeler

- Resmi e-Fatura/e-Arşiv entegrasyonu
- Detaylı iade nedenleri raporu
- Ürün bazlı kârlılık raporu
- Depo/karantina stok yapısı
- Personel izin/puantaj modülü
- Barkod okuyucu entegrasyonu
- E-posta/SMS bildirimleri
- Veritabanı yedekleme ekranı

## Personel İzin Yönetimi

- Personel izin talebi oluşturma, kişisel takip, yönetici/İK onay ve red süreci eklendi.
- Tarih aralığı, aktif personel, rol yetkisi ve çakışan izin kontrolleri sunucu tarafında uygulanır.
- Eşzamanlı onay işlemleri RowVersion ile korunur.
- Yıllık izin hakkı ve devreden izin saklanır; kullanılan ve kalan izin onaylı yıllık izinlerden hesaplanır.
- Kalan hakkı aşan yıllık izin talepleri oluşturma ve onay aşamalarında engellenir.
- Onay, red ve iptal işlemleri özet gösteren Bootstrap modallarıyla güvenli biçimde tamamlanır.
- Personel izin kayıtları mevcut Mesai / Vardiya ve Puantaj modüllerinde
  çalışma süresi hesaplarına güvenli biçimde dahil edilir.

## Kasa Kapanışı / Gün Sonu Mutabakatı

- Kasiyerin belirli tarihteki satışları ödeme tipine göre hesaplanır.
- Tamamlanmış iadeler ödeme tipi bulunabildiğinde ilgili tutardan düşülür.
- Sayılan ve beklenen tutarlar arasındaki eksik/fazla farkı kayıt altına alınır.
- Aynı kasiyer ve tarih için ikinci kapanış hem uygulama hem veritabanı
  seviyesinde engellenir.
- Admin, Yonetici ve Muhasebe kapanışları onaylayabilir veya gerekçesiyle
  reddedebilir.
- Kasiyer yalnızca kendi kapanışlarını görür; diğer roller backend yetkisiyle
  sınırlandırılır.
- Eşzamanlı onay/red işlemleri RowVersion ile korunur.

## Personel Mesai ve Vardiya Yönetimi

- Personel vardiya, gerçekleşen çalışma ve fazla mesai süreleri kayıt altına alınır.
- Gece vardiyası ve fazla mesai süreleri sunucu tarafında yeniden hesaplanır.
- Çakışan bekleyen veya onaylı vardiyalar kayıt ve onay aşamasında engellenir.
- Admin, Yonetici ve InsanKaynaklari onay/red sürecini yönetir.
- Muhasebe yalnızca onaylı kayıtları bordro kontrolü amacıyla görüntüler.
- Kasiyer, Personel ve Depo yalnızca kendi taleplerini görür ve bekleyen taleplerini iptal edebilir.
- Bu sürüm maaş veya finans hareketi oluşturmaz; ilerideki bordro modülüne güvenli veri hazırlar.

## Puantaj ve Depo Sipariş Talepleri

- Aylık puantaj raporu aktif personeller için onaylı mesai, fazla mesai ve izin
  kayıtlarından otomatik hesaplanır; ayrı puantaj tablosu oluşturulmaz.
- Aya taşan izinler seçilen ay sınırlarına göre gün bazında kırpılır.
- Depo ürün sipariş talepleri çoklu ürün kalemi, öncelik, onay/red ve iptal
  süreçleriyle kayıt altına alınır.
- Talep ve yönetici onayı stok miktarını değiştirmez.
- Yalnız onaylı talebin teslim alınması ürün stoğunu artırır ve stok giriş
  hareketi oluşturur; işlem transaction ile korunur.
- Depo sipariş süreci satın alma faturası veya finans hareketi oluşturmaz.
- Ürün-tedarikçi bağlantısı çoklu tedarikçi, maliyet, indirim, minimum sipariş,
  teslim süresi ve varsayılan tedarikçi bilgileriyle genişletildi.
- Depo sipariş ekranında ürün bazlı tedarikçi filtreleme ve En Uygun / En Hızlı /
  Varsayılan önerileri eklendi.
- En Uygun Fiyat, En Hızlı Teslimat ve maliyet-hız ağırlıklı Dengeli Seçim
  seçenekleriyle tedarik karar desteği eklendi.
- Ürün kalemlerinde karşılaştırmalı tedarikçi tablosu ve Seçilen etiketi
  gösterilir; kullanıcı öneriyi elle değiştirebilir.
- `023_seed_alternatif_urun_tedarikcileri.sql` ile seçili demo ürünlerine
  hızlı teslimat ve düşük net maliyet odaklı alternatif tedarikçi verileri
  eklendi; mevcut fiyat ve indirim kayıtları korunur.
- Seçilen tedarikçi ve tahmini maliyet bilgileri talep kaleminde saklanır; onay
  stok artırmaz, mevcut teslim alma transaction akışı korunur.
