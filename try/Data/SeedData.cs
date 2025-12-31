using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MarkShop.Models;
using MarkShop.Data.ShopSbS.Data;
using System.IO;
using System.Linq;

namespace MarkShop.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            // 1. Detect and clear "mangled" data (where everything is stuck in the Name field)
            // If any product has a comma in the name but an empty brand, it's likely mangled.
            if (context.Products.Any(p => p.Name.Contains(",") && (p.Brand == "Generic" || string.IsNullOrEmpty(p.Brand))))
            {
                Console.WriteLine("--> SEEDER: Mangled data detected. Cleaning database for a fresh start...");
                context.Products.RemoveRange(context.Products);
                context.SaveChanges();
            }

            // 2. Skip if we already have clean data
            if (context.Products.Any())
            {
                Console.WriteLine("--> SEEDER: Database already contains products. Skipping.");
                return;
            }

            // 3. Find the file in the output directory
            var filePath = Path.Combine(AppContext.BaseDirectory, "pens.csv");
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"--> SEEDER ERROR: File not found at {filePath}");
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null
            };

            try
            {
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Read();
                    csv.ReadHeader();

                    int count = 0;
                    while (csv.Read())
                    {
                        try
                        {
                            string rawName = csv.GetField(0) ?? "";
                            string[] parts;

                            // LOGIC: If column 1 has a comma and column 2 is empty, 
                            // the CSV was read as one giant string. We split it manually.
                            if (rawName.Contains(",") && string.IsNullOrEmpty(csv.GetField(1)))
                            {
                                parts = rawName.Split(',').Select(p => p.Trim('\"', ' ')).ToArray();
                            }
                            else
                            {
                                // Standard CSV parsing worked
                                parts = new string[10];
                                for (int i = 0; i < 10; i++)
                                {
                                    parts[i] = csv.GetField(i) ?? "";
                                }
                            }

                            if (parts.Length < 2) continue;

                            var product = new Product
                            {
                                Name = parts[0],
                                Brand = parts.Length > 1 ? parts[1] : "Generic",
                                Price = parts.Length > 2 && double.TryParse(parts[2], CultureInfo.InvariantCulture, out var p) ? p : 0.0,
                                Description = parts.Length > 3 ? parts[3] : "",
                                ImageUrl = parts.Length > 4 ? parts[4] : "",
                                Type = parts.Length > 5 && Enum.TryParse<ProductType>(parts[5], true, out var t) ? t : ProductType.Pen,
                                Color = parts.Length > 6 ? parts[6] : "N/A",
                                NibSize = parts.Length > 7 ? parts[7] : "N/A",
                                Style = parts.Length > 8 && Enum.TryParse<PenStyle>(parts[8], true, out var s) ? s : PenStyle.Modern,
                                Usage = parts.Length > 9 && Enum.TryParse<PenUsage>(parts[9], true, out var u) ? u : PenUsage.Everyday
                            };

                            context.Products.Add(product);
                            count++;
                        }
                        catch (Exception rowEx)
                        {
                            Console.WriteLine($"--> SEEDER ROW ERROR: Skipping row. {rowEx.Message}");
                        }
                    }

                    context.SaveChanges();
                    Console.WriteLine($"--> SEEDER SUCCESS: Imported {count} products correctly.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> SEEDER CRITICAL ERROR: {ex.Message}");
            }
        }
    }
}