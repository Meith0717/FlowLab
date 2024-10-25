using Fluid_Simulator.Core.ParticleManagement;
using Microsoft.Xna.Framework;
using System;

namespace Fluid_Simulator.Core.SphComponents
{
    public static class SPHComponents
    {
        public static float ComputeLocalDensity(float particleDiameter, Particle particle) 
            => particle.Mass * Utilitys.Sum(particle.NeighborParticles, neighbor => SphKernel.CubicSpline(particle.Position, neighbor.Position, particleDiameter));

        public static Vector2 ComputePressureAcceleration(float particleDiameter, Particle particle)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureBoundaryAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = particle.Pressure / MathF.Pow(particle.Density, 2);

            var sum = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var kernelDerivative = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                var neighborPressureOverDensitySquared = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                if (neighbor.IsBoundary)
                    return neighbor.Mass * 2f * pressureOverDensitySquared * kernelDerivative;
                return neighbor.Mass * (pressureOverDensitySquared + neighborPressureOverDensitySquared) * kernelDerivative;
            });

            return - sum;
        }

        public static Vector2 ComputeViscosityAcceleration(float particleDiameter, float fluidViscosity, Particle particle)
        {
            return  Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var v_ij = particle.Velocity - neighbor.Velocity;
                var x_ij = particle.Position - neighbor.Position;
                var massOverDensity = neighbor.Mass / neighbor.Density;
                var dotVelocityPosition = Vector2.Dot(v_ij, x_ij);
                var dotPositionPosition = Vector2.Dot(x_ij, x_ij);
                var scaledParticleDiameter2 = 0.01f * (particleDiameter * particleDiameter);
                var kernelDerivative = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                var res = massOverDensity * (dotVelocityPosition / (dotPositionPosition + scaledParticleDiameter2)) * kernelDerivative;
                if (neighbor.IsBoundary) 
                    return fluidViscosity * res;
                return 2 * fluidViscosity * res;
            });
        }
    }
}
