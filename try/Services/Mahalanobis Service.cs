using System;
using System.Collections.Generic;
using System.Linq;
using MarkShop.Models;

namespace MarkShop.Services
{
    public class MahalanobisService
    {
        // The Precision Matrix (Inverse Covariance)
        private readonly double[,] _precisionMatrix = new double[8, 8] {
            { 2.181, -0.627, 0.400, 0.285, 0.207, 0.237, 0.355, 0.638 },
            { -0.627, 2.768, 0.985, -0.125, -0.211, 0.178, 0.457, 0.083 },
            { 0.400, 0.985, 2.871, -0.428, -0.153, -0.493, -0.236, -0.412 },
            { 0.285, -0.125, -0.428, 6.784, -0.154, -0.245, -0.098, -0.215 },
            { 0.207, -0.211, -0.153, -0.154, 3.425, -0.110, 0.187, -0.124 },
            { 0.237, 0.178, -0.493, -0.245, -0.110, 3.125, -0.214, -0.365 },
            { 0.355, 0.457, -0.236, -0.098, 0.187, -0.214, 2.658, -0.287 },
            { 0.638, 0.083, -0.412, -0.215, -0.124, -0.365, -0.287, 3.189 }
        };

        // Vector Index Reference:
        // [0:Price, 1:Pro, 2:Mod, 3:Prec, 4:Dur, 5:Pres, 6:Flash, 7:Port]

        public double[] GenerateUserVector(QuizViewModel input)
        {
            double[] v = new double[8];

            // --- 1. Visual Aesthetic Mapping ---
            // This maps the "TikTok" image choice to the vector
            switch (input.VisualChoice)
            {
                case "DarkAcademia":
                    // Vintage, Library, Leather. 
                    // High Prestige, Low Modernity, High Price.
                    v[5] += 1.0; // Prestige
                    v[1] += 0.8; // Professionalism
                    v[2] -= 0.9; // Not Modern
                    v[0] += 0.5; // Pricey taste
                    break;

                case "Cyberpunk":
                    // Neon, Tech, Metal.
                    // High Modernity, High Flashiness.
                    v[2] += 1.0; // Modernity
                    v[6] += 1.0; // Flashiness
                    v[3] += 0.5; // Precision (Tech)
                    v[1] -= 0.5; // Less "Business Pro"
                    break;

                case "Minimalist":
                    // Bauhaus, Apple-style, Clean.
                    // High Precision, High Modernity, Low Flash.
                    v[3] += 1.0; // Precision
                    v[2] += 0.8; // Modernity
                    v[6] -= 0.8; // Not Flashy
                    v[0] -= 0.2; // Moderate Price
                    break;

                case "Cottagecore":
                    // Artsy, Nature, Warm.
                    // High Flash (Artistic), Low Modernity, Low Professionalism.
                    v[6] += 0.8; // Flash/Artistic
                    v[2] -= 0.5; // Not Modern
                    v[1] -= 0.8; // Not Corporate
                    v[7] += 0.5; // Portable (Sketching outside)
                    break;
            }

            // --- 2. Scale Metric A (Entropy: Order vs Chaos) ---
            // 1 = Order (Precision), 10 = Chaos (Flashiness)
            double metricA = (input.ScaleMetricA - 5.5) / 4.5;
            v[3] -= metricA * 0.7; // Order = Precision
            v[6] += metricA * 0.8; // Chaos = Flash

            // --- 3. Scale Metric B (Risk: Chill vs Anxiety) ---
            // 1 = Chill (Portability), 10 = Anxiety (Durability)
            double metricB = (input.ScaleMetricB - 5.5) / 4.5;
            v[4] += metricB * 0.9; // Anxiety = Durability
            v[7] -= metricB * 0.6; // Chill = Portability

            // --- 4. Grid Plot (Personality) ---
            // X: Introvert (-1) vs Extrovert (1) -> Affects Flash/Prestige
            // Y: Function (-1) vs Form (1) -> Affects Modernity/Precision
            v[6] += input.GridX * 0.7; // Extrovert = Flash
            v[5] += input.GridX * 0.3; // Extrovert = Prestige
            v[2] += input.GridY * 0.7; // Form = Modernity
            v[3] -= input.GridY * 0.5; // Function = Precision

            // --- 5. Reaction Time (Impulse Control) ---
            if (input.ReactionTimeMs < 2000)
            {
                v[6] += 0.3; // Impulsive = Flashy
            }
            else if (input.ReactionTimeMs > 5000)
            {
                v[3] += 0.3; // Deliberate = Precise
            }

            return v;
        }

        public Product? FindBestMatch(double[] userVector, List<Product> allPens)
        {
            Product? bestPen = null;
            double minDistance = double.MaxValue;

            foreach (var pen in allPens)
            {
                if (string.IsNullOrEmpty(pen.Vector)) continue;
                double[] penVector = ParseVector(pen.Vector);
                double dist = CalculateDistance(userVector, penVector);

                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestPen = pen;
                }
            }
            return bestPen;
        }

        private double[] ParseVector(string vectorStr)
        {
            var clean = vectorStr.Trim('[', ']', '"');
            var parts = clean.Split(',');
            var result = new double[8];
            for (int i = 0; i < 8 && i < parts.Length; i++)
            {
                double.TryParse(parts[i], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out result[i]);
            }
            return result;
        }

        public double CalculateDistance(double[] userVector, double[] penVector)
        {
            if (userVector.Length != 8 || penVector.Length != 8) return double.MaxValue;

            double[] d = new double[8];
            for (int i = 0; i < 8; i++) d[i] = userVector[i] - penVector[i];

            double[] temp = new double[8];
            for (int i = 0; i < 8; i++)
            {
                double sum = 0;
                for (int j = 0; j < 8; j++) sum += d[j] * _precisionMatrix[j, i];
                temp[i] = sum;
            }

            double distanceSquared = 0;
            for (int i = 0; i < 8; i++) distanceSquared += temp[i] * d[i];

            return Math.Sqrt(Math.Abs(distanceSquared));
        }
    }
}