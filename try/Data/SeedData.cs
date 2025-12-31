using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MarkShop.Models;
using MarkShop.Data.ShopSbS.Data;
using System.IO;
using System.Linq;
using System.Diagnostics;

namespace MarkShop.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // Force DB creation
            context.Database.EnsureCreated();

            // --- DEBUG TIP ---
            // If you want to FORCE the seeder to run even if the table isn't empty,
            // uncomment the two lines below:
            // context.Products.RemoveRange(context.Products);
            // context.SaveChanges();

            if (context.Products.Any())
            {
                Debug.WriteLine("--> SEEDER: Database already has products. Skipping.");
                return;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "pens.csv");
            Debug.WriteLine($"--> SEEDER: Looking for file at: {filePath}");

            if (!File.Exists(filePath))
            {
                Debug.WriteLine("--> SEEDER ERROR: File 'pens.csv' not found. Make sure it is in your project root.");
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                MissingFieldFound = null,
                HeaderValidated = null
            };

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    // Read the header row but ignore it to avoid mapping errors
                    csv.Read();
                    csv.ReadHeader();

                    int count = 0;
                    while (csv.Read())
                    {
                        // We read by INDEX (0, 1, 2...) instead of names to be 100% safe
                        var product = new Product
                        {
                            Name = csv.GetField(0),
                            Brand = csv.GetField(1),
                            Price = double.TryParse(csv.GetField(2), out var p) ? p : 0,
                            Description = csv.GetField(3),
                            ImageUrl = csv.GetField(4),
                            Type = Enum.TryParse<ProductType>(csv.GetField(5), true, out var t) ? t : ProductType.Pen,
                            Color = csv.GetField(6),
                            NibSize = csv.GetField(7),
                            Style = Enum.TryParse<PenStyle>(csv.GetField(8), true, out var s) ? s : PenStyle.Modern,
                            Usage = Enum.TryParse<PenUsage>(csv.GetField(9), true, out var u) ? u : PenUsage.Everyday
                        };

                        context.Products.Add(product);
                        count++;
                    }

                    context.SaveChanges();
                    Debug.WriteLine($"--> SEEDER SUCCESS: Added {count} products.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"--> SEEDER CRITICAL ERROR: {ex.Message}");
            }
        }
    }
}