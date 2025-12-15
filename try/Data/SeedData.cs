using MarkShop.Models;
using MarkShop.Data.ShopSbS.Data;
using System.IO;

namespace MarkShop.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            if (context.Products.Any()) return; // Database already seeded

            // Read the CSV file
            var lines = File.ReadAllLines("pens.csv");

            foreach (var line in lines.Skip(1)) // Skip the header row
            {
                var parts = line.Split(',');

                var product = new Product
                {
                    Name = parts[0],
                    Brand = parts[1],
                    Price = double.Parse(parts[2]),
                    Description = parts[3],
                    ImageUrl = parts[4],
                    Type = Enum.Parse<ProductType>(parts[5]),
                    Color = parts[6],
                    NibSize = parts[7],
                    Style = Enum.Parse<PenStyle>(parts[8]),
                    Usage = Enum.Parse<PenUsage>(parts[9])
                };

                context.Products.Add(product);
            }

            context.SaveChanges();
        }
    }
}