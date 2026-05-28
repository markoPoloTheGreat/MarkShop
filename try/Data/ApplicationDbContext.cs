
using Microsoft.EntityFrameworkCore;
using MarkShop.Models;

namespace MarkShop.Data
{
    namespace ShopSbS.Data
    {
        public class AppDbContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Customer> Customers { get; set; }
            public DbSet<ShoppingCart> shoppingCarts { get; set; }
            public DbSet<CartItem> CartItems { get; set; }
            public DbSet<ProductSupply> ProductSupplies { get; set; }

            public AppDbContext(DbContextOptions<AppDbContext> options)
                    : base(options)
            {
            }

            // Configure the One-to-One relationship explicitly
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Product>()
                    .HasOne(p => p.Supply)
                    .WithOne(s => s.Product)
                    .HasForeignKey<ProductSupply>(s => s.ProductId);
            }
        }
    }
}