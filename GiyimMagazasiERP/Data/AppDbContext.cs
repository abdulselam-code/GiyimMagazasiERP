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
    public DbSet<Tedarikci> Tedarikciler => Set<Tedarikci>();
    public DbSet<Urun> Urunler => Set<Urun>();
    public DbSet<Satis> Satislar => Set<Satis>();
    public DbSet<SatisDetayi> SatisDetaylari => Set<SatisDetayi>();
    public DbSet<StokHareketi> StokHareketleri => Set<StokHareketi>();
    public DbSet<FinansHareketi> FinansHareketleri => Set<FinansHareketi>();
    public DbSet<MagazaBilgileri> MagazaBilgileri { get; set; }
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

            entity.HasOne(x => x.Kategori)
                .WithMany(x => x.Urunler)
                .HasForeignKey(x => x.KategoriId);

            entity.HasOne(x => x.Tedarikci)
                .WithMany(x => x.Urunler)
                .HasForeignKey(x => x.TedarikciId);
        });

        modelBuilder.Entity<Satis>(entity =>
        {
            entity.ToTable("Satislar");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.ToplamTutar).HasPrecision(18, 2);
            entity.Property(x => x.IndirimTutari).HasPrecision(18, 2);
            entity.Property(x => x.NetTutar).HasPrecision(18, 2);
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
    }
}