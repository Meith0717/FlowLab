// SPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;

namespace FlowLab.Logic.SphComponents
{
    internal static class SPHComponents
    {
        public static float ComputeDynamicTimeStep(SimulationSettings settings, SimulationState state, float particleDiameter) 
        {
            float ts;
            if (state.MaxVelocity == 0)
                ts = float.PositiveInfinity;
            else
                ts = settings.MaxCfl * (particleDiameter / state.MaxVelocity);
            return float.Min(settings.MaxTimeStep, ts);
        }

        public static void ComputeLocalDensity(Particle particle, float gamma)
        {
            particle.Density = 0;
            foreach (var neighbor in particle.Neighbors)
            {
                var density = neighbor.Mass * particle.Kernel(neighbor);
                particle.Density += neighbor.IsBoundary ? gamma * density : density;
            }
        }

        public static void PressureExtrapolation(Particle particle, float gravitation)
        {
            var s1 = 0f;
            foreach (var neighbor in particle.FluidNeighbors)
                s1 += neighbor.Pressure * particle.Kernel(neighbor);

            var s2 = System.Numerics.Vector2.Zero;
            foreach (var neighbor in particle.FluidNeighbors)
                s2 += neighbor.Density * (particle.Position - neighbor.Position) * particle.Kernel(neighbor);

            var s3 = 0f;
            foreach (var neighbor in particle.FluidNeighbors)
                s3 += particle.Kernel(neighbor);

            particle.Pressure = (s1 + System.Numerics.Vector2.Dot(new System.Numerics.Vector2(0, -gravitation), s2)) / s3;
        }

        public static void ComputePressureAcceleration(Particle fParticle, float gamma, bool mirroring)
        {
            float particlePressureOverDensity2 = fParticle.Pressure / (fParticle.Density * fParticle.Density);
            gamma *= mirroring ? 2 : 1;
            fParticle.PressureAcceleration = System.Numerics.Vector2.Zero;

            foreach (var neighbor in fParticle.Neighbors)
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                var kernelDerivative = fParticle.KernelDerivativ(neighbor);
                var combinedPressure = particlePressureOverDensity2 + neighborPressureOverDensity2;

                var acceleration = neighbor.Mass * combinedPressure * kernelDerivative;

                fParticle.PressureAcceleration -= neighbor.IsBoundary ? gamma * acceleration : acceleration;
            }
        }

        public static void ComputeViscosityAcceleration(float h, float boundaryViscosity, float fluidViscosity, Particle fParticle)
        {
            float scaledParticleDiameter2 = 0.01f * (h * h);
            foreach (var neighbor in fParticle.Neighbors)
            {
                var x_ij = fParticle.Position - neighbor.Position;
                var dotPositionPosition = System.Numerics.Vector2.Dot(x_ij, x_ij) + scaledParticleDiameter2;

                var v_ij = fParticle.Velocity - neighbor.Velocity;
                var dotVelocityPosition = System.Numerics.Vector2.Dot(v_ij, x_ij);

                var massOverDensity = neighbor.Mass / neighbor.Density;
                var kernelDerivative = fParticle.KernelDerivativ(neighbor);
                var res = massOverDensity * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;

                var viscosity = neighbor.IsBoundary ? boundaryViscosity : fluidViscosity;
                fParticle.ViscosityAcceleration += 2f * viscosity * res;
            }
        }
    }
}
