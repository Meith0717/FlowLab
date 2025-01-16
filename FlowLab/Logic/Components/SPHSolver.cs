// SPHSolver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.ParticleManagement;
using System;

namespace FlowLab.Logic.SphComponents
{
    internal static class SPHSolver
    {
        private static SimulationState BaseSph(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings,  Func<FluidDomain, int> pressureSolver, bool boundaryPressureReflection)
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
            var maxCfl = 0f;

            // Compute density
            Utilitys.ForEach(parallel, particles.All, particle =>
            {
                particle.FindNeighbors(spatialHashing, gamma1, Kernels.CubicSpline, Kernels.NablaCubicSpline);
                SPHComponents.ComputeLocalDensity(particle, gamma2);
                var particleDensity = particle.Density < FluidDensity ? FluidDensity : particle.Density;
                particle.DensityError = 100 * ((particleDensity - FluidDensity) / FluidDensity);
                densityErrorSum += particle.DensityError;
            });

            // compute non-pressure forces & update intermediate velocities using non-pressure forces
            Utilitys.ForEach(parallel, particles.Fluid, particle =>
            {
                SPHComponents.ComputeViscosityAcceleration(h, boundaryViscosity, fluidViscosity, particle);
                particle.GravitationAcceleration = new(0, gravitation);
                particle.IntermediateVelocity = particle.Velocity + (timeStep * particle.NonPAcceleration);
            });

            // Compute pressures
            pressureSolver.Invoke(particles);

            // Compute accelerations
            if (boundaryPressureReflection)
                Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAccelerationWithReflection(particle, gamma3));
            else
                Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3));

            // update velocities
            Utilitys.ForEach(false, particles.All, (particle) =>
            {
                if (!particle.IsBoundary)
                    particle.Velocity = particle.IntermediateVelocity + (timeStep * particle.PressureAcceleration);

                spatialHashing.RemoveObject(particle);
                if (particle.IsBoundary)
                    particle.Position += particle.Velocity;
                else
                    particle.Position += timeStep * particle.Velocity;
                spatialHashing.InsertObject(particle);

                particle.Cfl = timeStep * (particle.Velocity.Length() / h);
                maxCfl = float.Max(maxCfl, particle.Cfl);
            });
            return new(0, maxCfl, densityErrorSum / particles.Count);
        }

        public static SimulationState IISPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            return BaseSph(particles, spatialHashing, h, FluidDensity, settings, p =>  IISPHComponents.RelaxedJacobiSolver(p, FluidDensity, settings), false);
        }

        public static SimulationState SESPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            return BaseSph(particles, spatialHashing, h, FluidDensity, settings, p =>
            {
                Utilitys.ForEach(true, particles.Fluid, particle => SESPHComponents.ComputeLocalPressure(particle, settings.FluidStiffness));
                Utilitys.ForEach(true, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, settings.Gravitation));
                return 0;
            }, false);
        }
    }
}
