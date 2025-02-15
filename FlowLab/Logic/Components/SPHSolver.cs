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
        private readonly Stopwatch _neighborSearchWatch = new();
        private void ComputeDensity(FluidDomain particles, SpatialHashing spatialHashing, float fluidDensity, bool parallel, float gamma1, float gamma2, out float compressionErrorSum, out float absErrorSum, out double neighborSearchTime)
        {
            _neighborSearchWatch.Restart();
            Utilitys.ForEach(parallel, particles.All, particle =>{
                particle.FindNeighbors(spatialHashing, kernels, gamma1, fluidDensity);
            });
            _neighborSearchWatch.Stop();
            neighborSearchTime = _neighborSearchWatch.Elapsed.TotalMilliseconds;
            Utilitys.ForEach(parallel, particles.All, particle =>
            {
                SPHComponents.ComputeLocalDensity(particle, gamma2);
                particle.DensityError = (particle.Density - fluidDensity) / fluidDensity * 100;
            });
            compressionErrorSum = particles.Fluid.AsParallel().Sum(p => float.Max(p.DensityError, 0));
            absErrorSum = particles.Fluid.AsParallel().Sum(p => float.Abs(p.DensityError));
            if (float.IsNaN(compressionErrorSum))
                Debugger.Break();
        }

        private void ComputeNonPressureAccelerations(FluidDomain particles, bool parallel, float particleSize, float fluidViscosity, float boundaryViscosity, float gravitation, float timeStep)
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
                if (!particle.IsBoundary){
                    particle.Velocity = particle.IntermediateVelocity + (timeStep * particle.PressureAcceleration);
                    particle.Position += timeStep * particle.Velocity;
                }
                else
                    particle.Position += particle.Velocity;

                particle.Cfl = timeStep * (particle.Velocity.Length() / particleSize);
            });
            maxVelocity = particles.Fluid.AsParallel().Max(p => p.Velocity.Length());
        }

        private readonly static Stopwatch _totalSolverTimeWatch = new();
        private readonly static Stopwatch _pressureWatch = new();

        public SolverState IISPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings settings)
        {
            _totalSolverTimeWatch.Restart();
            if (particles.CountFluid == 0) return new();
            var parallel = settings.ParallelProcessing;
            var timeStep = settings.TimeStep;
            var fluidViscosity = settings.FluidViscosity;
            var boundaryViscosity = settings.BoundaryViscosity;
            var gravitation = settings.Gravitation;
            var gamma1 = settings.Gamma1;
            var gamma2 = settings.Gamma2;

            ComputeDensity(particles, spatialHashing, FluidDensity, parallel, gamma1, gamma2, out var compressionErrorSum, out var absErrorSum, out var neighborSearchTime);
            ComputeNonPressureAccelerations(particles, parallel, h, fluidViscosity, boundaryViscosity, gravitation, timeStep);

            _pressureWatch.Restart();
            var iterations = IISPHComponents.RelaxedJacobiSolver(particles, FluidDensity, settings);
            _pressureWatch.Stop();

            UpdateVelocities(particles, parallel, h, timeStep, out var maxVelocity);
            spatialHashing.Rearrange(parallel);
            _totalSolverTimeWatch.Stop();
            return new(iterations,
                maxVelocity,
                timeStep * (maxVelocity / h),
                particles.All.AsParallel().Max(p => p.Pressure),
                compressionErrorSum / particles.CountFluid,
                absErrorSum / particles.CountFluid,
                _totalSolverTimeWatch.Elapsed.TotalMilliseconds,
                _pressureWatch.Elapsed.TotalMilliseconds,
                neighborSearchTime);
        }

        public SolverState SESPH(FluidDomain particles, SpatialHashing spatialHashing, float h, float fluidDensity, SimulationSettings settings)
        {
            _totalSolverTimeWatch.Restart();
            if (particles.CountFluid == 0) return new();
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

            ComputeDensity(particles, spatialHashing, fluidDensity, parallel, gamma1, gamma2, out var compressionErrorSum, out var absErrorSum, out var neighborSearchTime);
            ComputeNonPressureAccelerations(particles, parallel, h, fluidViscosity, boundaryViscosity, gravitation, timeStep);

            _pressureWatch.Restart();
            Utilitys.ForEach(parallel, particles.Fluid, particle => SESPHComponents.StateEquation(particle, fluidDensity, fluidStiffness));
            if (boundaryHandling == BoundaryHandling.Extrapolation)
                Utilitys.ForEach(parallel, particles.Boundary, particle => SPHComponents.PressureExtrapolation(particle, gravitation));
            _pressureWatch.Stop();

            ComputePressureAcceleration(particles, parallel, gamma3, boundaryHandling == BoundaryHandling.Mirroring);
            UpdateVelocities(particles, parallel, h, timeStep, out var maxVelocity);
            spatialHashing.Rearrange(parallel);
            _totalSolverTimeWatch.Stop();
            return new(0,
                maxVelocity,
                timeStep * (maxVelocity / h),
                particles.All.AsParallel().Max(p => p.Pressure),
                compressionErrorSum / particles.CountFluid,
                absErrorSum / particles.CountFluid,
                _totalSolverTimeWatch.Elapsed.TotalMilliseconds,
                _pressureWatch.Elapsed.TotalMilliseconds,
                neighborSearchTime);
        }
    }
}
