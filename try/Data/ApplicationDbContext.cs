
using Microsoft.EntityFrameworkCore;
using MarkShop.Models;

    namespace MarkShop.Data
    {
        public class AppDbContext : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Customer> Customers { get; set; }
            public DbSet<ShoppingCart> shoppingCarts { get; set; }
            public DbSet<CartItem> CartItems { get; set; }

            
        
        public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            {

            }

            // Add DbSet<TEntity> properties here, for example:
            // public DbSet<YourEntity> YourEntities { get; set; }
        }
    }
