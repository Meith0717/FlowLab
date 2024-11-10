using Fluid_Simulator.Core.ParticleManagement;
using System.Collections.Generic;
using System.Linq;

namespace Fluid_Simulator.Core.SphComponents
{
    internal static class SPHSolver
    {
        private const float MaxError = .1f;
        private const int MaxI = 10;

        public static void IISPH(List<Particle> _particles, SpatialHashing spatialHashing, float h, float FluidDensity, float fluidViscosity, float gravitation, float timeSteps)
        {
            var parallel = true;
            if (_particles.Count <= 0) return;
            var noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            // neighborhood search & reset the accelerations of the particles
            Utilitys.ForEach(parallel, _particles, particle => particle.Initialize(spatialHashing, SphKernel.CubicSpline, SphKernel.NablaCubicSpline));

            // Compute densities DONE
            Utilitys.ForEach(parallel, _particles, SPHComponents.ComputeLocalDensity);

            // Compute diagonal matrix elements DONE
            Utilitys.ForEach(parallel, noBoundaryParticles, particle =>  IISPHComponents.ComputeDiagonalElement(particle, timeSteps));

            // compute non-pressure forces Done
            Utilitys.ForEach(parallel, noBoundaryParticles, particle =>
            {
                SPHComponents.ComputeViscosityAcceleration(h, fluidViscosity, particle);
                particle.GravitationAcceleration = new(0, gravitation);
            });

            // update velocities using non-pressure forces Done
            Utilitys.ForEach(parallel, noBoundaryParticles, particle => particle.Velocity += timeSteps * particle.NonPAcceleration);

            // Compute source term Done
            Utilitys.ForEach(parallel, noBoundaryParticles, particle => IISPHComponents.ComputeSourceTerm(timeSteps, particle));

            // perform pressure solve using IISPH
            var errors = new List<float>();
            var i = 0;
            for (; ; )
            {
                // compute pressure accelerations Done
                Utilitys.ForEach(parallel, noBoundaryParticles, IISPHComponents.ComputePressureAcceleration);

                // compute aij * pj Done
                Utilitys.ForEach(parallel, noBoundaryParticles, particle => IISPHComponents.ComputeLaplacian(particle, timeSteps));

                // update pressure values
                Utilitys.ForEach(parallel, noBoundaryParticles, pI =>
                {
                    if (float.Abs(pI.AII) > 0)
                        pI.Pressure += .5f / pI.AII * (pI.St - pI.Ap);
                    else
                        pI.Pressure = 0;

                    if (float.IsNaN(pI.Pressure)) throw new System.Exception();

                    // pressure clamping
                    pI.Pressure = float.Max(pI.Pressure, 0);
                    pI.DensityError = 100 * (float.Max(pI.Ap - pI.St, 0) / FluidDensity);
                });

                // Break condition
                var avgDensityError = noBoundaryParticles.Any() ? noBoundaryParticles.Average(p => p.DensityError) : 0;
                errors.Add(avgDensityError);
                if ((avgDensityError <= MaxError) && (i > 2)) 
                     break;

                i++;
            }

            // update velocities using pressure forces
            foreach (var fluidParticle in noBoundaryParticles)
            {
                // integrate velocity considering pressure forces 
                fluidParticle.Velocity += timeSteps * fluidParticle.PressureAcceleration;

                // integrate position
                spatialHashing.RemoveObject(fluidParticle);
                fluidParticle.Position += timeSteps * fluidParticle.Velocity;
                spatialHashing.InsertObject(fluidParticle);

                fluidParticle.Cfl = timeSteps * (fluidParticle.Velocity.Length() / h);
                // warm start with factor 0.5
                fluidParticle.Pressure *= .5f;
            }
        }

        public static void SESPH(List<Particle> _particles, SpatialHashing spatialHashing, float h, float FluidDensity, float fluidStiffness, float fluidViscosity, float gravitation, float timeSteps)
        {
            Utilitys.ForEach(true, _particles, particle => particle.Initialize(spatialHashing, SphKernel.CubicSpline, SphKernel.NablaCubicSpline));
            var noBoundaryParticles = _particles.Where((p) => !p.IsBoundary);

            // Compute density
            Utilitys.ForEach(true, _particles, particle =>
            {
                SPHComponents.ComputeLocalDensity(particle);
                particle.Density = particle.Neighbors.Count <= 1 ? FluidDensity : particle.Density;
                particle.DensityError = (particle.Density - FluidDensity) / FluidDensity;
            });

            // Compute pressures
            Utilitys.ForEach(true, noBoundaryParticles, particle => SESPHComponents.ComputeLocalPressure(particle, fluidStiffness));

            // Compute accelerations
            Utilitys.ForEach(true, noBoundaryParticles, fluidParticle =>
            {
                // Non-pressure accelerations
                SPHComponents.ComputeViscosityAcceleration(h, fluidViscosity, fluidParticle);
                // Pressure acceleration
                fluidParticle.PressureAcceleration = SPHComponents.ComputePressureAcceleration(fluidParticle);
                // Gravitational acceleration
                fluidParticle.GravitationAcceleration = new(0, gravitation);
            });

            // Update Velocities
            foreach (var fluidParticle in noBoundaryParticles)
            {
                // Update Velocity
                fluidParticle.Velocity += timeSteps * fluidParticle.Acceleration;
                // Update Position
                spatialHashing.RemoveObject(fluidParticle);
                fluidParticle.Position += timeSteps * fluidParticle.Velocity;
                spatialHashing.InsertObject(fluidParticle);

                fluidParticle.Cfl = timeSteps * (fluidParticle.Velocity.Length() / h);
            }
        }
    }
}
