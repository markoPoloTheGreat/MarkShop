using System.Globalization;
using CsvHelper;
using MarkShop.Models;
using MarkShop.Data.ShopSbS.Data;
using System.IO;

namespace MarkShop.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // 1. Check if already seeded
            if (context.Products.Any()) return;

            // 2. Locate the file
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "pens.csv");
            if (!File.Exists(filePath)) return;

            // 3. Use CsvHelper to parse and save
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // CsvHelper automatically matches headers to Product properties
                var records = csv.GetRecords<Product>().ToList();

                context.Products.AddRange(records);
                context.SaveChanges();
            }
        }
    }
}