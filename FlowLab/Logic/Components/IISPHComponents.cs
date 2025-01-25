// IISPHComponents.cs 
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
            var sum1 = 0f;
            foreach (var neighbor in particle.Neighbors.Where(p => !p.IsBoundary))
                sum1 += (neighbor.Mass * particle.KernelDerivativ(neighbor)).SquaredNorm();

            var sum2 = System.Numerics.Vector2.Zero;
            foreach (var neighbor in particle.Neighbors)
                sum2 += neighbor.Mass * particle.KernelDerivativ(neighbor);

            sum1 += sum2.SquaredNorm();

            particle.AII = - timeStep / (particle.Density * particle.Density) * sum1;
        }

        private static void ComputeSourceTerm(float timeStep, Particle particle)
        {
            var predDensityOfNonPVel = 0f;
            foreach (var neighbor in particle.Neighbors)
            {
                var velDif = particle.IntermediateVelocity - neighbor.IntermediateVelocity;
                predDensityOfNonPVel += (neighbor.Mass * velDif).Dot(particle.KernelDerivativ(neighbor));
            }
            var predDensity = particle.Density + (timeStep * predDensityOfNonPVel);
            particle.St = (particle.Density0 - predDensity) / timeStep;
        }

        private static void ComputeLaplacian(Particle particle, float timeStep)
        {
            particle.Ap = 0;
            foreach (var neighbor in particle.Neighbors)
                particle.Ap += (neighbor.Mass * (particle.Acceleration - neighbor.Acceleration)).Dot(particle.KernelDerivativ(neighbor));
            particle.Ap *= timeStep;
        }

        public static int RelaxedJacobiSolver(FluidDomain particles, float fluidDensity, SimulationSettings settings)
        {
            var parallel = settings.ParallelProcessing;
            var timeStep = settings.TimeStep;
            var gravitation = settings.Gravitation;
            var omega = settings.RelaxationCoefficient;
            var minError = settings.MinError;
            var maxIterations = settings.MaxIterations;
            var gamma3 = settings.Gamma3;

            Utilitys.ForEach(parallel, particles.Fluid, particle =>
            {
                ComputeSourceTerm(timeStep, particle);
                ComputeDiagonalElement(particle, timeStep);

                particle.Pressure *= .5f;
                if (float.Abs(particle.AII) > 1e-6)
                    particle.Pressure = omega / particle.AII * particle.St;
            });

            // perform pressure solve using IISPH
            var i = -1;
            while (true)
            {
                var estimatedDensityErrorSum = 0f;
                i++;

                switch (settings.BoundaryHandling)
                {
                    case BoundaryHandling.Zero:
                        Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3, false));
                        break;
                    case BoundaryHandling.Mirroring:
                        Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3, true));
                        break;
                    case BoundaryHandling.Extrapolation:
                        Utilitys.ForEach(parallel, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, gravitation));
                        Utilitys.ForEach(parallel, particles.Fluid, p => SPHComponents.ComputePressureAcceleration(p, gamma3, false));
                        break;
                }

                Utilitys.ForEach(parallel, particles.Fluid, p =>
                {
                    if (float.Abs(p.AII) > 1e-6)
                    {
                        // compute aij * pj
                        ComputeLaplacian(p, timeStep);

                        // update pressure values
                        p.Pressure += omega / p.AII * (p.St - p.Ap);
                    }
                    else
                        p.Pressure = 0;

                    // pressure clamping
                    p.Pressure = float.Max(p.Pressure, 0);
                    p.EstimatedDensityError = 100 * ((p.Ap - p.St) / fluidDensity);
                    estimatedDensityErrorSum += float.Max(p.EstimatedDensityError, 0);
                });

                // Break condition
                var avgDensityError = estimatedDensityErrorSum / particles.CountFluid;
                if ((avgDensityError <= minError) && (i > 2) || (i >= maxIterations))
                    break;
            }
            return i;
        }
    }
}
