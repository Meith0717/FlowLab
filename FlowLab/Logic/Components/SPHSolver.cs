// SPHSolver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.ParticleManagement;
using System.Linq;

namespace FlowLab.Logic.SphComponents
{
    internal class SPHSolver(Kernels kernels)
    {
        private float ComputeDensity(FluidDomain particles, SpatialHashing spatialHashing, float fluidDensity, bool parallel, float gamma1, float gamma2)
        {
            Utilitys.ForEach(parallel, particles.All, particle =>
            {
                particle.FindNeighbors(spatialHashing, gamma1, kernels);
                SPHComponents.ComputeLocalDensity(particle, gamma2);
                particle.DensityError = 100 * ((particle.Density - fluidDensity) / fluidDensity);
            });
            return particles.Fluid.AsParallel().Sum(p => float.Max(p.DensityError, 0));
        }

        private void ComputeNonPresAcceleartions(FluidDomain particles, bool parallel, float particleSize, float fluidViscosity, float boundaryViscosity, float gravitation, float timeStep)
        {
            Utilitys.ForEach(parallel, particles.Fluid, particle =>
            {
                SPHComponents.ComputeViscosityAcceleration(particleSize, boundaryViscosity, fluidViscosity, particle);
                particle.GravitationAcceleration = new(0, gravitation);
                particle.IntermediateVelocity = particle.Velocity + (timeStep * particle.NonPAcceleration);
            });
        }

        private void ComputePressureAcceleration(FluidDomain particles, bool parallel, float gamma3, bool pressureMirroring)
        {
            Utilitys.ForEach(parallel, particles.Fluid, particle => SPHComponents.ComputePressureAcceleration(particle, gamma3, pressureMirroring));
        }

        private float UpdateVelocities(FluidDomain particles, bool parallel, float particleSize, float timeStep)
        {
            Utilitys.ForEach(parallel, particles.All, (particle) =>
            {
                if (!particle.IsBoundary)
                    particle.Velocity = particle.IntermediateVelocity + (timeStep * particle.PressureAcceleration);
                particle.Position += timeStep * particle.Velocity;
                particle.Cfl = timeStep * (particle.Velocity.Length() / particleSize);
            });
            return particles.Fluid.AsParallel().Max(p => p.Velocity.Length());
        }

        public SimulationState IISPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            var parallel = settings.ParallelProcessing;
            var timeStep = settings.TimeStep;
            var fluidViscosity = settings.FluidViscosity;
            var boundaryViscosity = settings.BoundaryViscosity;
            var gravitation = settings.Gravitation;
            var gamma1 = settings.Gamma1;
            var gamma2 = settings.Gamma2;

            var densityErrorSum = ComputeDensity(particles, spatialHashing, FluidDensity, parallel, gamma1, gamma2);
            ComputeNonPresAcceleartions(particles, parallel, h, fluidViscosity, boundaryViscosity, gravitation, timeStep);
            var iterations = IISPHComponents.RelaxedJacobiSolver(particles, FluidDensity, settings);
            var maxVelocity = UpdateVelocities(particles, parallel, h, timeStep);
            spatialHashing.Rearrange(parallel);
            return new(iterations, maxVelocity, timeStep * (maxVelocity / h), densityErrorSum / particles.CountFluid);
        }

        public SimulationState SESPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            var parallel = settings.ParallelProcessing;
            var timeStep = settings.TimeStep;
            var fluidViscosity = settings.FluidViscosity;
            var boundaryViscosity = settings.BoundaryViscosity;
            var fluidStiffness = settings.FluidStiffness;
            var gravitation = settings.Gravitation;
            var boundaryHandling = settings.BoundaryHandling;
            var gamma1 = settings.Gamma1;
            var gamma2 = settings.Gamma2;
            var gamma3 = settings.Gamma3;

            var densityErrorSum = ComputeDensity(particles, spatialHashing, FluidDensity, parallel, gamma1, gamma2);
            ComputeNonPresAcceleartions(particles, parallel, h, fluidViscosity, boundaryViscosity, gravitation, timeStep);

            Utilitys.ForEach(parallel, particles.Fluid, particle => SESPHComponents.StateEquation(particle, fluidStiffness));
            if (boundaryHandling == BoundaryHandling.Extrapolation)
                Utilitys.ForEach(parallel, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, gravitation));

            ComputePressureAcceleration(particles, parallel, gamma3, boundaryHandling == BoundaryHandling.Mirroring);
            var maxVelocity = UpdateVelocities(particles, parallel, h, timeStep);
            spatialHashing.Rearrange(parallel);
            return new(0, maxVelocity, timeStep * (maxVelocity / h), densityErrorSum / particles.CountFluid);
        }
    }
}
