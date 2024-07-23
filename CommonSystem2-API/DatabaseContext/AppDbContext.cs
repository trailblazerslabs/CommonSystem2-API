using CommonSystem2_API.Models;
using Microsoft.EntityFrameworkCore;

namespace CommonSystem2_API.DatabaseContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .HasColumnType("char(36)")
                    .IsRequired();

                entity.Property(e => e.IsActive)
                      .HasColumnName("IsActive")
                      .HasColumnType("bit")
                      .HasConversion<bool?>(f => f, t => t ?? false);
            });
        }
    }
}
