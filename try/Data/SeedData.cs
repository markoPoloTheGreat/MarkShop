using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MarkShop.Models;
using MarkShop.Data.ShopSbS.Data;
using System.IO;

namespace MarkShop.Data
{
    public static class SeedData
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Check if data already exists
            if (context.Products.Any())
            {
                // To force a re-seed, you could uncomment the lines below:
                // context.Products.RemoveRange(context.Products);
                // context.SaveChanges();
                return;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "pens.csv");

            if (!File.Exists(filePath)) return;

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
                    // Using dynamic to manually map because CSV headers might have hidden quotes or spaces
                    var records = csv.GetRecords<dynamic>().ToList();

                    foreach (var row in records)
                    {
                        var dict = (IDictionary<string, object>)row;

                        // We extract by index if header matching fails
                        var values = dict.Values.Select(v => v?.ToString() ?? "").ToList();

                        if (values.Count < 10) continue;

                        var product = new Product
                        {
                            Name = values[0],
                            Brand = values[1],
                            Price = double.TryParse(values[2], out var p) ? p : 0,
                            Description = values[3],
                            ImageUrl = values[4],
                            Type = Enum.TryParse<ProductType>(values[5], true, out var t) ? t : ProductType.Pen,
                            Color = values[6],
                            NibSize = values[7],
                            Style = Enum.TryParse<PenStyle>(values[8], true, out var s) ? s : PenStyle.Modern,
                            Usage = Enum.TryParse<PenUsage>(values[9], true, out var u) ? u : PenUsage.Everyday
                        };

                        context.Products.Add(product);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception)
            {
                // Silently fails if there's a serious parsing error
                // In a real app, you'd log this to a file.
            }
        }
    }
}