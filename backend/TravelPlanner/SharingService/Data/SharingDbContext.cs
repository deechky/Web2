using Microsoft.EntityFrameworkCore;
using SharingService.Models;

namespace SharingService.Data
{
    public class SharingDbContext : DbContext
    {
        public SharingDbContext(DbContextOptions<SharingDbContext> options) : base(options) { }

        public DbSet<Share> Shares => Set<Share>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Share>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.Kod).IsUnique();
                entity.Property(s => s.Kod).IsRequired().HasMaxLength(20);
                entity.Property(s => s.Tip).HasConversion<string>().HasMaxLength(10);
                entity.HasIndex(s => s.PlanId);
            });
        }
    }
}
