// SPHSolver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.ParticleManagement;
using System;
using System.Threading;

namespace FlowLab.Logic.SphComponents
{
    internal class SPHSolver(Kernels kernels)
    {
        private static readonly object _lockObject = new();

        private SimulationState BaseSph(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings,  Func<FluidDomain, int> pressureSolver, bool pressureMirroring)
        {
            var parallel = settings.ParallelProcessing;
            var timeStep = settings.TimeStep;
            var fluidViscosity = settings.FluidViscosity;
            var boundaryViscosity = settings.BoundaryViscosity;
            var fluidStiffness = settings.FluidStiffness;
            var gravitation = settings.Gravitation;
            var gamma1 = settings.Gamma1;
            var gamma2 = settings.Gamma2;
            var gamma3 = settings.Gamma3;

            var densityErrorSum = 0f;
            var maxVelocity = 0f;
            var iterations = 0;

            // Compute density
            Utilitys.ForEach(parallel, particles.All, particle =>
            {
                particle.FindNeighbors(spatialHashing, gamma1, kernels.CubicSpline, kernels.NablaCubicSpline);
                SPHComponents.ComputeLocalDensity(particle, gamma2);
                particle.DensityError = 100 * ((particle.Density - FluidDensity) / FluidDensity);
                if (particle.IsBoundary) return;
                lock (_lockObject)
                    densityErrorSum += float.Max(particle.DensityError, 0);
            });

            // compute non-pressure forces & update intermediate velocities using non-pressure forces
            Utilitys.ForEach(parallel, particles.Fluid, particle =>
            {
                SPHComponents.ComputeViscosityAcceleration(h, boundaryViscosity, fluidViscosity, particle);
                particle.GravitationAcceleration = new(0, gravitation);
                particle.IntermediateVelocity = particle.Velocity + (timeStep * particle.NonPAcceleration);
            });

            // Compute pressures
            iterations = pressureSolver.Invoke(particles);

            // Compute accelerations
            Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3, pressureMirroring));

            // update velocities
            Utilitys.ForEach(parallel, particles.All, (particle) =>
            {
                if (!particle.IsBoundary)
                    particle.Velocity = particle.IntermediateVelocity + (timeStep * particle.PressureAcceleration);

                particle.Position += timeStep * particle.Velocity;

                particle.Cfl = timeStep * (particle.Velocity.Length() / h);
                lock (_lockObject)
                    maxVelocity = float.Max(maxVelocity, particle.Velocity.Length());
            });
            spatialHashing.Rearrange(false);
            return new(iterations, maxVelocity, timeStep * (maxVelocity / h), densityErrorSum / particles.CountFluid);
        }

        public SimulationState IISPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            return settings.BoundaryHandling switch
            {
                BoundaryHandling.Extrapolation => BaseSph(particles, spatialHashing, h, FluidDensity, settings, p => IISPHComponents.RelaxedJacobiSolver(p, FluidDensity, settings), false),
                BoundaryHandling.Mirroring => BaseSph(particles, spatialHashing, h, FluidDensity, settings, p => IISPHComponents.RelaxedJacobiSolver(p, FluidDensity, settings), true),
                BoundaryHandling.Zero => BaseSph(particles, spatialHashing, h, FluidDensity, settings, p => IISPHComponents.RelaxedJacobiSolver(p, FluidDensity, settings), false),
                _ => throw new NotImplementedException()
            };
        }

        public SimulationState SESPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            return settings.BoundaryHandling switch
            {
                BoundaryHandling.Extrapolation => BaseSph(particles, spatialHashing, h, FluidDensity, settings, p =>
                {
                    Utilitys.ForEach(true, particles.Fluid, particle => SESPHComponents.ComputeLocalPressure(particle, settings.FluidStiffness));
                    Utilitys.ForEach(true, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, settings.Gravitation));
                    return 0;
                }, false),
                BoundaryHandling.Mirroring => BaseSph(particles, spatialHashing, h, FluidDensity, settings, p =>
                {
                    Utilitys.ForEach(true, particles.Fluid, particle => SESPHComponents.ComputeLocalPressure(particle, settings.FluidStiffness));
                    return 0;
                }, true),
                BoundaryHandling.Zero => BaseSph(particles, spatialHashing, h, FluidDensity, settings, p =>
                {
                    Utilitys.ForEach(true, particles.Fluid, particle => SESPHComponents.ComputeLocalPressure(particle, settings.FluidStiffness));
                    return 0;
                }, false),
                _ => throw new NotImplementedException()
            };
        }
    }
}
