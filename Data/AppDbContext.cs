using Microsoft.EntityFrameworkCore;
using TunCRM.Models;

namespace TunCRM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Firma> Firmalar { get; set; }
        public DbSet<Firsat> Firsatlar { get; set; }
        public DbSet<Aktivite> Aktiviteler { get; set; }
        public DbSet<Kullanici> Kullanicilar { get; set; }
        public DbSet<Gorev> Gorevler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Firma konfigürasyonu
            modelBuilder.Entity<Firma>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ad).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Adres).HasMaxLength(500);
                entity.Property(e => e.Telefon).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.WebSitesi).HasMaxLength(200);
                entity.Property(e => e.Sehir).HasMaxLength(100);
                entity.Property(e => e.Ilce).HasMaxLength(100);
                entity.Property(e => e.PostaKodu).HasMaxLength(50);
                entity.Property(e => e.Notlar).HasMaxLength(1000);
                entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("datetime('now')");
            });

            // Fırsat konfigürasyonu
            modelBuilder.Entity<Firsat>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ad).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Aciklama).HasMaxLength(1000);
                entity.Property(e => e.Tutar).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Asama).HasConversion<int>();
                entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("datetime('now')");
                
                entity.HasOne(e => e.Firma)
                    .WithMany(f => f.Firsatlar)
                    .HasForeignKey(e => e.FirmaId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                entity.HasOne(e => e.Kullanici)
                    .WithMany(k => k.Firsatlar)
                    .HasForeignKey(e => e.KullaniciId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Aktivite konfigürasyonu
            modelBuilder.Entity<Aktivite>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Baslik).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Aciklama).HasMaxLength(2000);
                entity.Property(e => e.Tip).HasConversion<int>();
                entity.Property(e => e.Tarih).HasDefaultValueSql("datetime('now')");
                entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("datetime('now')");
                
                entity.HasOne(e => e.Firma)
                    .WithMany(f => f.Aktiviteler)
                    .HasForeignKey(e => e.FirmaId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Firsat)
                    .WithMany(f => f.Aktiviteler)
                    .HasForeignKey(e => e.FirsatId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                entity.HasOne(e => e.Kullanici)
                    .WithMany(k => k.Aktiviteler)
                    .HasForeignKey(e => e.KullaniciId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Kullanıcı konfigürasyonu
            modelBuilder.Entity<Kullanici>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ad).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Soyad).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Telefon).HasMaxLength(20);
                entity.Property(e => e.Sifre).HasMaxLength(100);
                entity.Property(e => e.OlusturmaTarihi).HasDefaultValueSql("datetime('now')");
                
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Varsayılan kullanıcı
            modelBuilder.Entity<Kullanici>().HasData(
                new Kullanici
                {
                    Id = 1,
                    Ad = "Admin",
                    Soyad = "User",
                    Email = "admin@tuncrm.com",
                    Telefon = "0555 123 45 67",
                    Aktif = true,
                    OlusturmaTarihi = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}
