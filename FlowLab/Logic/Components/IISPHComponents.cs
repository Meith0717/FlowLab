// IISPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Logic.ParticleManagement;
using MonoGame.Extended;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;

namespace FlowLab.Logic.SphComponents
{
    /// <summary>
    /// https://interactivecomputergraphics.github.io/physics-simulation/examples/iisph.html
    /// </summary>
    internal static class IISPHComponents
    {
        private static void ComputeDiagonalElement(Particle particle, float timeStep)
        {
            var sum1 = 0f;
            var sum2 = System.Numerics.Vector2.Zero;
            foreach (var neighbor in particle.Neighbors)
            {
                sum1 += (neighbor.Mass * particle.KernelDerivativ(neighbor)).SquaredNorm();
                sum2 += neighbor.Mass * particle.KernelDerivativ(neighbor);
# if DEBUG
                if (float.IsNaN(sum1)) throw new System.Exception("ComputeDiagonalElement: sum1 is NaN");
                if (float.IsNaN(sum2.X) || float.IsNaN(sum2.Y)) throw new System.Exception("ComputeDiagonalElement: sum2 is NaN");
#endif
            }

            particle.AII = - (timeStep / (particle.Density * particle.Density)) * (sum1 + sum2.SquaredNorm());
#if DEBUG
            if (float.IsNaN(particle.AII)) throw new System.Exception("ComputeDiagonalElement: aii is NaN");
#endif
        }

        private static void ComputeSourceTerm(Particle particle, float timeStep, float fluidDensity)
        {
            var sum = 0f;
            foreach (var neighbor in particle.Neighbors)
            {
                var velDif = particle.IntermediateVelocity - neighbor.IntermediateVelocity;
                sum += neighbor.Mass * velDif.Dot(particle.KernelDerivativ(neighbor));
#if DEBUG
                if (float.IsNaN(sum)) throw new System.Exception("ComputeSourceTerm: predDensityOfNonPVel is NaN");
# endif
            }
            var predDensity = particle.Density + (timeStep * sum);
            particle.St = (fluidDensity - predDensity) / timeStep;
        }

        private static void ComputeLaplacian(Particle particle, float timeStep)
        {
            var sum = 0f;
            foreach (var neighbor in particle.Neighbors)
            {
                var accDif = particle.PressureAcceleration - neighbor.PressureAcceleration;
                sum += neighbor.Mass * accDif.Dot(particle.KernelDerivativ(neighbor));
#if DEBUG
                if (float.IsNaN(sum)) throw new System.Exception("ComputeLaplacian: particle.Ap is NaN");
# endif
            }
            particle.Ap = timeStep * sum;
        }

        public static int RelaxedJacobiSolver(FluidDomain particles, float fluidDensity, SimulationSettings settings)
        {
            var parallel = settings.ParallelProcessing;
            var timeStep = settings.TimeStep;
            var gravitation = settings.Gravitation;
            var omega = settings.RelaxationCoefficient;
            var minCompression = settings.MinError;
            var maxIterations = settings.MaxIterations;
            var boundaryHandling = settings.BoundaryHandling;
            var gamma3 = settings.Gamma3;

            Utilitys.ForEach(parallel, particles.Fluid, particle =>
            {
                ComputeSourceTerm(particle, timeStep, fluidDensity);
                ComputeDiagonalElement(particle, timeStep);
                particle.Pressure = float.Abs(particle.AII) > 1e-6 ? omega / particle.AII * particle.St : 0;
            });

            int i;
            for (i = 1; i < maxIterations; i++)
            {
                if (boundaryHandling == BoundaryHandling.Extrapolation)
                    Utilitys.ForEach(parallel, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, gravitation));
                Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3, boundaryHandling == BoundaryHandling.Mirroring));

                Utilitys.ForEach(parallel, particles.Fluid, particle =>
                {
                    ComputeLaplacian(particle, timeStep);
                    if (float.Abs(particle.AII) > 0)
                        particle.Pressure += omega / particle.AII * (particle.St - particle.Ap);
                    else
                        particle.Pressure = 0;
                    particle.Pressure = float.Max(particle.Pressure, 0);
#if DEBUG
                    if (float.IsNaN(particle.Pressure)) throw new System.Exception("RelaxedJacobiSolver: particle.Pressure is NaN");
#endif
                    particle.EstimatedCompression = -float.Min(particle.St - particle.Ap, 0) * timeStep / fluidDensity * 100;
                });

                var estimatedCompressionSum = particles.Fluid.AsParallel().Sum(p => p.EstimatedCompression);
                var avgCompression = estimatedCompressionSum / particles.CountFluid;
#if DEBUG
                if (float.IsNaN(avgCompression)) throw new System.Exception("RelaxedJacobiSolver: avgDensityError is NaN");
#endif
                if ((avgCompression <= minCompression) && (i > 2)) 
                    return i;
            }
            return i;
        }
    }
}
