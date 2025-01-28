// SPHSolver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.ParticleManagement;
using System.Diagnostics;
using System.Linq;

namespace FlowLab.Logic.SphComponents
{
    internal class SPHSolver(Kernels kernels)
    {
        private readonly static Stopwatch neighborSearchStopWatch = new();

        private void ComputeDensity(FluidDomain particles, SpatialHashing spatialHashing, float fluidDensity, bool parallel, float gamma1, float gamma2, out float densityError, out double neighborSearchTime)
        {
            neighborSearchStopWatch.Restart();
            Utilitys.ForEach(parallel, particles.All, particle =>
            {
                particle.FindNeighbors(spatialHashing, gamma1, kernels);
            });
            neighborSearchStopWatch.Stop();
            Utilitys.ForEach(parallel, particles.All, particle =>
            {
                SPHComponents.ComputeLocalDensity(particle, gamma2);
                particle.DensityError = 100 * ((particle.Density - fluidDensity) / fluidDensity);
            });
            densityError = particles.Fluid.AsParallel().Sum(p => float.Max(p.DensityError, 0));
            neighborSearchTime = neighborSearchStopWatch.Elapsed.TotalMilliseconds;
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

        private void UpdateVelocities(FluidDomain particles, bool parallel, float particleSize, float timeStep, out float maxVelocity)
        {
            Utilitys.ForEach(parallel, particles.All, (particle) =>
            {
                if (!particle.IsBoundary)
                    particle.Velocity = particle.IntermediateVelocity + (timeStep * particle.PressureAcceleration);
                particle.Position += timeStep * particle.Velocity;
                particle.Cfl = timeStep * (particle.Velocity.Length() / particleSize);
            });
            maxVelocity = particles.Fluid.AsParallel().Max(p => p.Velocity.Length());
        }

        private readonly static Stopwatch simStepStopWatch = new();

        public SimulationState IISPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            simStepStopWatch.Restart();
            if (particles.CountFluid == 0) return new(0, 0, 0, 0, 0, 0);
            var parallel = settings.ParallelProcessing;
            var timeStep = settings.TimeStep;
            var fluidViscosity = settings.FluidViscosity;
            var boundaryViscosity = settings.BoundaryViscosity;
            var gravitation = settings.Gravitation;
            var gamma1 = settings.Gamma1;
            var gamma2 = settings.Gamma2;

            ComputeDensity(particles, spatialHashing, FluidDensity, parallel, gamma1, gamma2, out var densityErrorSum, out var neighborSearchTime);
            ComputeNonPresAcceleartions(particles, parallel, h, fluidViscosity, boundaryViscosity, gravitation, timeStep);
            var iterations = IISPHComponents.RelaxedJacobiSolver(particles, FluidDensity, settings);
            UpdateVelocities(particles, parallel, h, timeStep, out var maxVelocity);
            spatialHashing.Rearrange(parallel);
            simStepStopWatch.Stop();
            return new(iterations,
                maxVelocity,
                timeStep * (maxVelocity / h),
                densityErrorSum / particles.CountFluid,
                simStepStopWatch.Elapsed.TotalMilliseconds,
                neighborSearchTime);
        }

        public SimulationState SESPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            simStepStopWatch.Restart();
            if (particles.CountFluid == 0) return new(0, 0, 0, 0, 0, 0);
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

            ComputeDensity(particles, spatialHashing, FluidDensity, parallel, gamma1, gamma2, out var densityErrorSum, out var neighborSearchTime);
            ComputeNonPresAcceleartions(particles, parallel, h, fluidViscosity, boundaryViscosity, gravitation, timeStep);

            Utilitys.ForEach(parallel, particles.Fluid, particle => SESPHComponents.StateEquation(particle, fluidStiffness));
            if (boundaryHandling == BoundaryHandling.Extrapolation)
                Utilitys.ForEach(parallel, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, gravitation));

            ComputePressureAcceleration(particles, parallel, gamma3, boundaryHandling == BoundaryHandling.Mirroring);
            UpdateVelocities(particles, parallel, h, timeStep, out var maxVelocity);
            spatialHashing.Rearrange(parallel);
            simStepStopWatch.Stop();
            return new(0,
                maxVelocity,
                timeStep * (maxVelocity / h),
                densityErrorSum / particles.CountFluid,
                simStepStopWatch.Elapsed.TotalMilliseconds,
                neighborSearchTime);
        }
    }
}
