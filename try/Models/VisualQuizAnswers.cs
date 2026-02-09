namespace MarkShop.Models
{
    public class QuizViewModel
    {
        // --- 1. Visual/Aesthetic Choice (The TikTok/Instagram Question) ---
        // Stores the value of the selected image (e.g., "Vintage", "Cyberpunk")
        public string VisualChoice { get; set; } = string.Empty;

        // --- 2. Linear Scales (1-10) ---
        // Generalized so they can be "Entropy", "Risk", "Happiness", etc.
        public int ScaleMetricA { get; set; } // e.g., Order vs Chaos
        public int ScaleMetricB { get; set; } // e.g., Anxiety vs Chill

        // --- 3. Cartesian Plot (-1.0 to 1.0) ---
        // X and Y coordinates from the grid click
        public double GridX { get; set; }
        public double GridY { get; set; }

        // --- 4. Implicit Metrics ---
        // Time taken to complete the form
        public long ReactionTimeMs { get; set; }
    }
}