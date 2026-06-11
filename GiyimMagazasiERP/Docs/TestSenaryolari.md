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
