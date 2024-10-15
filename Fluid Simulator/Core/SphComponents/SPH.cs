using Fluid_Simulator.Core.SphComponents;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Fluid_Simulator.Core.SphComponents
{
    public static class Sph
    {
        public static float ComputeLocalDensity(float particelDiameter, Particle particle, List<Particle> neighbors) => Utilitys.Sum(neighbors, neighbor => neighbor.Mass * SphKernel.CubicSpline(particle.Position, neighbor.Position, particelDiameter));

        public static float ComputeLocalPressure(float fluidStiffness, float fluidDensity, float localDensity)
            => MathF.Max(fluidStiffness * ((localDensity / fluidDensity) - 1), 0);

        public static Vector2 GetPressureAcceleration(float particelDiameter, Particle particle, List<Particle> neighbors)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureBoundaryAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = particle.Pressure / MathF.Pow(particle.Density, 2);
            foreach (var neighbor in neighbors)
            {
                var kernelDerivative = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particelDiameter);
                if (neighbor.IsBoundary)
                {
                    pressureBoundaryAcceleration += neighbor.Mass * (2f * pressureOverDensitySquared) * kernelDerivative;
                    continue;
                }
                var neighborPressureOverDensitySquared = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                pressureAcceleration += neighbor.Mass * (pressureOverDensitySquared + neighborPressureOverDensitySquared) * kernelDerivative;
            }
            return -pressureAcceleration - pressureBoundaryAcceleration;
        }

        public static Vector2 GetViscosityAcceleration(float particelDiameter, float fluidViscosity, Particle particle, List<Particle> neighbors)
        {
            Vector2 sumNonBoundry = Vector2.Zero;
            Vector2 sumBoundry = Vector2.Zero;

            foreach (var neighbor in neighbors)
            {
                var v_ij = particle.Velocity - neighbor.Velocity;
                var x_ij = particle.Position - neighbor.Position;

                var massDensityRatio = neighbor.Mass / neighbor.Density;
                var dotVelocityPosition = Vector2.Dot(v_ij, x_ij);
                var dotPositionPosition = Vector2.Dot(x_ij, x_ij);
                var scaledParticleDiameter = 0.01f * (particelDiameter * particelDiameter);
                var kernelDerivative = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particelDiameter);

                var computaton = massDensityRatio * (dotVelocityPosition / (dotPositionPosition + scaledParticleDiameter)) * kernelDerivative;

                if (neighbor.IsBoundary)
                {
                    sumBoundry += computaton;
                    continue;
                }
                sumNonBoundry += computaton;
            }
            return (2 * fluidViscosity * sumNonBoundry) + (fluidViscosity * sumBoundry);
        }
    }
}
