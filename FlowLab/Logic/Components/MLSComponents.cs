﻿// MLSComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace FlowLab.Logic.Components
{
    internal static class MLSComponents
    {
        public static void PressureExtrapolation(Particle bParticle)
        {
            var parameters = SolveHyperplaneParameters(bParticle);
            var position = Vector<float>.Build.DenseOfArray([1, bParticle.Position.X, bParticle.Position.Y]);
            var result = position * parameters;
            bParticle.Pressure = result;
        }

        private static Vector<float> SolveHyperplaneParameters(Particle particle)
        {
            var sumMatrix = Matrix<float>.Build.Dense(3, 3, 0);
            foreach (var neighbor in particle.FluidNeighbors)
            {
                var x = neighbor.Position.X;
                var y = neighbor.Position.Y;
                var matrix = Matrix<float>.Build.DenseOfArray(new float[,]
                {
                    { 1, x, y },
                    { x, x * x, x * y },
                    { y, x * y, y * y }
                });
                var weight = neighbor.Mass / neighbor.Density * particle.Kernel(neighbor);
                sumMatrix += matrix * weight;
            }

            var sumVector = Vector<float>.Build.Dense(3, 0);
            foreach (var neighbor in particle.FluidNeighbors)
            {
                var position = Vector<float>.Build.DenseOfArray([1, neighbor.Position.X, neighbor.Position.Y]);
                var weight = neighbor.Mass / neighbor.Density * particle.Kernel(neighbor);
                sumVector += position * neighbor.Pressure * weight;
            }

            // Apply SVD-based safe inversion
            return sumMatrix.SafeMatrixInverse().Multiply(sumVector);
        }


        private static Matrix<float> SafeMatrixInverse(this Matrix<float> A, float threshold = 1e-6f)
        {
            var svd = A.Svd();
            var u = svd.U;        // Left singular vectors
            var sigma = svd.S;    // Singular values
            var v = svd.VT;       // Right singular vectors (transposed)
            var sigmaVecInverse = sigma.Map(s => Math.Abs(s) > threshold ? 1.0f / s : 0.0f);
            var sigmaInverse = Matrix<float>.Build.DenseOfDiagonalVector(sigmaVecInverse);
            return v.Transpose() * sigmaInverse * u.Transpose();
        }
    }
}