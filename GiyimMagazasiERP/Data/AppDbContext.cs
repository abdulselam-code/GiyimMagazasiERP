using GiyimMagazasiERP.Models;
using Microsoft.EntityFrameworkCore;

namespace GiyimMagazasiERP.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Kullanici> Kullanicilar => Set<Kullanici>();
    public DbSet<Personel> Personeller => Set<Personel>();
    public DbSet<Musteri> Musteriler => Set<Musteri>();
    public DbSet<Kategori> Kategoriler => Set<Kategori>();
    public DbSet<AltKategori> AltKategoriler => Set<AltKategori>();
    public DbSet<Tedarikci> Tedarikciler => Set<Tedarikci>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<UrunTedarikci> UrunTedarikcileri => Set<UrunTedarikci>();
    public DbSet<Satis> Satislar => Set<Satis>();
    public DbSet<SatisDetayi> SatisDetaylari => Set<SatisDetayi>();
    public DbSet<StokHareketi> StokHareketleri => Set<StokHareketi>();
    public DbSet<FinansHareketi> FinansHareketleri => Set<FinansHareketi>();
    public DbSet<MagazaBilgileri> MagazaBilgileri { get; set; }

    public DbSet<TedarikciAltKategori> TedarikciAltKategoriler => Set<TedarikciAltKategori>();
    public DbSet<ToptanSatisTalebi> ToptanSatisTalepleri
    => Set<ToptanSatisTalebi>();

    public DbSet<ToptanSatisTalepDetayi> ToptanSatisTalepDetaylari
        => Set<ToptanSatisTalepDetayi>();

    public DbSet<ToptanSatisTalepHareketi> ToptanSatisTalepHareketleri
        => Set<ToptanSatisTalepHareketi>();
    public DbSet<IadeDegisimTalebi> IadeDegisimTalepleri
    => Set<IadeDegisimTalebi>();

    public DbSet<IadeDegisimTalepDetayi> IadeDegisimTalepDetaylari
        => Set<IadeDegisimTalepDetayi>();

    public DbSet<IadeDegisimTalepHareketi> IadeDegisimTalepHareketleri
        => Set<IadeDegisimTalepHareketi>();

    public DbSet<IadeDegisimYeniUrunDetayi> IadeDegisimYeniUrunDetaylari
        => Set<IadeDegisimYeniUrunDetayi>();
    public DbSet<PersonelIzni> PersonelIzinleri => Set<PersonelIzni>();
    public DbSet<PersonelIzinBakiyesi> PersonelIzinBakiyeleri
        => Set<PersonelIzinBakiyesi>();
    public DbSet<KasaKapanisi> KasaKapanislari => Set<KasaKapanisi>();
    public DbSet<PersonelMesaiKaydi> PersonelMesaiKayitlari
        => Set<PersonelMesaiKaydi>();
    public DbSet<DepoSiparisTalebi> DepoSiparisTalepleri => Set<DepoSiparisTalebi>();
    public DbSet<DepoSiparisTalepKalemi> DepoSiparisTalepKalemleri => Set<DepoSiparisTalepKalemi>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.ToTable("Kullanicilar");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.KullaniciAdi).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SifreHash).HasMaxLength(255).IsRequired();
            entity.Property(x => x.Rol).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<Personel>(entity =>
        {
            entity.ToTable("Personeller");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AdSoyad).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Telefon).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(100);
            entity.Property(x => x.Pozisyon).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Departman).HasMaxLength(50).IsRequired();

            entity.Property(x => x.Maas).HasPrecision(18, 2);
            entity.Property(x => x.PrimOrani).HasPrecision(5, 2);
            entity.Property(x => x.MesaiSaati).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Musteri>(entity =>
        {
            entity.ToTable("Musteriler");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AdSoyad).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Telefon).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(100);

            entity.Property(x => x.IndirimOrani).HasPrecision(5, 2);
            entity.Property(x => x.ToplamHarcama).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Kategori>(entity =>
        {
            entity.ToTable("Kategoriler");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.KategoriAdi).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Aciklama).HasMaxLength(250);
        });

        modelBuilder.Entity<AltKategori>(entity =>
        {
            entity.ToTable("AltKategoriler");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.AltKategoriAdi).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Aciklama).HasMaxLength(250);

            entity.HasOne(x => x.Kategori)
                .WithMany(x => x.AltKategoriler)
                .HasForeignKey(x => x.KategoriId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Tedarikci>(entity =>
        {
            entity.ToTable("Tedarikciler");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FirmaAdi).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Telefon).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(100);
            entity.Property(x => x.Adres).HasMaxLength(250);
            entity.Property(x => x.IndirimOrani).HasPrecision(5, 2);
        });

        modelBuilder.Entity<Urun>(entity =>
        {
            entity.ToTable("Urunler");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UrunAdi).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Barkod).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Beden).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Renk).HasMaxLength(50).IsRequired();

            entity.Property(x => x.AlisFiyati).HasPrecision(18, 2);
            entity.Property(x => x.SatisFiyati).HasPrecision(18, 2);
            entity.Property(x => x.KdvOrani).HasPrecision(5, 2);
            entity.HasOne(x => x.Kategori)
                .WithMany(x => x.Urunler)
                .HasForeignKey(x => x.KategoriId);

            entity.HasOne(x => x.AltKategori)
                .WithMany(x => x.Urunler)
                .HasForeignKey(x => x.AltKategoriId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(x => x.Tedarikci)
                .WithMany(x => x.Urunler)
                .HasForeignKey(x => x.TedarikciId);
        });

        modelBuilder.Entity<UrunTedarikci>(entity =>
        {
            entity.ToTable("UrunTedarikcileri");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TedarikciUrunKodu).HasMaxLength(100);
            entity.Property(x => x.BirimMaliyet).HasPrecision(18, 2);
            entity.Property(x => x.IndirimOrani).HasPrecision(5, 2);
            entity.Property(x => x.NetBirimMaliyet).HasPrecision(18, 2);
            entity.Property(x => x.Aciklama).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => new { x.UrunId, x.TedarikciId }).IsUnique();
            entity.HasIndex(x => x.UrunId);
            entity.HasIndex(x => x.TedarikciId);
            entity.HasIndex(x => x.AktifMi);
            entity.HasIndex(x => x.VarsayilanMi);
            entity.HasOne(x => x.Urun)
                .WithMany(x => x.UrunTedarikcileri)
                .HasForeignKey(x => x.UrunId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Tedarikci)
                .WithMany(x => x.UrunTedarikcileri)
                .HasForeignKey(x => x.TedarikciId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Satis>(entity =>
        {
            entity.ToTable("Satislar");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ToplamTutar).HasPrecision(18, 2);
            entity.Property(x => x.IndirimTutari).HasPrecision(18, 2);
            entity.Property(x => x.NetTutar).HasPrecision(18, 2);
            entity.Property(x => x.ToplamKdvTutari).HasPrecision(18, 2);
            entity.Property(x => x.VergiHaricToplam).HasPrecision(18, 2);
            entity.Property(x => x.VergiDahilToplam).HasPrecision(18, 2);
            entity.Property(x => x.OdemeTipi).HasMaxLength(30).IsRequired();

            entity.HasOne(x => x.Musteri)
                .WithMany(x => x.Satislar)
                .HasForeignKey(x => x.MusteriId);

            entity.HasOne(x => x.Personel)
                .WithMany(x => x.Satislar)
                .HasForeignKey(x => x.PersonelId);
        });

        modelBuilder.Entity<SatisDetayi>(entity =>
        {
            entity.ToTable("SatisDetaylari");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BirimFiyat).HasPrecision(18, 2);
            entity.Property(x => x.ToplamTutar).HasPrecision(18, 2);
            entity.Property(x => x.SatirIndirimTutari).HasPrecision(18, 2);
            entity.Property(x => x.KdvOrani).HasPrecision(5, 2);
            entity.Property(x => x.KdvTutari).HasPrecision(18, 2);
            entity.Property(x => x.VergiHaricTutar).HasPrecision(18, 2);
            entity.Property(x => x.VergiDahilTutar).HasPrecision(18, 2);

            entity.Property(x => x.UrunAdiSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.BarkodSnapshot).HasMaxLength(50).IsRequired();
            entity.Property(x => x.BedenSnapshot).HasMaxLength(20).IsRequired();
            entity.Property(x => x.RenkSnapshot).HasMaxLength(50).IsRequired();
            entity.HasOne(x => x.Satis)
                .WithMany(x => x.SatisDetaylari)
                .HasForeignKey(x => x.SatisId);

            entity.HasOne(x => x.Urun)
                .WithMany(x => x.SatisDetaylari)
                .HasForeignKey(x => x.UrunId);
        });

        modelBuilder.Entity<StokHareketi>(entity =>
        {
            entity.ToTable("StokHareketleri");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.HareketTipi).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Aciklama).HasMaxLength(250);

            entity.HasOne(x => x.Urun)
                .WithMany(x => x.StokHareketleri)
                .HasForeignKey(x => x.UrunId);
        });

        modelBuilder.Entity<FinansHareketi>(entity =>
        {
            entity.ToTable("FinansHareketleri");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.HareketTipi).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Kategori).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Tutar).HasPrecision(18, 2);
            entity.Property(x => x.Aciklama).HasMaxLength(250);

            entity.HasOne(x => x.Satis)
                .WithMany(x => x.FinansHareketleri)
                .HasForeignKey(x => x.SatisId);

            entity.HasOne(x => x.Kullanici)
                .WithMany(x => x.FinansHareketleri)
                .HasForeignKey(x => x.KullaniciId);
        });
        modelBuilder.Entity<Kullanici>()
                .HasOne(k => k.Personel)
                .WithMany()
                .HasForeignKey(k => k.PersonelId)
                .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TedarikciAltKategori>(entity =>
        {
            entity.ToTable("TedarikciAltKategoriler");

            entity.HasKey(x => x.Id);

            entity.HasIndex(x => new { x.TedarikciId, x.AltKategoriId })
                .IsUnique();

            entity.Property(x => x.AktifMi)
                .HasDefaultValue(true);

            entity.Property(x => x.OlusturmaTarihi)
                .HasDefaultValueSql("GETDATE()");

            entity.HasOne(x => x.Tedarikci)
                .WithMany(x => x.TedarikciAltKategoriler)
                .HasForeignKey(x => x.TedarikciId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.AltKategori)
                .WithMany(x => x.TedarikciAltKategoriler)
                .HasForeignKey(x => x.AltKategoriId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ToptanSatisTalebi>(entity =>
        {
            entity.ToTable("ToptanSatisTalepleri");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.TalepNo).HasMaxLength(30).IsRequired();
            entity.Property(x => x.OdemeTipi).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Aciklama).HasMaxLength(500);
            entity.Property(x => x.Durum).HasMaxLength(40).IsRequired();
            entity.Property(x => x.RedNedeni).HasMaxLength(500);

            entity.Property(x => x.ToplamTutar).HasPrecision(18, 2);
            entity.Property(x => x.IndirimTutari).HasPrecision(18, 2);
            entity.Property(x => x.NetTutar).HasPrecision(18, 2);
            entity.Property(x => x.ToplamKdvTutari).HasPrecision(18, 2);
            entity.Property(x => x.VergiHaricToplam).HasPrecision(18, 2);
            entity.Property(x => x.VergiDahilToplam).HasPrecision(18, 2);

            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.HasIndex(x => x.TalepNo).IsUnique();

            entity.HasIndex(x => x.SatisId)
                .IsUnique()
                .HasFilter("[SatisId] IS NOT NULL");

            entity.HasOne(x => x.Musteri)
                .WithMany()
                .HasForeignKey(x => x.MusteriId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.TalepEdenPersonel)
                .WithMany()
                .HasForeignKey(x => x.TalepEdenPersonelId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.TalepEdenKullanici)
                .WithMany()
                .HasForeignKey(x => x.TalepEdenKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.YoneticiOnaylayanKullanici)
                .WithMany()
                .HasForeignKey(x => x.YoneticiOnaylayanKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.MuhasebeOnaylayanKullanici)
                .WithMany()
                .HasForeignKey(x => x.MuhasebeOnaylayanKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.ReddedenKullanici)
                .WithMany()
                .HasForeignKey(x => x.ReddedenKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Satis)
                .WithMany()
                .HasForeignKey(x => x.SatisId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ToptanSatisTalepDetayi>(entity =>
        {
            entity.ToTable("ToptanSatisTalepDetaylari");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BirimFiyat).HasPrecision(18, 2);
            entity.Property(x => x.SatirAraToplam).HasPrecision(18, 2);
            entity.Property(x => x.SatirIndirimTutari).HasPrecision(18, 2);
            entity.Property(x => x.KdvOrani).HasPrecision(5, 2);
            entity.Property(x => x.KdvTutari).HasPrecision(18, 2);
            entity.Property(x => x.VergiHaricTutar).HasPrecision(18, 2);
            entity.Property(x => x.VergiDahilTutar).HasPrecision(18, 2);

            entity.Property(x => x.UrunAdiSnapshot).HasMaxLength(150).IsRequired();
            entity.Property(x => x.BarkodSnapshot).HasMaxLength(50).IsRequired();
            entity.Property(x => x.BedenSnapshot).HasMaxLength(20).IsRequired();
            entity.Property(x => x.RenkSnapshot).HasMaxLength(50).IsRequired();

            entity.HasOne(x => x.ToptanSatisTalebi)
                .WithMany(x => x.Detaylar)
                .HasForeignKey(x => x.ToptanSatisTalebiId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Urun)
                .WithMany()
                .HasForeignKey(x => x.UrunId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ToptanSatisTalepHareketi>(entity =>
        {
            entity.ToTable("ToptanSatisTalepHareketleri");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.OncekiDurum).HasMaxLength(40);
            entity.Property(x => x.YeniDurum).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Aciklama).HasMaxLength(500);

            entity.HasOne(x => x.ToptanSatisTalebi)
                .WithMany(x => x.Hareketler)
                .HasForeignKey(x => x.ToptanSatisTalebiId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Kullanici)
                .WithMany()
                .HasForeignKey(x => x.KullaniciId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<IadeDegisimTalebi>(entity =>
        {
    entity.ToTable("IadeDegisimTalepleri");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.TalepNo)
        .HasMaxLength(30)
        .IsRequired();

    entity.Property(x => x.IadeBelgeNo)
        .HasMaxLength(30);

    entity.Property(x => x.IslemTipi)
        .HasMaxLength(20)
        .IsRequired();

    entity.Property(x => x.Durum)
        .HasMaxLength(40)
        .IsRequired();

    entity.Property(x => x.Aciklama)
        .HasMaxLength(500);

    entity.Property(x => x.RedNedeni)
        .HasMaxLength(500);

    entity.Property(x => x.IptalNedeni)
        .HasMaxLength(500);

    entity.Property(x => x.OdemeTipiSnapshot)
        .HasMaxLength(50);

    entity.Property(x => x.ToplamIadeTutari)
        .HasPrecision(18, 2);

    entity.Property(x => x.ToplamKdvTutari)
        .HasPrecision(18, 2);

    entity.Property(x => x.VergiHaricToplam)
        .HasPrecision(18, 2);

    entity.Property(x => x.VergiDahilToplam)
        .HasPrecision(18, 2);

    entity.Property(x => x.RowVersion)
        .IsRowVersion();

    entity.HasIndex(x => x.TalepNo)
        .IsUnique();

    entity.HasIndex(x => x.IadeBelgeNo)
        .IsUnique()
        .HasFilter("[IadeBelgeNo] IS NOT NULL");

    entity.HasIndex(x => x.SatisId);
    entity.HasIndex(x => x.Durum);
    entity.HasIndex(x => x.IslemTipi);
    entity.HasIndex(x => x.TalepTarihi);

    entity.HasOne(x => x.Satis)
        .WithMany()
        .HasForeignKey(x => x.SatisId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.Musteri)
        .WithMany()
        .HasForeignKey(x => x.MusteriId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.TalepEdenKullanici)
        .WithMany()
        .HasForeignKey(x => x.TalepEdenKullaniciId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.TalepEdenPersonel)
        .WithMany()
        .HasForeignKey(x => x.TalepEdenPersonelId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.YoneticiOnaylayanKullanici)
        .WithMany()
        .HasForeignKey(x => x.YoneticiOnaylayanKullaniciId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.MuhasebeOnaylayanKullanici)
        .WithMany()
        .HasForeignKey(x => x.MuhasebeOnaylayanKullaniciId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.ReddedenKullanici)
        .WithMany()
        .HasForeignKey(x => x.ReddedenKullaniciId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.IptalEdenKullanici)
        .WithMany()
        .HasForeignKey(x => x.IptalEdenKullaniciId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.FinansHareketi)
        .WithMany()
        .HasForeignKey(x => x.FinansHareketiId)
        .OnDelete(DeleteBehavior.NoAction);
});

modelBuilder.Entity<IadeDegisimTalepDetayi>(entity =>
{
    entity.ToTable("IadeDegisimTalepDetaylari");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.BirimFiyat)
        .HasPrecision(18, 2);

entity.Property(x => x.KdvOrani)
        .HasPrecision(5, 2);

entity.Property(x => x.SatirIndirimTutari)
        .HasPrecision(18, 2);

entity.Property(x => x.KdvTutari)
        .HasPrecision(18, 2);

entity.Property(x => x.VergiHaricTutar)
        .HasPrecision(18, 2);

entity.Property(x => x.VergiDahilTutar)
        .HasPrecision(18, 2);

entity.Property(x => x.IadeNedeni)
        .HasMaxLength(300);

entity.Property(x => x.UrunDurumu)
        .HasMaxLength(40)
        .IsRequired();

entity.Property(x => x.UrunAdiSnapshot)
        .HasMaxLength(200)
        .IsRequired();

entity.Property(x => x.BarkodSnapshot)
        .HasMaxLength(100);

entity.Property(x => x.BedenSnapshot)
        .HasMaxLength(50);

entity.Property(x => x.RenkSnapshot)
        .HasMaxLength(50);

entity.HasIndex(x => x.IadeDegisimTalebiId);
    entity.HasIndex(x => x.SatisDetayiId);
    entity.HasIndex(x => x.UrunId);

    entity.HasOne(x => x.IadeDegisimTalebi)
        .WithMany(x => x.Detaylar)
        .HasForeignKey(x => x.IadeDegisimTalebiId)
        .OnDelete(DeleteBehavior.NoAction);

entity.HasOne(x => x.SatisDetayi)
        .WithMany()
        .HasForeignKey(x => x.SatisDetayiId)
        .OnDelete(DeleteBehavior.NoAction);

entity.HasOne(x => x.Urun)
        .WithMany()
        .HasForeignKey(x => x.UrunId)
        .OnDelete(DeleteBehavior.NoAction);
});

modelBuilder.Entity<IadeDegisimTalepHareketi>(entity =>
{
    entity.ToTable("IadeDegisimTalepHareketleri");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.OncekiDurum)
        .HasMaxLength(40);

    entity.Property(x => x.YeniDurum)
        .HasMaxLength(40)
        .IsRequired();

    entity.Property(x => x.Aciklama)
        .HasMaxLength(500);

    entity.HasIndex(x => x.IadeDegisimTalebiId);
    entity.HasIndex(x => x.KullaniciId);
    entity.HasIndex(x => x.IslemTarihi);

    entity.HasOne(x => x.IadeDegisimTalebi)
        .WithMany(x => x.Hareketler)
        .HasForeignKey(x => x.IadeDegisimTalebiId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.Kullanici)
        .WithMany()
        .HasForeignKey(x => x.KullaniciId)
        .OnDelete(DeleteBehavior.NoAction);
});

modelBuilder.Entity<IadeDegisimYeniUrunDetayi>(entity =>
{
    entity.ToTable("IadeDegisimYeniUrunDetaylari");

    entity.HasKey(x => x.Id);

    entity.Property(x => x.BirimFiyat)
        .HasPrecision(18, 2);

    entity.Property(x => x.KdvOrani)
        .HasPrecision(5, 2);

    entity.Property(x => x.KdvTutari)
        .HasPrecision(18, 2);

    entity.Property(x => x.VergiHaricTutar)
        .HasPrecision(18, 2);

    entity.Property(x => x.VergiDahilTutar)
        .HasPrecision(18, 2);

    entity.Property(x => x.UrunAdiSnapshot)
        .HasMaxLength(200)
        .IsRequired();

    entity.Property(x => x.BarkodSnapshot)
        .HasMaxLength(100);

    entity.Property(x => x.BedenSnapshot)
        .HasMaxLength(50);

    entity.Property(x => x.RenkSnapshot)
        .HasMaxLength(50);

    entity.HasIndex(x => x.IadeDegisimTalebiId);
    entity.HasIndex(x => x.YeniUrunId);

    entity.HasOne(x => x.IadeDegisimTalebi)
        .WithMany(x => x.YeniUrunDetaylari)
        .HasForeignKey(x => x.IadeDegisimTalebiId)
        .OnDelete(DeleteBehavior.NoAction);

    entity.HasOne(x => x.YeniUrun)
        .WithMany()
        .HasForeignKey(x => x.YeniUrunId)
        .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PersonelIzni>(entity =>
        {
            entity.ToTable("PersonelIzinleri");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IzinTuru)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Aciklama)
                .HasMaxLength(500);

            entity.Property(x => x.Durum)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.RedNedeni)
                .HasMaxLength(500);

            entity.Property(x => x.RowVersion)
                .IsRowVersion();

            entity.HasIndex(x => x.PersonelId);
            entity.HasIndex(x => x.KullaniciId);
            entity.HasIndex(x => x.Durum);
            entity.HasIndex(x => new { x.BaslangicTarihi, x.BitisTarihi });
            entity.HasIndex(x => x.OlusturmaTarihi);

            entity.HasOne(x => x.Personel)
                .WithMany()
                .HasForeignKey(x => x.PersonelId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Kullanici)
                .WithMany()
                .HasForeignKey(x => x.KullaniciId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.OnaylayanKullanici)
                .WithMany()
                .HasForeignKey(x => x.OnaylayanKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PersonelIzinBakiyesi>(entity =>
        {
            entity.ToTable("PersonelIzinBakiyeleri");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.YillikIzinHakki)
                .HasPrecision(5, 2);

            entity.Property(x => x.DevredenIzinGunu)
                .HasPrecision(5, 2);

            entity.Property(x => x.RowVersion)
                .IsRowVersion();

            entity.HasIndex(x => new { x.PersonelId, x.Yil })
                .IsUnique();
            entity.HasIndex(x => x.PersonelId);
            entity.HasIndex(x => x.Yil);

            entity.HasOne(x => x.Personel)
                .WithMany()
                .HasForeignKey(x => x.PersonelId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<KasaKapanisi>(entity =>
        {
            entity.ToTable("KasaKapanislari");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.KapanisNo).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Durum).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Tarih).HasColumnType("date");
            entity.Property(x => x.Aciklama).HasMaxLength(500);
            entity.Property(x => x.RedNedeni).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.Property(x => x.BeklenenNakit).HasPrecision(18, 2);
            entity.Property(x => x.BeklenenKrediKarti).HasPrecision(18, 2);
            entity.Property(x => x.BeklenenHavale).HasPrecision(18, 2);
            entity.Property(x => x.BeklenenToplam).HasPrecision(18, 2);
            entity.Property(x => x.SayilanNakit).HasPrecision(18, 2);
            entity.Property(x => x.SayilanKrediKarti).HasPrecision(18, 2);
            entity.Property(x => x.SayilanHavale).HasPrecision(18, 2);
            entity.Property(x => x.SayilanToplam).HasPrecision(18, 2);
            entity.Property(x => x.FarkNakit).HasPrecision(18, 2);
            entity.Property(x => x.FarkKrediKarti).HasPrecision(18, 2);
            entity.Property(x => x.FarkHavale).HasPrecision(18, 2);
            entity.Property(x => x.FarkToplam).HasPrecision(18, 2);
            entity.Property(x => x.IadeToplami).HasPrecision(18, 2);

            entity.HasIndex(x => x.KapanisNo).IsUnique();
            entity.HasIndex(x => new { x.KasaPersonelId, x.Tarih }).IsUnique();
            entity.HasIndex(x => x.Tarih);
            entity.HasIndex(x => x.KasaPersonelId);
            entity.HasIndex(x => x.KasaKullaniciId);
            entity.HasIndex(x => x.Durum);

            entity.HasOne(x => x.KasaPersonel)
                .WithMany()
                .HasForeignKey(x => x.KasaPersonelId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.KasaKullanici)
                .WithMany()
                .HasForeignKey(x => x.KasaKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.OnaylayanKullanici)
                .WithMany()
                .HasForeignKey(x => x.OnaylayanKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PersonelMesaiKaydi>(entity =>
        {
            entity.ToTable("PersonelMesaiKayitlari");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Tarih).HasColumnType("date");
            entity.Property(x => x.VardiyaBaslangic).HasColumnType("time(0)");
            entity.Property(x => x.VardiyaBitis).HasColumnType("time(0)");
            entity.Property(x => x.GercekGiris).HasColumnType("time(0)");
            entity.Property(x => x.GercekCikis).HasColumnType("time(0)");
            entity.Property(x => x.PlanlananSaat).HasPrecision(5, 2);
            entity.Property(x => x.GerceklesenSaat).HasPrecision(5, 2);
            entity.Property(x => x.FazlaMesaiSaati).HasPrecision(5, 2);
            entity.Property(x => x.MesaiTuru).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Durum).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Aciklama).HasMaxLength(500);
            entity.Property(x => x.RedNedeni).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();

            entity.HasIndex(x => x.PersonelId);
            entity.HasIndex(x => x.KullaniciId);
            entity.HasIndex(x => x.Tarih);
            entity.HasIndex(x => x.Durum);
            entity.HasIndex(x => x.MesaiTuru);
            entity.HasIndex(x => x.OlusturmaTarihi);
            entity.HasIndex(x => new { x.PersonelId, x.Tarih });

            entity.HasOne(x => x.Personel)
                .WithMany()
                .HasForeignKey(x => x.PersonelId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.Kullanici)
                .WithMany()
                .HasForeignKey(x => x.KullaniciId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.OnaylayanKullanici)
                .WithMany()
                .HasForeignKey(x => x.OnaylayanKullaniciId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DepoSiparisTalebi>(entity =>
        {
            entity.ToTable("DepoSiparisTalepleri");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TalepNo).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Durum).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Oncelik).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Aciklama).HasMaxLength(500);
            entity.Property(x => x.RedNedeni).HasMaxLength(500);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasIndex(x => x.TalepNo).IsUnique();
            entity.HasIndex(x => x.Durum);
            entity.HasIndex(x => x.TalepTarihi);
            entity.HasIndex(x => x.TalepEdenKullaniciId);
            entity.HasIndex(x => x.Oncelik);
            entity.HasOne(x => x.TalepEdenKullanici).WithMany().HasForeignKey(x => x.TalepEdenKullaniciId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.TalepEdenPersonel).WithMany().HasForeignKey(x => x.TalepEdenPersonelId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.OnaylayanKullanici).WithMany().HasForeignKey(x => x.OnaylayanKullaniciId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.TeslimAlanKullanici).WithMany().HasForeignKey(x => x.TeslimAlanKullaniciId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<DepoSiparisTalepKalemi>(entity =>
        {
            entity.ToTable("DepoSiparisTalepKalemleri");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TahminiBirimMaliyet).HasPrecision(18, 2);
            entity.Property(x => x.TahminiIndirimOrani).HasPrecision(5, 2);
            entity.Property(x => x.Aciklama).HasMaxLength(300);
            entity.HasIndex(x => new { x.DepoSiparisTalebiId, x.UrunId }).IsUnique();
            entity.HasIndex(x => x.UrunId);
            entity.HasOne(x => x.DepoSiparisTalebi).WithMany(x => x.Kalemler).HasForeignKey(x => x.DepoSiparisTalebiId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Urun).WithMany().HasForeignKey(x => x.UrunId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.Tedarikci).WithMany().HasForeignKey(x => x.TedarikciId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(x => x.UrunTedarikci).WithMany().HasForeignKey(x => x.UrunTedarikciId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
