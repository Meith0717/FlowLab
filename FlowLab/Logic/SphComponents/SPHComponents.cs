// SPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;

namespace FlowLab.Logic.SphComponents
{
    public static class SPHComponents
    {
        public static void ComputeLocalDensity(Particle particle, float gamma)
        {
            particle.Density = Utilitys.Sum(particle.Neighbors, neighbor => 
            {
                var density = neighbor.Mass * particle.Kernel(neighbor);
                if (neighbor.IsBoundary) density *= gamma;
                return density;
            });

            if (float.IsNaN(particle.Density)) 
                throw new System.Exception();
            if (particle.Density == 0) 
                throw new System.Exception();
        }

        public static void ComputePressureAccelerationWithReflection(Particle particle, float gamma)
        {
            var KernelDerivativ = particle.KernelDerivativ;
            var particlePressureOverDensity2 = particle.Pressure / (particle.Density * particle.Density);
            particle.PressureAcceleration = - Utilitys.Sum(particle.Neighbors, neighbor =>
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                if (neighbor.IsBoundary)
                    return gamma * neighbor.Mass * 2f * particlePressureOverDensity2 * KernelDerivativ(neighbor);
                return neighbor.Mass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * KernelDerivativ(neighbor);
            });
            if (float.IsNaN(particle.PressureAcceleration.X) || float.IsNaN(particle.PressureAcceleration.Y)) 
                throw new System.Exception();
        }

        public static void ComputePressureAcceleration(Particle particle, float gamma)
        {
            var KernelDerivativ = particle.KernelDerivativ;

            var particlePressureOverDensity2 = particle.Pressure / (particle.Density * particle.Density);
            particle.PressureAcceleration = -Utilitys.Sum(particle.Neighbors, neighbor =>
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                var acceleration = neighbor.Mass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * KernelDerivativ(neighbor);
                if (neighbor.IsBoundary) return gamma * acceleration;
                return acceleration;
            });
            if (float.IsNaN(particle.PressureAcceleration.X) || float.IsNaN(particle.PressureAcceleration.Y)) throw new System.Exception();
        }

        public static void ComputeViscosityAcceleration(float h, float boundaryViscosity, float fluidViscosity, Particle particle)
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
                    return 2 * boundaryViscosity * res;
                return 2 * fluidViscosity * res;
            });
        }
    }
}
