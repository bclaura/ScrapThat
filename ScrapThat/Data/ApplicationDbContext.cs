using Microsoft.EntityFrameworkCore;
using ScrapThat.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ScrapThat.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPriceHistory> ProductPriceHistories { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductPriceHistory>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<ProductPriceHistory>()
                .Property(p => p.ProductId)
                .IsRequired();

            base.OnModelCreating(modelBuilder);
        }
    }
}
