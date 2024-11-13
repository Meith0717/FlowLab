// SPHComponents.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;

namespace FlowLab.Logic.SphComponents
{
    public static class SPHComponents
    {
        public static void ComputeLocalDensity(Particle particle)
        {
            particle.Density = Utilitys.Sum(particle.Neighbors, neighbor => neighbor.Mass * particle.Kernel(neighbor));
            if (float.IsNaN(particle.Density)) throw new System.Exception();
            if (particle.Density == 0) throw new System.Exception();
        }

        public static Vector2 ComputePressureAcceleration(Particle particle)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureBoundaryAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = particle.Pressure / (particle.Density * particle.Density);

            var sum = Utilitys.Sum(particle.Neighbors, neighbor =>
            {
                var nablaCubicSpline = particle.KernelDerivativ(neighbor);
                var neighborPressureOverDensitySquared = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                if (neighbor.IsBoundary)
                    return neighbor.Mass * 2f * pressureOverDensitySquared * nablaCubicSpline;
                return neighbor.Mass * (pressureOverDensitySquared + neighborPressureOverDensitySquared) * nablaCubicSpline;
            });

            return -sum;
        }

        public static void ComputeViscosityAcceleration(float h, float fluidViscosity, Particle particle)
        {
            particle.ViscosityAcceleration = Utilitys.Sum(particle.Neighbors, neighbor =>
            {
                var v_ij = particle.Velocity - neighbor.Velocity;
                var x_ij = particle.Position - neighbor.Position;
                var massOverDensity = neighbor.Mass / neighbor.Density;
                var dotVelocityPosition = Vector2.Dot(v_ij, x_ij);
                var dotPositionPosition = Vector2.Dot(x_ij, x_ij);
                var scaledParticleDiameter2 = 0.01f * (h * h);
                var kernelDerivative = particle.KernelDerivativ(neighbor);
                var res = massOverDensity * (dotVelocityPosition / (dotPositionPosition + scaledParticleDiameter2)) * kernelDerivative;
                if (neighbor.IsBoundary)
                    return fluidViscosity * res;
                return 2 * fluidViscosity * res;
            });
        }
    }
}
