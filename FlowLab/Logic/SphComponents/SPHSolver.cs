// SPHSolver.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Engine;
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

            // neighborhood search & reset the accelerations of the particles
            Utilitys.ForEach(parallel, _particles, particle => particle.Initialize(spatialHashing, SphKernel.CubicSpline, SphKernel.NablaCubicSpline));

            // Compute densities
            Utilitys.ForEach(parallel, _particles, p => SPHComponents.ComputeLocalDensity(p, gamma1));
            Utilitys.ForEach(parallel, noBoundaryParticles, p => p.DensityError = 100 * ((p.Density - FluidDensity) / FluidDensity));

            // Compute diagonal matrix elements
            Utilitys.ForEach(parallel, noBoundaryParticles, particle => IISPHComponents.ComputeDiagonalElement(particle, timeStep));

            // compute non-pressure forces
            Utilitys.ForEach(parallel, noBoundaryParticles, particle =>
            {
                SPHComponents.ComputeViscosityAcceleration(h, boundaryViscosity, fluidViscosity, particle);
                particle.GravitationAcceleration = new(0, gravitation);
            });

            // update velocities using non-pressure forces
            Utilitys.ForEach(parallel, noBoundaryParticles, particle => particle.Velocity += timeStep * particle.NonPAcceleration);

            // Compute source term
            Utilitys.ForEach(parallel, noBoundaryParticles, particle => IISPHComponents.ComputeSourceTerm(timeStep, particle));

            // perform pressure solve using IISPH
            var i = 0;
            for (; ; )
            {
                // compute pressure accelerations
                Utilitys.ForEach(parallel, noBoundaryParticles, p => SPHComponents.ComputePressureAccelerationWithReflection(p, gamma2));

                // compute aij * pj
                Utilitys.ForEach(parallel, noBoundaryParticles, particle => IISPHComponents.ComputeLaplacian(particle, timeStep));

                // update pressure values
                Utilitys.ForEach(parallel, noBoundaryParticles, pI =>
                {
                    if (float.Abs(pI.AII) > 1e-6)
                        pI.Pressure += omega / pI.AII * (pI.St - pI.Ap);
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
            foreach (var fluidParticle in noBoundaryParticles)
            {
                // integrate velocity considering pressure forces 
                fluidParticle.Velocity += timeStep * fluidParticle.PressureAcceleration;

                // integrate position
                spatialHashing.RemoveObject(fluidParticle);
                fluidParticle.Position += timeStep * fluidParticle.Velocity;
                spatialHashing.InsertObject(fluidParticle);

                fluidParticle.Cfl = timeStep * (fluidParticle.Velocity.Length() / h);
                // warm start with factor 0.5
                fluidParticle.Pressure *= .5f;
            }
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

            Utilitys.ForEach(true, _particles, particle => particle.Initialize(spatialHashing, SphKernel.CubicSpline, SphKernel.NablaCubicSpline));
            var noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            // Compute density
            Utilitys.ForEach(true, _particles, particle =>
            {
                SPHComponents.ComputeLocalDensity(particle, gamma1);
                particle.Density = particle.Neighbors.Count <= 1 ? FluidDensity : particle.Density;
                particle.DensityError = 100 * ((particle.Density - FluidDensity) / FluidDensity);
            });

            // Compute pressures
            Utilitys.ForEach(true, noBoundaryParticles, particle => SESPHComponents.ComputeLocalPressure(particle, fluidStiffness));

            // Compute accelerations
            Utilitys.ForEach(true, noBoundaryParticles, fluidParticle =>
            {
                // Non-pressure accelerations
                SPHComponents.ComputeViscosityAcceleration(h, boundaryViscosity, fluidViscosity, fluidParticle);
                // Pressure acceleration
                SPHComponents.ComputePressureAccelerationWithReflection(fluidParticle, gamma2);
                // Gravitational acceleration
                fluidParticle.GravitationAcceleration = new(0, gravitation);
            });

            // Update Velocities
            foreach (var fluidParticle in noBoundaryParticles)
            {
                // Update Velocity
                fluidParticle.Velocity += timeStep * fluidParticle.Acceleration;
                // Update Position
                spatialHashing.RemoveObject(fluidParticle);
                fluidParticle.Position += timeStep * fluidParticle.Velocity;
                spatialHashing.InsertObject(fluidParticle);

                fluidParticle.Cfl = timeStep * (fluidParticle.Velocity.Length() / h);
            }
        }
    }
}
