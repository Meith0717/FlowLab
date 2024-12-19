// SPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System.Linq;

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

        public static void PressureExtrapolation(Particle bParticle, float gravitation)
        {
            var s1 = Utilitys.Sum(bParticle.Neighbors.Where(n => !n.IsBoundary), (fNeighbor) => fNeighbor.Pressure * fNeighbor.Kernel(fNeighbor));
            var s2 = Utilitys.Sum(bParticle.Neighbors, (fNeighbor) => fNeighbor.Density * (bParticle.Position - fNeighbor.Position) * fNeighbor.Kernel(fNeighbor));
            var s3 = Utilitys.Sum(bParticle.Neighbors, (fNeighbor) => fNeighbor.Kernel(fNeighbor));

            bParticle.Pressure = (s1 + new Vector2(0, -gravitation).Dot(s2)) / s3;
        }

        public static void ComputePressureAccelerationWithReflection(Particle fParticle, float gamma)
        {
            var KernelDerivativ = fParticle.KernelDerivativ;
            var particlePressureOverDensity2 = fParticle.Pressure / (fParticle.Density * fParticle.Density);
            fParticle.PressureAcceleration = -Utilitys.Sum(fParticle.Neighbors, neighbor =>
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                if (neighbor.IsBoundary)
                    return gamma * neighbor.Mass * 2f * particlePressureOverDensity2 * KernelDerivativ(neighbor);
                return neighbor.Mass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * KernelDerivativ(neighbor);
            });
            if (float.IsNaN(fParticle.PressureAcceleration.X) || float.IsNaN(fParticle.PressureAcceleration.Y))
                throw new System.Exception();
        }

        public static void ComputePressureAcceleration(Particle fParticle, float gamma)
        {
            var KernelDerivativ = fParticle.KernelDerivativ;

            var particlePressureOverDensity2 = fParticle.Pressure / (fParticle.Density * fParticle.Density);
            fParticle.PressureAcceleration = -Utilitys.Sum(fParticle.Neighbors, neighbor =>
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                var acceleration = neighbor.Mass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * KernelDerivativ(neighbor);
                if (neighbor.IsBoundary) return gamma * acceleration;
                return acceleration;
            });
            if (float.IsNaN(fParticle.PressureAcceleration.X) || float.IsNaN(fParticle.PressureAcceleration.Y)) throw new System.Exception();
        }

        public static void ComputeViscosityAcceleration(float h, float boundaryViscosity, float fluidViscosity, Particle fParticle)
        {
            fParticle.ViscosityAcceleration = Utilitys.Sum(fParticle.Neighbors, neighbor =>
            {
                var v_ij = fParticle.Velocity - neighbor.Velocity;
                var x_ij = fParticle.Position - neighbor.Position;
                var massOverDensity = neighbor.Mass / neighbor.Density;
                var dotVelocityPosition = Vector2.Dot(v_ij, x_ij);
                var dotPositionPosition = Vector2.Dot(x_ij, x_ij);
                var scaledParticleDiameter2 = 0.01f * (h * h);
                var kernelDerivative = fParticle.KernelDerivativ(neighbor);
                var res = massOverDensity * (dotVelocityPosition / (dotPositionPosition + scaledParticleDiameter2)) * kernelDerivative;
                if (neighbor.IsBoundary)
                    return 2 * boundaryViscosity * res;
                return 2 * fluidViscosity * res;
            });
        }
    }
}
