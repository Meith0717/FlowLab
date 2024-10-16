using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Fluid_Simulator.Core.SphComponents
{
    internal static class IISPHComponents
    {
        // Eq 49
        public static float ComputeDiagonalElement(Particle particle, float particleDiameter, float timeStep)
        {
            var timeStep2 = timeStep * timeStep;
            var massDensity2 = particle.Mass / (particle.Density * particle.Density);
            return - timeStep2 * Utilitys.Sum(particle.NeighborParticles, (neighbor) =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighbor.Mass * Vector2.Dot((massDensity2 * nablaCubicSpline), nablaCubicSpline);
            });
        }

        // Eq 39
        public static float ComputeSourceTerm(Particle particle, float particleDiameter, float timeStep, float fluidDensity)
        {
            var timeStep2 = timeStep * timeStep;
            var sum = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return particle.Mass * Vector2.Dot(particle.Velocity - neighbor.Velocity, nablaCubicSpline);
            });
            return fluidDensity - particle.Density - (timeStep2 * sum);
        }

        // Eq 41
        public static Vector2 ComputePressureAcceleration(Particle particle, float particleDiameter)
        {
            var particlePressure = particle.Pressure;
            var particleDensity2 = particle.Density * particle.Density;
            var particlePressureOverDensity2 = particlePressure / particleDensity2;

            return - Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var neighborMass = neighbor.Mass;
                var neighborPressure = neighbor.Pressure;
                var neighborDensity2 = neighbor.Density * neighbor.Density;
                var neighborPressureOverDensity2 = neighborPressure / neighborDensity2;

                return neighborMass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
            });
        }

        // Eq 40
        public static float ComputeLaplacian(Particle particle, float timeStep, float particleDiameter)
        {
            var timeStep2 = timeStep * timeStep;
            var particlePressureAcceleration = particle.PressureAcceleration;
            var sum = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var neighborMass = neighbor.Mass;
                var neighborPressureAcceleration = particlePressureAcceleration;
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighborMass * Vector2.Dot(particlePressureAcceleration - neighborPressureAcceleration, nablaCubicSpline);
            });

            return timeStep2 * sum;
        }

        // Eq 48
        public static float UpdatePressure(float pressure, float diagonalElement, float sourceTerm, float laplacian)
        {
            var omegaOverDiagonalElement = .5f / diagonalElement;
            var diff = sourceTerm - laplacian;
            var value = pressure + (omegaOverDiagonalElement * diff);
            return float.Max(value, 0);
        }

        public static float ComputeDensityError(float laplacian, float sourceTerm, float fluidDensity)
            => (laplacian - sourceTerm) / fluidDensity;

        public static float ComputeAverageError(IEnumerable<float> densityErrors)
            => densityErrors.Sum() / densityErrors.Count();
    }
}
