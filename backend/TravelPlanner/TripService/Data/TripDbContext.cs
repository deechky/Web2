using Microsoft.EntityFrameworkCore;
using TripService.Models;

namespace TripService.Data
{
    public class TripDbContext : DbContext
    {
        public TripDbContext(DbContextOptions<TripDbContext> options) : base(options) { }

        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<Destinacija> Destinacije => Set<Destinacija>();
        public DbSet<Aktivnost> Aktivnosti => Set<Aktivnost>();
        public DbSet<Trosak> Troskovi => Set<Trosak>();
        public DbSet<ChecklistStavka> ChecklistStavke => Set<ChecklistStavka>();
        public DbSet<Beleska> Beleske => Set<Beleska>();
        public DbSet<Podsetnik> Podsetnici => Set<Podsetnik>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Plan>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Naziv).IsRequired().HasMaxLength(200);
                entity.Property(p => p.PlaniraniBudzet).HasColumnType("decimal(18,2)");
                entity.HasIndex(p => p.KorisnikId);

                entity.HasMany(p => p.Destinacije)
                    .WithOne()
                    .HasForeignKey(d => d.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Aktivnosti)
                    .WithOne()
                    .HasForeignKey(a => a.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Troskovi)
                    .WithOne()
                    .HasForeignKey(t => t.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.ChecklistStavke)
                    .WithOne()
                    .HasForeignKey(c => c.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Beleske)
                    .WithOne()
                    .HasForeignKey(b => b.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.Podsetnici)
                    .WithOne()
                    .HasForeignKey(r => r.PlanId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Destinacija>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Naziv).IsRequired().HasMaxLength(200);
                entity.Property(d => d.Lokacija).IsRequired().HasMaxLength(200);
            });

            modelBuilder.Entity<Aktivnost>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Naziv).IsRequired().HasMaxLength(200);
                entity.Property(a => a.ProcenjeniTrosak).HasColumnType("decimal(18,2)");
                entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<Trosak>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Naziv).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Iznos).HasColumnType("decimal(18,2)");
                entity.Property(t => t.Kategorija).HasConversion<string>().HasMaxLength(20);
            });

            modelBuilder.Entity<ChecklistStavka>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Naziv).IsRequired().HasMaxLength(200);
            });

            modelBuilder.Entity<Beleska>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.Naslov).IsRequired().HasMaxLength(200);
                entity.Property(b => b.Sadrzaj).IsRequired().HasMaxLength(5000);
            });

            modelBuilder.Entity<Podsetnik>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Naziv).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Opis).HasMaxLength(2000);
            });
        }
    }
}
