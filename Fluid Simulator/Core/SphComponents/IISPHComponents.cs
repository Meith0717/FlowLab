using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Fluid_Simulator.Core.SphComponents
{
    internal static class IISPHComponents
    {
        // Eq 49
        private static float ComputeDiagonalElement(Particle particle, float particleDiameter, float timeStep)
        {

            var sum = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var massOverDensity2 = neighbor.Mass / (neighbor.Density * neighbor.Density);
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return massOverDensity2 * nablaCubicSpline;
            });

            var sum1 = Utilitys.Sum(particle.NeighborParticles, (neighbor) =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighbor.Mass * Vector2.Dot(sum, nablaCubicSpline);
            });

            var massOverDensity2 = particle.Mass / (particle.Density * particle.Density);
            var sum2 = Utilitys.Sum(particle.NeighborParticles, (neighbor) =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighbor.Mass * Vector2.Dot((massOverDensity2 * nablaCubicSpline), nablaCubicSpline);
            });

            var timeStep2 = timeStep * timeStep;
            return - (timeStep2 * sum1) - (timeStep2 * sum2);
        }

        // Eq 39
        private static float ComputeSourceTerm(Particle particle, float particleDiameter, float timeStep, float fluidDensity)
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
            var particlePressureOverDensity2 =   particle.Pressure / (particle.Density * particle.Density) ;

            return - Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);

                return neighbor.Mass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
            });
        }

        // Eq 40
        private static float ComputeLaplacian(Particle particle, float timeStep, float particleDiameter)
        {
            var timeStep2 = timeStep * timeStep;
            var sum = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighbor.Mass * Vector2.Dot(particle.Acceleration - neighbor.Acceleration, nablaCubicSpline);
            });

            return timeStep2 * sum;
        }

        // Eq 48
        private static float UpdatePressure(float pressure, float diagonalElement, float sourceTerm, float laplacian)
        {
            var omegaOverDiagonalElement = .5f / diagonalElement;
            var diff = sourceTerm - laplacian;
            var value = pressure + (omegaOverDiagonalElement * diff);
            return float.Max(value, 0);
        }

        public static float ComputeDensityError(float laplacian, float sourceTerm, float fluidDensity)
            => (laplacian - sourceTerm) / fluidDensity;


        public static void SolveLocalPressures(List<Particle> particles, float particleDiameter, float timeStep, float fluidDensity)
        {
            foreach (var particle in particles)
            {
                particle.DiagonalElement = ComputeDiagonalElement(particle, particleDiameter, timeStep);
                particle.SourceTerm = ComputeSourceTerm(particle, particleDiameter, timeStep, fluidDensity);
                particle.Pressure = 0;
            }

            var densityAverageError = float.PositiveInfinity;
            var iterations = 0;

            while (densityAverageError >= 0.001f)
            {
                foreach (var particle in particles)
                    particle.Acceleration = ComputePressureAcceleration(particle, particleDiameter);

                var densityErrorSum = 0f;
                foreach (var particle in particles)
                {
                    particle.Laplacian = ComputeLaplacian(particle, timeStep, particleDiameter);
                    particle.Pressure = UpdatePressure(particle.Pressure, particle.DiagonalElement, particle.SourceTerm, particle.Laplacian);
                    densityErrorSum += ComputeDensityError(particle.Laplacian, particle.SourceTerm, fluidDensity);
                }

                densityAverageError = densityErrorSum / particles.Count;
                iterations++;

                if (iterations > 10000) 
                    throw new TimeoutException();
            }
        }
    }
}
