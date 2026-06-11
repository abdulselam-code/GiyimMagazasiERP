# Giyim Mağazası ERP Kullanıcı Kılavuzu

## Ana Sayfa ve Dashboard

Giriş yaptıktan sonra rolünüze uygun Dashboard açılır. Özet kartları güncel
satış, finans, stok ve bekleyen iş bilgilerini gösterir. Kartlara veya hızlı
işlem düğmelerine tıklayarak ilgili ayrıntı ekranına geçebilirsiniz.

## Satış İşlemi

1. Menüden **Satış Yap** seçeneğini açın.
2. Ürünleri barkod veya ürün listesinden sepete ekleyin.
3. Müşteri, satış türü, ödeme tipi ve indirim bilgilerini kontrol edin.
4. Sepetteki adetleri ve toplamları doğrulayın.
5. Satışı tamamlayın.

Satış tamamlandığında stok düşer, finans hareketi ve fatura kaydı oluşur.

## Fatura Görüntüleme ve Yazdırma

**Hareketler > Satışlar** veya **Faturalar** ekranından ilgili kaydı açın.
Fatura detayında ürünler, KDV, indirim ve ödeme toplamlarını kontrol edin.
**Yazdır** düğmesiyle A4 yazdırma görünümünü açabilirsiniz.

## Toptan Satış Talebi

Kasiyer ve satış personeli toptan satışı doğrudan tamamlamak yerine talep
oluşturur. Müşteri ve ürünler seçildikten sonra talep kaydedilir. Talep
oluşturma aşamasında stok düşmez, fatura veya finans kaydı oluşmaz.

Yönetici talebi değerlendirir. Onaylanan talep muhasebe kontrolünden sonra
satışa dönüştürülür.

## İade İşlemi

İade/değişim ekranından ilgili satış seçilir. İade edilecek ürün, adet, ürün
durumu ve neden bilgileri girilir. Talep önce yönetici değerlendirmesine,
ardından gerekiyorsa muhasebe tamamlama aşamasına gider.

## İade Belgesi

Tamamlanan iadelerde uygulama içi bir İade Belgesi oluşur. Belge
görüntülenebilir ve yazdırılabilir. Bu belge kurum içi operasyon belgesidir;
resmi iade faturası veya GİB belgesi değildir.

## Finans Hareketleri

Muhasebe, Yönetici ve Admin kullanıcıları gelir, gider ve satış iadesi
hareketlerini görüntüleyebilir. Tarih, hareket türü ve kategori filtreleriyle
istenen kayıtlar bulunabilir.

## Stok Takibi

Depo, Yönetici ve Admin kullanıcıları ürün stoğunu, kritik stokları ve stok
hareketlerini takip eder. Satış çıkışları ve tamamlanan iade girişleri otomatik
olarak stok hareketlerinde görünür.

## Müşteri Yönetimi

Yetkili kullanıcılar müşteri listesine yeni kayıt ekleyebilir ve mevcut
iletişim bilgilerini güncelleyebilir. Kurumsal müşterilerde unvan, VKN ve vergi
dairesi bilgileri kullanılabilir.

## Rol Bazlı Yetkiler

- **Admin:** Sistem genelindeki bütün operasyon ve teknik yönetim alanları.
- **Yonetici:** Satış, stok, personel, rapor ve onay işlemleri.
- **Muhasebe:** Finans, fatura, rapor ve muhasebe tamamlama işlemleri.
- **Kasiyer:** Perakende satış ve kendi satış/talep kayıtları.
- **Personel:** Kendi toptan satış ve iade/değişim talepleri.
- **Depo:** Ürün, tedarikçi, stok ve iadeden dönen ürün işlemleri.
- **InsanKaynaklari:** Personel bilgileri ve personel özetleri.

## Demo Hesapları

Demo ortamında şu kullanıcı adları bulunur:

`admin`, `yonetici1`, `muhasebe1`, `kasa1`, `kasa2`, `personel1`,
`personel2`, `depo1`, `ik1`

Demo parola proje teslim notunda belirtilen parola politikasıyla yönetilir.
Gerçek kullanıma geçmeden önce tüm demo hesaplarının parolaları değiştirilmelidir.

## Personel İzinleri

Çalışanlar kullanıcı menüsündeki **Benim İzinlerim** bağlantısından kendi izin
taleplerini görüntüleyebilir ve yeni talep oluşturabilir. Başlangıç tarihi,
bitiş tarihi ve izin türü seçildiğinde talep onay sürecine gönderilir.
Onaylanmadan personel izinli kabul edilmez.

Admin, Yonetici ve InsanKaynaklari rolleri **Personel İzinleri** ekranından tüm
talepleri filtreleyebilir, detaylarını inceleyebilir, onaylayabilir veya red
nedeni belirterek reddedebilir. Talep sahibi yalnızca onay bekleyen kendi
talebini iptal edebilir. Aynı personele ait çakışan bekleyen veya onaylı izin
aralıkları sistem tarafından engellenir.

İzin ekranlarında içinde bulunulan yıla ait yıllık izin hakkı, devreden izin,
kullanılan yıllık izin ve kalan izin bilgileri gösterilir. Yalnızca onaylanmış
**Yıllık İzin** kayıtları bakiyeden düşer; mazeret, hastalık, ücretsiz, doğum ve
diğer izin türleri yıllık izin bakiyesini etkilemez. Kalan hakkı aşan yıllık izin
talebi oluşturulamaz ve bakiye onay sırasında yeniden kontrol edilir.

Onay, red ve iptal işlemleri işlem özeti gösteren Bootstrap pencereleriyle
tamamlanır. Red işlemi için neden yazılması zorunludur.

## Kasa Kapanışı / Gün Sonu Mutabakatı

Kasiyer, **Hareketler > Benim Kasa Kapanışlarım** ekranından **Gün Sonu
Kapanışı Oluştur** seçeneğini açar. Sistem seçilen tarihte kasiyerin yaptığı
satışları ödeme tipine göre toplar ve tamamlanmış iadeleri düşerek beklenen
nakit, kredi kartı ve havale tutarlarını hesaplar. Kasiyer fiziki kasa ve
terminal kayıtlarındaki sayılan tutarları girer.

Fark, `Sayılan - Beklenen` olarak hesaplanır. Sıfır fark mutabakat sağlandığını,
negatif fark kasa eksiğini, pozitif fark ise kasa fazlasını gösterir. Kapanış
kaydedildiğinde **Hazırlandı** durumuna geçer.

Admin, Yonetici ve Muhasebe rolleri **Hareketler > Kasa Kapanışları**
ekranından tüm kapanışları inceler. Hazırlanmış kayıt onaylanabilir veya red
nedeni yazılarak reddedilebilir. Onaylanan ya da reddedilen kapanış tekrar
işleme alınamaz. Kasiyer yalnızca kendi kapanışlarını görüntüleyebilir.

## Personel Mesai ve Vardiya

Kasiyer, Personel ve Depo kullanıcıları **Benim Mesailerim** ekranından kendi
vardiya kayıtlarını görüntüler ve **Fazla Mesai Talebi Oluştur** bağlantısıyla
yalnızca kendi adlarına Fazla Mesai veya Ek Vardiya talebi oluşturur. Vardiya
başlangıç ve bitiş saati gece yarısını aşıyorsa sistem bitişi ertesi gün kabul
eder. Gerçek giriş ve çıkış birlikte girildiğinde gerçekleşen süre ve fazla
mesai otomatik hesaplanır.

Admin, Yonetici ve InsanKaynaklari rolleri **Mesai / Vardiya** ekranından tüm
aktif personeller adına kayıt oluşturabilir, bekleyen kayıtları onaylayabilir
veya gerekçesiyle reddedebilir. Aynı personelin aynı tarihte çakışan bekleyen
veya onaylı vardiyaları hem kayıt hem onay aşamasında engellenir.

Muhasebe rolü **Mesai Kayıtları** ekranında yalnızca onaylanmış kayıtları görür.
Bu ekran bordro ve maaş kontrolüne veri hazırlar; bu aşamada otomatik maaş veya
finans hareketi oluşturmaz.

## Puantaj Raporu

Admin, Yonetici, InsanKaynaklari ve Muhasebe kullanıcıları **Puantaj Raporu**
ekranında ay ve yıl seçerek aktif personellerin onaylı vardiya, gerçekleşen
çalışma, fazla mesai ve izin özetlerini görüntüler. Başka aya taşan izinlerde
yalnız seçilen ay içinde kalan günler hesaplanır. Rapor yeni kayıt üretmez;
Personel Mesai Kayıtları ile Personel İzinlerinden anlık olarak hesaplanır.

## Depo Ürün Sipariş Talepleri

Depo kullanıcısı **Ürün Sipariş Talebi Oluştur** ekranında bir veya daha fazla
aktif ürün, ürüne bağlı tedarikçi, talep adedi ve öncelik seçerek talep
oluşturur. Ürün seçildiğinde mevcut stok, minimum stok ve kritik stok durumu
gösterilir. Tedarikçi listesi yalnız seçilen ürüne ait aktif bağlantılardan
oluşur; net maliyet, indirim ve teslim süresi birlikte gösterilir.

Talep başındaki **Tedarik Tercihi** alanı önerinin nasıl üretileceğini belirler:

- **En Uygun Fiyat:** Net birim maliyeti en düşük tedarikçiyi önerir.
- **En Hızlı Teslimat:** Teslim süresi en kısa tedarikçiyi önerir.
- **Dengeli Seçim:** Net maliyeti yüzde 55, teslim süresini yüzde 45 ağırlıkla
  karşılaştırır ve toplam skoru en iyi tedarikçiyi önerir.

Karşılaştırma tablosunda En Uygun, En Hızlı, Varsayılan ve Seçilen etiketleri
gösterilir. Öneri zorunlu değildir; depocu farklı bir tedarikçiyi elle seçebilir.

Ürün detayındaki **Bağlı Tedarikçiler** bölümünde ürünün satın alınabileceği
firmalar karşılaştırılır. Net birim maliyeti en düşük olan bağlantı **En Uygun**,
teslim süresi en kısa olan bağlantı **En Hızlı**, ürün için ana tercih olarak
işaretlenen bağlantı ise **Varsayılan** rozeti alır. Depo kullanıcısı bu
bilgileri görüntüler; bağlantı ekleme ve düzenleme Admin/Yonetici tarafından
yapılır.

Bağlı tedarikçisi olmayan ürün için Depo rolü talep gönderemez. Admin/Yonetici
eski ürün kartındaki ana tedarikçiyle devam edebilir; kurumsal kullanımda önce
ürün detayından satın alma koşullarının tanımlanması önerilir.

Talep oluşturulması stoğu değiştirmez. Admin veya Yonetici talebi onayladığında kalemlerin
onaylanan adetleri talep adetleri olarak sabitlenir; yine stok artmaz.

Onaylanmış talep Admin, Yonetici veya talebin sahibi Depo kullanıcısı tarafından
teslim alındığında ürün stokları transaction içinde artırılır ve her ürün için
`Giris` türünde stok hareketi oluşturulur. İşlem başarısız olursa tüm stok
değişiklikleri geri alınır. Bu süreç satın alma faturası veya finans hareketi
oluşturmaz.

**Yeni Ürün** işlemi yalnız ürün kartı/master data oluşturur. Stok tamamlamak
için yeni ürün açmak yerine Depo Sipariş Talebi kullanılmalıdır.
