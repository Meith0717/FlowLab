﻿// IISPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Logic.ParticleManagement;
using MonoGame.Extended;
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
            var dii = System.Numerics.Vector2.Zero;
            var dij = 0f;
            foreach (var neighbour in particle.neighbours)
            {
                var massKernel = neighbour.Mass * particle.KernelDerivativ(neighbour);
                dii += massKernel;
                if (!neighbour.IsBoundary) 
                    dij += massKernel.SquaredNorm();
# if DEBUG
                if (float.IsNaN(dij)) throw new System.Exception("ComputeDiagonalElement: sum1 is NaN");
                if (float.IsNaN(dii.X) || float.IsNaN(dii.Y)) throw new System.Exception("ComputeDiagonalElement: sum2 is NaN");
#endif
            }

            particle.AII = - timeStep / (particle.Density * particle.Density) * (dij + dii.SquaredNorm());
#if DEBUG
            if (float.IsNaN(particle.AII)) throw new System.Exception("ComputeDiagonalElement: aii is NaN");
#endif
        }

        private static void ComputeSourceTerm(Particle particle, float timeStep, float fluidDensity)
        {
            var sum = 0f;
            foreach (var neighbour in particle.neighbours)
            {
                var velDif = particle.IntermediateVelocity - neighbour.IntermediateVelocity;
                sum += neighbour.Mass * velDif.Dot(particle.KernelDerivativ(neighbour));
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
            foreach (var neighbour in particle.neighbours)
            {
                var accDif = particle.PressureAcceleration - neighbour.PressureAcceleration;
                sum += neighbour.Mass * accDif.Dot(particle.KernelDerivativ(neighbour));
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
                particle.Pressure = float.Abs(particle.AII) > 1e-6f ? omega  * (particle.St / particle.AII) : 0;
                particle.Pressure = float.Max(particle.Pressure, 0);
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

                    if (float.Abs(particle.AII) > 1e-6f)
                        particle.Pressure += omega * ((particle.St - particle.Ap) / particle.AII);
                    else
                        particle.Pressure = 0;
                    particle.Pressure = float.Max(particle.Pressure, 0);
#if DEBUG
                    if (float.IsNaN(particle.Pressure)) throw new System.Exception("RelaxedJacobiSolver: particle.Pressure is NaN");
#endif
                    particle.EstimatedCompression = float.Max(particle.Ap - particle.St, 0) * timeStep / fluidDensity * 100;
                });

                var estimatedCompressionSum = particles.Fluid.AsParallel().Sum(p => p.EstimatedCompression);
                var avgCompression = estimatedCompressionSum / particles.CountFluid;
#if DEBUG
                if (float.IsNaN(avgCompression)) throw new System.Exception("RelaxedJacobiSolver: avgDensityError is NaN");
#endif
                if ((avgCompression <= minCompression) && (i > 1)) 
                    return i;
            }
            return i;
        }
    }
}
