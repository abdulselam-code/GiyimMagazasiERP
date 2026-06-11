# Test Senaryoları

## Giriş ve Rol Testleri

- [ ] `admin` kullanıcısı giriş yapabiliyor.
- [ ] `yonetici1` kullanıcısı giriş yapabiliyor.
- [ ] `muhasebe1` kullanıcısı giriş yapabiliyor.
- [ ] `kasa1` ve `kasa2` kullanıcıları giriş yapabiliyor.
- [ ] `personel1` ve `personel2` kullanıcıları giriş yapabiliyor.
- [ ] `depo1` kullanıcısı giriş yapabiliyor.
- [ ] `ik1` kullanıcısı giriş yapabiliyor.
- [ ] Her kullanıcı rolüne uygun Dashboard ekranını görüyor.
- [ ] Kasiyer finans, stok yönetimi ve teknik panelleri göremiyor.
- [ ] Depo finans ve satış geliri ekranlarını göremiyor.
- [ ] Muhasebe finans, fatura ve rapor ekranlarını görebiliyor.
- [ ] İnsan Kaynakları satış, finans ve stok yönetimini göremiyor.

## Satış Testleri

- [ ] Kasiyer perakende satış oluşturabiliyor.
- [ ] Kasiyer sadece kendi satışlarını ve faturalarını görüyor.
- [ ] Ürün stoğu yetersizse satış tamamlanmıyor.
- [ ] Satış tamamlanınca stok doğru miktarda azalıyor.
- [ ] Satış tamamlanınca finans hareketi oluşuyor.
- [ ] Satır bazlı KDV ve toplam KDV doğru hesaplanıyor.
- [ ] Fatura detay ve yazdırma ekranı açılıyor.

## Toptan Satış Testleri

- [ ] Personel ve Kasiyer toptan satış talebi oluşturabiliyor.
- [ ] Talep oluşturulunca stok, fatura ve finans etkilenmiyor.
- [ ] Yönetici talebi onaylayabiliyor veya reddedebiliyor.
- [ ] Muhasebe onaylanan talebi satışa dönüştürebiliyor.
- [ ] Satışa dönüştürülünce stok, finans ve fatura kayıtları oluşuyor.
- [ ] Aynı talep ikinci kez satışa dönüştürülemiyor.

## İade / Değişim Testleri

- [ ] Kullanıcı kendi satışından iade/değişim talebi oluşturabiliyor.
- [ ] Satılan miktardan fazla iade talep edilemiyor.
- [ ] Yönetici talebi onaylayabiliyor veya reddedebiliyor.
- [ ] Muhasebe onaylanan iadeyi tamamlayabiliyor.
- [ ] Tamamlanan iade stok hareketine yansıyor.
- [ ] Tamamlanan iade finans hareketine yansıyor.
- [ ] İade Belgesi açılıyor ve yazdırılabiliyor.
- [ ] İptal edilmiş veya tamamlanmış talep tekrar işlenemiyor.

## Finans Testleri

- [ ] Gelir, gider ve satış iadesi filtreleri çalışıyor.
- [ ] Satış geliri ilgili satış kaydına bağlı görünüyor.
- [ ] İade hareketi ilgili iade talebiyle izlenebiliyor.
- [ ] Muhasebe toplam gelir, gider ve net durumu görebiliyor.
- [ ] Kasiyer ve Depo finans ekranına erişemiyor.

## Stok Testleri

- [ ] Satış işlemi stok çıkışı oluşturuyor.
- [ ] İade işlemi uygun stok girişini oluşturuyor.
- [ ] Stok düzeltme kayıtları hareket geçmişinde görünüyor.
- [ ] Kritik stok ürünleri Dashboard ve ürün listesinde görünüyor.
- [ ] Sıfır stoklu ürün sayısı kontrol sorgusuyla doğrulanıyor.
- [ ] Her aktif alt kategoride en az iki ürün bulunuyor.

## Dashboard Testleri

- [ ] Admin işletme, finans, stok ve bekleyen onay özetlerini görüyor.
- [ ] Yönetici teknik SQL/DB panellerini görmüyor.
- [ ] Muhasebe gelir, gider, iade ve bekleyen muhasebe onaylarını görüyor.
- [ ] Kasiyer yalnızca kendi satış ve talep özetlerini görüyor.
- [ ] Depo stok ve iade giriş özetlerini görüyor.
- [ ] İnsan Kaynakları yalnızca personel özetlerini görüyor.
- [ ] Özet kartlarının detay bağlantıları doğru sayfaya gidiyor.
- [ ] Grafikler mobil ve masaüstünde taşma yapmıyor.

## Demo Veri Testleri

- [ ] Rol bazlı kullanıcı sayıları kontrol sorgusunda doğru görünüyor.
- [ ] Kullanıcıların PersonelId bağlantıları doğru personeli gösteriyor.
- [ ] `Çocuk/Cocuk`, `Kadın/Kadin` gibi yazım tekrarları kontrol edildi.
- [ ] Çakışan kategori kayıtları silinmeden raporlandı.
- [ ] Her kategori ve alt kategorinin altında ürün bulunuyor.
- [ ] Ürün barkodları benzersiz.
- [ ] Satış fiyatları alış fiyatlarından yüksek.
- [ ] Gereksiz sıfır stoklu, hiç satılmamış ürünler düzeltildi.
- [ ] Birkaç ürün bilinçli olarak kritik stok seviyesinde kaldı.
- [ ] Müşteri listesi Şanlıurfa ağırlıklı görünüyor.
- [ ] Bireysel ve kurumsal demo müşteriler bulunuyor.
- [ ] Müşterilerde eksik telefon, e-posta ve adres kontrol edildi.

## Regresyon Testleri

- [ ] Ürün CRUD işlemleri çalışıyor.
- [ ] Alt kategori ve tedarikçi filtreleri çalışıyor.
- [ ] Satış, fatura, rapor ve Dashboard sayfaları açılıyor.
- [ ] Mevcut satış/fatura/iade kayıtları korunmuş.
- [ ] Rol bazlı AccessDenied davranışı korunmuş.
- [ ] Proje hatasız build alıyor.

## Kasa Kapanışı Testleri

- [ ] `018_create_kasa_kapanislari.sql` ilk ve tekrar çalıştırmada hata vermiyor.
- [ ] Kasiyer yalnızca kendi adına kasa kapanışı oluşturabiliyor.
- [ ] Aynı kasiyer ve tarih için ikinci kapanış engelleniyor.
- [ ] Beklenen nakit, kredi kartı ve havale tutarları günlük satışlardan doğru hesaplanıyor.
- [ ] Tamamlanmış iadeler ilgili ödeme tipinden düşülüyor.
- [ ] Sayılan tutarlar girildiğinde ödeme tipi ve toplam farkları doğru hesaplanıyor.
- [ ] Admin, Yonetici ve Muhasebe hazırlanmış kapanışı onaylayabiliyor.
- [ ] Red nedeni boş bırakıldığında red işlemi engelleniyor.
- [ ] Onaylanan veya reddedilen kapanış tekrar işleme alınamıyor.
- [ ] Kasiyer başka bir kasiyerin kapanış detayına erişemiyor.
- [ ] Personel, Depo ve InsanKaynaklari rolleri modüle erişemiyor.
- [ ] Dashboard ve Hareketler menüsü role göre doğru bağlantıyı gösteriyor.
- [ ] Satış, fatura, finans, iade ve toptan satış akışlarında regresyon oluşmuyor.

## Personel İzinleri Testleri

- [ ] `016_create_personel_izinleri.sql` tekrar çalıştırıldığında hata vermiyor.
- [ ] `017_create_personel_izin_bakiyeleri.sql` tekrar çalıştırıldığında hata vermiyor.
- [ ] Aktif personeller için içinde bulunulan yıl adına varsayılan bakiye oluşuyor.
- [ ] Admin, Yonetici ve InsanKaynaklari tüm izin taleplerini görebiliyor.
- [ ] Kasiyer, Personel, Depo ve Muhasebe yalnızca kendi izinlerini görebiliyor.
- [ ] Normal kullanıcı başka personel adına izin talebi oluşturamıyor.
- [ ] Yetkili kullanıcı aktif personel adına izin talebi oluşturabiliyor.
- [ ] Bitiş tarihi başlangıç tarihinden önce olan talep engelleniyor.
- [ ] Bekleyen veya onaylı izinle çakışan yeni talep engelleniyor.
- [ ] Onay sırasında çakışma kontrolü yeniden çalışıyor.
- [ ] Red nedeni boş bırakıldığında işlem engelleniyor.
- [ ] Yalnızca onay bekleyen talep onaylanabiliyor, reddedilebiliyor veya iptal edilebiliyor.
- [ ] Talep sahibi başka personelin izin detayına erişemiyor.
- [ ] RowVersion çakışmasında kullanıcıya açıklayıcı uyarı gösteriliyor.
- [ ] Dashboard ve menü bağlantıları role göre doğru sayfalara gidiyor.
- [ ] Yıllık izin hakkı, kullanılan izin ve kalan izin kartları doğru görünüyor.
- [ ] Kalan hakkı aşan yıllık izin talebi oluşturma aşamasında engelleniyor.
- [ ] Mazeret ve hastalık izinleri yıllık izin bakiyesinden düşmüyor.
- [ ] Yıllık izin onaylanırken bakiye yeniden kontrol ediliyor.
- [ ] Onay, red ve iptal işlemleri Bootstrap modal üzerinden tamamlanıyor.
- [ ] Red nedeni boşken red modalı açılmıyor ve işlem gönderilmiyor.

## Personel Mesai ve Vardiya Testleri

- [ ] `019_create_personel_mesai_kayitlari.sql` ilk ve tekrar çalıştırmada hata vermiyor.
- [ ] Admin, Yonetici ve InsanKaynaklari tüm mesai kayıtlarını görebiliyor.
- [ ] Kasiyer, Personel ve Depo yalnızca kendi mesai kayıtlarını görebiliyor.
- [ ] Muhasebe yalnızca onaylanmış mesai kayıtlarını ve detaylarını görebiliyor.
- [ ] Normal kullanıcı başka personel adına kayıt oluşturamıyor.
- [ ] Yetkili kullanıcı aktif personel adına kayıt oluşturabiliyor.
- [ ] Aynı başlangıç ve bitiş saati engelleniyor.
- [ ] Gece vardiyası ertesi güne taşınarak doğru hesaplanıyor.
- [ ] Gerçek giriş ve çıkıştan gerçekleşen saat ile fazla mesai doğru hesaplanıyor.
- [ ] Çakışan bekleyen veya onaylı vardiya engelleniyor.
- [ ] Çakışma onay sırasında yeniden kontrol ediliyor.
- [ ] Bekleyen kayıt onaylanabiliyor veya zorunlu red nedeniyle reddedilebiliyor.
- [ ] Talep sahibi bekleyen kendi kaydını iptal edebiliyor.
- [ ] Onaylanan, reddedilen veya iptal edilen kayıt yeniden işleme alınamıyor.
- [ ] RowVersion eşzamanlı işlem çakışmasını güvenli biçimde engelliyor.
- [ ] Dashboard ve menü bağlantıları role göre doğru ekranı açıyor.

## Puantaj Raporu Testleri

- [ ] Admin, Yonetici, InsanKaynaklari ve Muhasebe puantaj ekranını açabiliyor.
- [ ] Kasiyer, Personel ve Depo puantaj ekranına erişemiyor.
- [ ] Ay, yıl, personel arama, departman ve kayıt sayısı filtreleri çalışıyor.
- [ ] Planlanan ve gerçekleşen saatler yalnız onaylı mesailerden hesaplanıyor.
- [ ] Fazla mesai kapsamındaki onaylı kayıtlar doğru saatle rapora yansıyor.
- [ ] Aya taşan izinlerin yalnız seçilen aya düşen günleri hesaplanıyor.
- [ ] Bekleyen mesai sayısı ve durum notu doğru görünüyor.

## Depo Ürün Sipariş Talebi Testleri

- [ ] `021_create_depo_siparis_talepleri.sql` ilk ve tekrar çalıştırmada hata vermiyor.
- [ ] `022_create_urun_tedarikcileri.sql` ilk ve tekrar çalıştırmada hata vermiyor.
- [ ] `023_seed_alternatif_urun_tedarikcileri.sql` ilk ve tekrar çalıştırmada hata vermiyor.
- [ ] `023` sonrasında aktif ürün-tedarikçi bağlantılarında teslim süresi 0 kalmıyor.
- [ ] Seçili demo ürünlerinde iki veya üç aktif tedarikçi bulunuyor.
- [ ] Mevcut ürün kartlarındaki tedarikçiler varsayılan ürün-tedarikçi bağlantısına dönüşüyor.
- [ ] Ürün detayında bağlı tedarikçiler fiyat, indirim, net maliyet ve teslim süresiyle görünüyor.
- [ ] Kadın Hırka ürününe birden fazla tedarikçi eklenip karşılaştırılabiliyor.
- [ ] En Uygun, En Hızlı ve Varsayılan rozetleri doğru bağlantıda görünüyor.
- [ ] Depo, Admin ve Yonetici en az bir ürün kalemiyle talep oluşturabiliyor.
- [ ] Ürün seçildiğinde tedarikçi listesi yalnız o ürüne bağlı aktif tedarikçileri gösteriyor.
- [ ] Tedarikçi seçeneğinde net maliyet, indirim ve teslim süresi görünüyor.
- [ ] En Uygun Fiyat tercihinde net maliyeti en düşük tedarikçi seçiliyor.
- [ ] En Hızlı Teslimat tercihinde teslim süresi en kısa tedarikçi seçiliyor.
- [ ] Dengeli Seçim maliyet yüzde 55 ve teslim süresi yüzde 45 ağırlıkla öneri üretiyor.
- [ ] Karşılaştırma tablosundaki En Uygun, En Hızlı, Varsayılan ve Seçilen etiketleri doğru görünüyor.
- [ ] Kullanıcı otomatik önerilen tedarikçiyi elle değiştirebiliyor.
- [ ] Ürüne bağlı olmayan tedarikçi gönderildiğinde backend işlemi engelliyor.
- [ ] Maliyet, indirim ve teslim süresi veritabanındaki bağlantıdan snapshot olarak kaydediliyor.
- [ ] Depo rolü bağlı tedarikçisi olmayan ürün için talep gönderemiyor.
- [ ] Aynı ürün aynı talepte iki kez eklenemiyor.
- [ ] Talep oluşturulduğunda ürün stoğu değişmiyor.
- [ ] Admin ve Yonetici bekleyen talebi onaylayabiliyor veya gerekçesiyle reddedebiliyor.
- [ ] Red nedeni boş bırakıldığında işlem engelleniyor.
- [ ] Onaylanan talep teslim alınmadan stok değişmiyor.
- [ ] Teslim alma işleminde onaylanan adet kadar stok artıyor.
- [ ] Her teslim kalemi için `Giris` stok hareketi oluşuyor.
- [ ] Teslim alma transaction hatasında stok değişiklikleri geri alınıyor.
- [ ] Depo başka kullanıcının talebini görüntüleyemiyor veya işleyemiyor.
- [ ] Muhasebe talepleri yalnız görüntüleyebiliyor.
- [ ] Kasiyer, Personel ve InsanKaynaklari modüle erişemiyor.
- [ ] Satış, kasa kapanışı, iade, finans, mesai ve puantaj ekranlarında regresyon oluşmuyor.
