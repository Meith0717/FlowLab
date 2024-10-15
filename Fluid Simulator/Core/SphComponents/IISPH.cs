using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Fluid_Simulator.Core.SphComponents
{
    internal static class IISPH
    {
        // Eq 49
        public static Vector2 DiagonalElement(Particle particle, List<Particle> neighbors, int particleDiameter, float timeStep)
        {
            var timeStep2 = timeStep * timeStep;
            var massDensity = particle.Mass / (particle.Density * particle.Density);
            return - timeStep2 * Utilitys.Sum(neighbors, (neighbor) =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighbor.Mass * (massDensity * nablaCubicSpline) * nablaCubicSpline;
            });
        }

        // Eq 39
        public static Vector2 SourceTerm(Particle particle, List<Particle> neighbors, int particleDiameter, float timeStep, float fluidDensity)
        {
            var fluidDensity2 = fluidDensity * fluidDensity;
            return (fluidDensity - particle.Density - fluidDensity2) * Utilitys.Sum(neighbors, neighbor =>
            {
               return particle.Mass * (particle.Velocity - neighbor.Velocity) * SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
            });
        }

        // Eq 41
        public static Vector2 PressureAcceleration(Particle particle, List<Particle> neighbors, int particleDiameter)
        {
            var particlePressure = particle.Pressure;
            var particleDensity2 = particle.Density * particle.Density;
            var particlePressureOverDensity2 = particlePressure / particleDensity2;

            return - Utilitys.Sum(neighbors, neighbor =>
            {
                var neighborMass = neighbor.Mass;
                var neighborPressure = neighbor.Pressure;
                var neighborDensity2 = neighbor.Density * neighbor.Density;
                var neighborPressureOverDensity2 = neighborPressure / neighborDensity2;

                return neighborMass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
            });
        }

        // Eq 40
        public static Vector2 Laplacian(Particle particle, List<Particle> neighbors, float timeStep, Dictionary<Particle, Vector2> pressureAccelerations, int particleDiameter)
        {
            var timeStep2 = timeStep * timeStep;
            var particlePressureAcceleration = pressureAccelerations[particle];

            return timeStep2 * Utilitys.Sum(neighbors, neighbor =>
            {
                var neighborMass = neighbor.Mass;
                var neighborPressureAcceleration = pressureAccelerations[neighbor];
                return neighborMass * (particlePressureAcceleration - neighborPressureAcceleration) * SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
            });
        }

        // Eq 48
    }
}
