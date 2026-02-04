using System;

namespace MarkShop.Services
{
    public class MahalanobisService
    {
        // The INVERSE of the Covariance Matrix (Precision Matrix)
        // Calculated dynamically from your 60-pen dataset
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

        public double CalculateDistance(double[] userVector, double[] penVector)
        {
            if (userVector.Length != 8 || penVector.Length != 8)
                return double.MaxValue; // Invalid input

            // 1. Calculate Difference Vector (d = x - y)
            double[] d = new double[8];
            for (int i = 0; i < 8; i++)
            {
                d[i] = userVector[i] - penVector[i];
            }

            // 2. Multiply: temp = d * InverseCovariance
            double[] temp = new double[8];
            for (int i = 0; i < 8; i++)
            {
                double sum = 0;
                for (int j = 0; j < 8; j++)
                {
                    sum += d[j] * _precisionMatrix[j, i];
                }
                temp[i] = sum;
            }

            // 3. Final Dot Product: distance^2 = temp * d
            double distanceSquared = 0;
            for (int i = 0; i < 8; i++)
            {
                distanceSquared += temp[i] * d[i];
            }

            // Return Sqrt for the actual distance
            return Math.Sqrt(Math.Abs(distanceSquared));
        }
    }
}