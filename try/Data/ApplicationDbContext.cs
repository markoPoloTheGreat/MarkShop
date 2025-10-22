
using Microsoft.EntityFrameworkCore;
using MarkShop.Models;
namespace MarkShop.Data
{
    namespace ShopSbS.Data
    {
        public class AppDbContext : DbContext
        {
            public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
            {
            }

            // Add DbSet<TEntity> properties here, for example:
            // public DbSet<YourEntity> YourEntities { get; set; }
        }
    }
}
