using MarkShop.Data.ShopSbS.Data;
using MarkShop.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarkShop.Data
{
    public static class SeedData
    {
        // THIS SIGNATURE MUST MATCH PROGRAM.CS
        public static void Initialize(IServiceProvider serviceProvider)
        {
            // We get the context manually here using the provider
            var context = serviceProvider.GetRequiredService<AppDbContext>();
            if (!context.Customers.Any())
            {
                context.Customers.AddRange(
                    new Customer
                    {
                        Name = "Goblin Admin", // Using your nickname!
                        Email = "admin@com",
                        Password = "admin", // Change this to whatever you want to type to log in
                        Role = "Admin"
                    },
                    new Customer
                    {
                        Name = "Test Student",
                        Email = "mark@com",
                        Password = "123",
                        Role = "Customer"
                    }
                );

                context.SaveChanges();
                Console.WriteLine("--> Seeded default Admin and Customer.");
            }
            // Check if the database is already seeded
            if (context.Products.Any())
            {
                return;   // DB has data, do nothing
            }

            // Look for pens.csv in the root folder
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "pens.csv");

            if (!File.Exists(filePath))
            {
                Console.WriteLine("Seed file pens.csv not found at: " + filePath);
                return;
            }

            var lines = File.ReadAllLines(filePath);
            if (lines.Length <= 1) return; // Empty or just headers

            // REGEX FIX: Splits by comma ONLY if outside quotes
            var splitPattern = @",(?=(?:[^""]*""[^""]*"")*[^""]*$)";

            // Parse headers
            var headers = Regex.Split(lines[0], splitPattern)
                               .Select(h => h.Trim().Trim('"').Replace("\uFEFF", ""))
                               .ToList();

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = Regex.Split(line, splitPattern)
                                .Select(c => c.Trim().Trim('"'))
                                .ToList();

                string GetCol(string name)
                {
                    int idx = headers.IndexOf(name);
                    return (idx >= 0 && idx < cols.Count) ? cols[idx] : null;
                }

                var product = new Product
                {
                    Name = GetCol("Name") ?? "Unknown Pen",
                    Brand = GetCol("Brand") ?? "",
                    Description = GetCol("Description") ?? "",
                    ImageUrl = GetCol("ImageUrl") ?? "",
                    Color = GetCol("Color"),
                    NibSize = GetCol("NibSize"),
                    Vector = GetCol("Vector")
                };

                if (double.TryParse(GetCol("Price"), out double price)) product.Price = price;
                if (Enum.TryParse<ProductType>(GetCol("Type"), true, out var type)) product.Type = type;
                if (Enum.TryParse<PenStyle>(GetCol("Style"), true, out var style)) product.Style = style;
                if (Enum.TryParse<PenUsage>(GetCol("Usage"), true, out var usage)) product.Usage = usage;

                context.Products.Add(product);
            }

            context.SaveChanges();
        }
    }
}