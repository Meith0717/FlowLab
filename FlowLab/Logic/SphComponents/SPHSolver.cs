// SPHSolver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.ParticleManagement;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Logic.SphComponents
{
    internal static class SPHSolver
    {
        public static void IISPH(List<Particle> _particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings simulationSettings, out int iterations)
        {
            var parallel = true;
            iterations = 0;
            if (_particles.Count <= 0) return;
            var noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            var timeStep = simulationSettings.TimeStep;
            var fluidViscosity = simulationSettings.FluidViscosity;
            var boundaryViscosity = simulationSettings.BoundaryViscosity;
            var gravitation = simulationSettings.Gravitation;
            var omega = simulationSettings.RelaxationCoefficient;
            var minError = simulationSettings.MinError;
            var maxIterations = simulationSettings.MaxIterations;

            var gamma1 = simulationSettings.Gamma1;
            var gamma2 = simulationSettings.Gamma2;
            var gamma3 = simulationSettings.Gamma3;

            Utilitys.ForEach(parallel, _particles, particle =>
            {
                // neighborhood search & reset the accelerations of the particles
                particle.FindNeighbors(spatialHashing, gamma1, SphKernel.CubicSpline, SphKernel.NablaCubicSpline);
                // Compute densities
                SPHComponents.ComputeLocalDensity(particle, gamma2);
                var particleDensity = particle.Density < FluidDensity ? FluidDensity : particle.Density;
                particle.DensityError = 100 * ((particleDensity - FluidDensity) / FluidDensity);
            });

            // compute non-pressure forces
            Utilitys.ForEach(parallel, noBoundaryParticles, particle =>
            {
                SPHComponents.ComputeViscosityAcceleration(h, boundaryViscosity, fluidViscosity, particle);
                particle.GravitationAcceleration = new(0, gravitation);
            });

            // update velocities using non-pressure forces
            Utilitys.ForEach(parallel, noBoundaryParticles, particle => particle.Velocity += timeStep * particle.NonPAcceleration);

            // Compute diagonal matrix elements & source term
            Utilitys.ForEach(parallel, noBoundaryParticles, particle =>
            {
                IISPHComponents.ComputeSourceTerm(timeStep, particle);
                IISPHComponents.ComputeDiagonalElement(particle, timeStep);

                particle.Pressure *= .5f;
                if (float.Abs(particle.AII) > 1e-6)
                    particle.Pressure = omega / particle.AII * particle.St;
            });

            // perform pressure solve using IISPH
            var i = 0;
            while (true)
            {
                // compute pressure accelerations
                Utilitys.ForEach(parallel, noBoundaryParticles, p => SPHComponents.ComputePressureAccelerationWithReflection(p, gamma3));

                Utilitys.ForEach(parallel, noBoundaryParticles, pI =>
                {
                    if (float.Abs(pI.AII) > 1e-6)
                    {
                        // compute aij * pj
                        IISPHComponents.ComputeLaplacian(pI, timeStep);

                        // update pressure values
                        pI.Pressure += omega / pI.AII * (pI.St - pI.Ap);
                    }
                    else
                        pI.Pressure = 0;

                    if (float.IsNaN(pI.Pressure)) throw new System.Exception();

                    // pressure clamping
                    pI.Pressure = float.Max(pI.Pressure, 0);
                    pI.EstimatedDensityError = 100 * ((pI.Ap - pI.St) / FluidDensity);
                });

                // Break condition
                var avgDensityError = noBoundaryParticles.Any() ? noBoundaryParticles.Average(p => float.Max(p.EstimatedDensityError, 0)) : 0;
                if ((avgDensityError <= minError) && (i > 2) || (i >= maxIterations))
                    break;

                // Increment
                i++;
            }
            iterations = i;

            // update velocities using pressure forces
            Utilitys.ForEach(true, _particles, (particle) =>
            {
                // integrate velocity considering pressure forces 
                particle.Velocity += timeStep * particle.PressureAcceleration;

                // integrate position
                spatialHashing.RemoveObject(particle);
                if (particle.IsBoundary)
                    particle.Position += particle.Velocity;
                else
                    particle.Position += timeStep * particle.Velocity;
                spatialHashing.InsertObject(particle);

                particle.Cfl = timeStep * (particle.Velocity.Length() / h);
            });
        }

        public static void SESPH(List<Particle> _particles, SpatialHashing spatialHashing, float h, float FluidDensity, SimulationSettings simulationSettings)
        {
            var timeStep = simulationSettings.TimeStep;
            var fluidViscosity = simulationSettings.FluidViscosity;
            var boundaryViscosity = simulationSettings.BoundaryViscosity;
            var fluidStiffness = simulationSettings.FluidStiffness;
            var gravitation = simulationSettings.Gravitation;
            var gamma1 = simulationSettings.Gamma1;
            var gamma2 = simulationSettings.Gamma2;
            var gamma3 = simulationSettings.Gamma3;

            // Compute density
            Utilitys.ForEach(true, _particles, particle =>
            {
                particle.FindNeighbors(spatialHashing, gamma1, SphKernel.CubicSpline, SphKernel.NablaCubicSpline);
                SPHComponents.ComputeLocalDensity(particle, gamma2);
                particle.Density = particle.Neighbors.Count <= 1 ? FluidDensity : particle.Density;
                particle.DensityError = 100 * ((particle.Density - FluidDensity) / FluidDensity);
            });

            var noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            // Compute pressures
            Utilitys.ForEach(true, noBoundaryParticles, particle => SESPHComponents.ComputeLocalPressure(particle, fluidStiffness));

            // Compute accelerations
            Utilitys.ForEach(true, noBoundaryParticles, fluidParticle =>
            {
                // Non-pressure accelerations
                SPHComponents.ComputeViscosityAcceleration(h, boundaryViscosity, fluidViscosity, fluidParticle);
                // Pressure acceleration
                SPHComponents.ComputePressureAccelerationWithReflection(fluidParticle, gamma3);
                // Gravitational acceleration 
                fluidParticle.GravitationAcceleration = new(0, gravitation);
            });

            // update velocities
            Utilitys.ForEach(true, _particles, (particle) =>
            {
                // integrate velocity considering pressure forces 
                particle.Velocity += timeStep * particle.Acceleration;

                // integrate position
                spatialHashing.RemoveObject(particle);
                if (particle.IsBoundary)
                    particle.Position += particle.Velocity;
                else
                    particle.Position += timeStep * particle.Velocity;
                spatialHashing.InsertObject(particle);

                particle.Cfl = timeStep * (particle.Velocity.Length() / h);

            });

        }
    }
}
