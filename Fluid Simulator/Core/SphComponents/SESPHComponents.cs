using Microsoft.Xna.Framework;
using System;

namespace Fluid_Simulator.Core.SphComponents
{
    public static class SESPHComponents
    {
        public static float ComputeLocalDensity(float particleDiameter, Particle particle) 
            => Utilitys.Sum(particle.NeighborParticles, neighbor => neighbor.Mass * SphKernel.CubicSpline(particle.Position, neighbor.Position, particleDiameter));

        public static float ComputeLocalPressure(float fluidStiffness, float fluidDensity, float localDensity)
            => MathF.Max(fluidStiffness * ((localDensity / fluidDensity) - 1), 0);

        public static Vector2 ComputePressureAcceleration(float particleDiameter, Particle particle)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureBoundaryAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = particle.Pressure / MathF.Pow(particle.Density, 2);

            foreach (var neighbor in particle.NeighborParticles)
            {
                var kernelDerivative = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
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

        public static Vector2 ComputeViscosityAcceleration(float particleDiameter, float fluidViscosity, Particle particle)
        {
            Vector2 sumNonBoundary = Vector2.Zero;
            Vector2 sumBoundary = Vector2.Zero;

            foreach (var neighbor in particle.NeighborParticles)
            {
                var v_ij = particle.Velocity - neighbor.Velocity;
                var x_ij = particle.Position - neighbor.Position;

                var massDensityRatio = neighbor.Mass / neighbor.Density;
                var dotVelocityPosition = Vector2.Dot(v_ij, x_ij);
                var dotPositionPosition = Vector2.Dot(x_ij, x_ij);
                var scaledParticleDiameter = 0.01f * (particleDiameter * particleDiameter);
                var kernelDerivative = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);

                var computation = massDensityRatio * (dotVelocityPosition / (dotPositionPosition + scaledParticleDiameter)) * kernelDerivative;

                if (neighbor.IsBoundary)
                {
                    sumBoundary += computation;
                    continue;
                }
                sumNonBoundary += computation;
            }
            return (2 * fluidViscosity * sumNonBoundary) + (fluidViscosity * sumBoundary);
        }
    }
}
