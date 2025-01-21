// IISPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Logic.ParticleManagement;
using MonoGame.Extended;
using System.Collections.Generic;
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
            var KernelDerivativ = particle.KernelDerivativ;
            var sum1 = Utilitys.Sum(particle.Neighbors.Where(p => !p.IsBoundary), n => (n.Mass * KernelDerivativ(n)).SquaredNorm());
            var sum2 = Utilitys.Sum(particle.Neighbors, n => n.Mass * KernelDerivativ(n));
            sum1 += sum2.SquaredNorm();

            particle.AII = -timeStep / (particle.Density * particle.Density) * sum1;
            if (float.IsNaN(particle.AII)) throw new System.Exception();
        }

        private static void ComputeSourceTerm(float timeStep, Particle particle)
        {
            var KernelDerivativ = particle.KernelDerivativ;

            var predDensityOfNonPVel = Utilitys.Sum(particle.Neighbors, neighbor =>
            {
                var velDif = particle.IntermediateVelocity - neighbor.IntermediateVelocity;
                return (neighbor.Mass * velDif).Dot(KernelDerivativ(neighbor));
            });
            var predDensity = particle.Density + (timeStep * predDensityOfNonPVel);
            particle.St = (particle.Density0 - predDensity) / timeStep;
            if (float.IsNaN(particle.St)) throw new System.Exception();
        }

        private static void ComputeLaplacian(Particle particle, float timeStep)
        {
            var KernelDerivativ = particle.KernelDerivativ;
            particle.Ap = timeStep * Utilitys.Sum(particle.Neighbors, neighbor => (neighbor.Mass * (particle.Acceleration - neighbor.Acceleration)).Dot(KernelDerivativ(neighbor)));
            if (float.IsNaN(particle.Ap)) throw new System.Exception();
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
                        Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3));
                        break;
                    case BoundaryHandling.Mirroring:
                        Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAccelerationWithReflection(particle, gamma3));
                        break;
                    case BoundaryHandling.Extrapolation:
                        Utilitys.ForEach(parallel, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, gravitation));
                        Utilitys.ForEach(parallel, particles.Fluid, p => SPHComponents.ComputePressureAcceleration(p, gamma3));
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

                    if (float.IsNaN(p.Pressure)) throw new System.Exception();

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
