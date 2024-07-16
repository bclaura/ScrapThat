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
            modelBuilder.Entity<Product>()
                .HasMany(p => p.PriceHistories)
                .WithOne(ph => ph.Product)
                .HasForeignKey(p => p.ProductId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
