using System.ComponentModel.DataAnnotations;
using System.Numerics;


namespace MarkShop.Models
{
    // These Enums help the Matchmaker Logic
    public enum ProductType { Pen, Ink, Paper }
    public enum PenStyle { Classic, Modern, Artistic, Professional }
    public enum PenUsage { Everyday, Calligraphy, Signature, Student }

    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Brand")]
        public string Brand { get; set; } = string.Empty; //Pilot, Lamy, Pelikan

        public double Price { get; set; } = 0.0;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        // --- Technical Specs (For the Shop) ---
        public ProductType Type { get; set; } // Pen, Ink, or Paper

        [Display(Name = "Ink Color")]
        public string? Color { get; set; } // Blue, Black, Shimmering Gold

        [Display(Name = "Nib Size")]
        public string? NibSize { get; set; } // EF (Extra Fine), F, M, B, Stub

        // --- Personality Specs (For the Matchmaker) ---
        public PenStyle? Style { get; set; }  // Matches user vibe
        public PenUsage? Usage { get; set; }  // Matches user needs
        // [Price, Pro, Mod, Prec, Dur, Pres, Flash, Port]
        public string? Vector { get; set; }

        public ProductSupply? Supply { get; set; }
    }
}