// IISPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Logic.ParticleManagement;
using MonoGame.Extended;
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
            foreach (var neighbor in particle.Neighbors.Where(p => !p.IsBoundary))
                sum1 += (neighbor.Mass * particle.KernelDerivativ(neighbor)).SquaredNorm();

            var sum2 = System.Numerics.Vector2.Zero;
            foreach (var neighbor in particle.Neighbors)
                sum2 += neighbor.Mass * particle.KernelDerivativ(neighbor);

            if (float.IsNaN(sum1))
                Debugger.Break();
            if (float.IsNaN(sum2.X) || float.IsNaN(sum2.Y))
                Debugger.Break();

            sum1 += sum2.SquaredNorm();

            particle.AII = -timeStep / (particle.Density * particle.Density) * sum1;
        }

        private static void ComputeSourceTerm(Particle particle, float timeStep, float fluidDensity)
        {
            var predDensityOfNonPVel = 0f;
            foreach (var neighbor in particle.Neighbors)
            {
                var velDif = particle.IntermediateVelocity - neighbor.IntermediateVelocity;
                predDensityOfNonPVel += (neighbor.Mass * velDif).Dot(particle.KernelDerivativ(neighbor));
                if (float.IsNaN(predDensityOfNonPVel))
                    Debugger.Break();
            }
            var predDensity = particle.Density + (timeStep * predDensityOfNonPVel);
            particle.St = (fluidDensity - predDensity) / timeStep;
        }

        private static void ComputeLaplacian(Particle particle, float timeStep)
        {
            particle.Ap = 0;
            foreach (var neighbor in particle.Neighbors)
            {
                var update = (neighbor.Mass * (particle.Acceleration - neighbor.Acceleration)).Dot(particle.KernelDerivativ(neighbor));
                particle.Ap += update;
                if (float.IsNaN(particle.Ap))
                    Debugger.Break();
            }
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
            var boundaryHandling = settings.BoundaryHandling;
            var gamma3 = settings.Gamma3;

            Utilitys.ForEach(parallel, particles.Fluid, particle =>
            {
                ComputeSourceTerm(particle, timeStep, fluidDensity);
                ComputeDiagonalElement(particle, timeStep);
                particle.Pressure = float.Abs(particle.AII) > 1e-6 ? omega / particle.AII * particle.St : 0;
            });

            var i = 0;
            for (;;)
            {
                if (boundaryHandling == BoundaryHandling.Extrapolation)
                    Utilitys.ForEach(parallel, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, gravitation));

                Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3, boundaryHandling == BoundaryHandling.Mirroring));

                Utilitys.ForEach(parallel, particles.Fluid, particle =>
                {
                    ComputeLaplacian(particle, timeStep);
                    particle.Pressure = float.Abs(particle.AII) > 1e-6 ? particle.Pressure + (omega / particle.AII * (particle.St - particle.Ap)) : 0;
                    particle.Pressure = float.Max(particle.Pressure, 0);
                    if (float.IsNaN(particle.Pressure))
                        Debugger.Break();
                    particle.EstimatedDensityError = (particle.Ap - particle.St) / fluidDensity * 100;
                });

                var estimatedDensityErrorSum = particles.Fluid.AsParallel().Sum(p => float.Max(p.EstimatedDensityError, 0));
                var avgDensityError = estimatedDensityErrorSum / particles.CountFluid;
                if (float.IsNaN(avgDensityError))
                    Debugger.Break();

                i++;
                if ((avgDensityError <= minError) && (i > 2) || (i >= maxIterations))
                    break;
            }
            return i;
        }
    }
}
