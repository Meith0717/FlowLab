// MLSComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Statistics;
using Microsoft.Xna.Framework;
using System;
namespace FlowLab.Logic.Components
{
    internal static class MLSComponents
    {
        public static void PressureExtrapolation(Particle bParticle)
        {
            var parameters = SolveHyperplaneParameters(bParticle);
            var position = Vector<float>.Build.DenseOfArray([1, bParticle.Position.X, bParticle.Position.Y]);
            foreach (var neighbor in bParticle.FluidNeighbors)
            {
                var pos = Vector<float>.Build.DenseOfArray([1, neighbor.Position.X, neighbor.Position.Y]);
                var testPressure = pos * parameters;
                var pressure = neighbor.Pressure;
                // if (pressure > 0) System.Diagnostics.Debugger.Break();
            }
            var result = position * parameters;
            bParticle.Pressure = float.Max(result, bParticle.Pressure);
        }

        private static Vector<float> SolveHyperplaneParameters(Particle particle)
        {
            var sumMatrix = Matrix<float>.Build.DenseDiagonal(3, 3, 0);
            foreach (var neighbor in particle.FluidNeighbors)
            {
                var x = neighbor.Position.X; var y = neighbor.Position.Y;
                var X2 = x * x; var Y2 = y * y; var XY = x * y;
                var matrix = Matrix<float>.Build.DenseOfArray(new float[,]
                {
                    { 1, x, y },
                    { x, X2, XY },
                    { y, XY, Y2 }
                });
                sumMatrix += matrix.Multiply((neighbor.Mass / neighbor.Density) * particle.Kernel(neighbor));
            }

            var sumVector = Vector<float>.Build.Dense(3, 0);
            foreach (var neighbor in particle.FluidNeighbors)
            {
                var position = Vector<float>.Build.DenseOfArray([1, neighbor.Position.X, neighbor.Position.X]);
                sumVector += position.Multiply(neighbor.Pressure * (neighbor.Mass / neighbor.Density) * particle.Kernel(neighbor));
            }
            return sumMatrix.SafeMatrixInverse().Multiply(sumVector);
        }

        private static Matrix<float> SafeMatrixInverse(this Matrix<float> A, float threshold = 1e-6f)
        {
            var svd = A.Svd();
            var U = svd.U;
            var Sigma = svd.S;
            var V = svd.VT;
            var SigmaInverse = CreateSigmaInverse(Sigma, threshold);
            return V.Transpose() * SigmaInverse * U.Transpose();
        }

        private static Matrix<float> CreateSigmaInverse(Vector<float> sigma, float threshold)
        {
            var sigmaInverse = sigma.Map(s => Math.Abs(s) < threshold ? 0 : 1.0f / s);
            return Matrix<float>.Build.DenseOfDiagonalVector(sigmaInverse);
        }
    }
}
