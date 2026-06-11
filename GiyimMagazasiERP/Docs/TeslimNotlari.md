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
- Kasa/gün sonu mutabakatı bulunmamaktadır.
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

- Kasa/gün sonu mutabakatı
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
- Gelecek aşamada vardiya, mesai ve puantaj yönetimi eklenebilir.
