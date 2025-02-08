// SPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using System.Diagnostics;

namespace FlowLab.Logic.SphComponents
{
    internal static class SPHComponents
    {
        public static float ComputeDynamicTimeStep(SimulationSettings settings, SolverState state, float particleDiameter)
        {
            float ts;
            if (state.MaxParticleVelocity == 0)
                ts = float.PositiveInfinity;
            else
                ts = settings.MaxCfl * (particleDiameter / state.MaxParticleVelocity);
            return float.Min(settings.MaxTimeStep, ts);
        }

        public static void ComputeLocalDensity(Particle particle, float gamma)
        {
            particle.Density = 0;
            foreach (var neighbor in particle.Neighbors)
            {
                var density = neighbor.Mass * particle.Kernel(neighbor);
                particle.Density += neighbor.IsBoundary ? gamma * density : density;
                if (float.IsNaN(particle.Density))
                    Debugger.Break();
            }
        }

        public static void PressureExtrapolation(Particle particle, float gravitation)
        {
            particle.Pressure = 0;
            if (particle.FluidNeighbors.Count == 0) return;

            var s1 = 0f;
            foreach (var neighbor in particle.FluidNeighbors)
            {
                s1 += neighbor.Pressure * particle.Kernel(neighbor);
                if (float.IsNaN(s1))
                    Debugger.Break();
            }

            var s2 = System.Numerics.Vector2.Zero;
            foreach (var neighbor in particle.FluidNeighbors)
                s2 += neighbor.Density * (particle.Position - neighbor.Position) * particle.Kernel(neighbor);

            var s3 = 0f;
            foreach (var neighbor in particle.FluidNeighbors)
                s3 += particle.Kernel(neighbor);

            var dotProduct = System.Numerics.Vector2.Dot(new System.Numerics.Vector2(0, gravitation), s2);
            if (float.IsNaN(dotProduct))
                dotProduct = 0;
            particle.Pressure = (s1 + dotProduct) / s3;
        }

        public static void ComputePressureAcceleration(Particle fParticle, float gamma, bool mirroring)
        {
            fParticle.PressureAcceleration = System.Numerics.Vector2.Zero;
            float particlePressureOverDensity2 = fParticle.Pressure / (fParticle.Density * fParticle.Density);

            var sum1 = System.Numerics.Vector2.Zero;
            foreach (var neighbor in fParticle.FluidNeighbors)
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                var kernelDerivative = fParticle.KernelDerivativ(neighbor);
                var combinedPressure = particlePressureOverDensity2 + neighborPressureOverDensity2;
                sum1 += neighbor.Mass * combinedPressure * kernelDerivative;
                if (float.IsNaN(sum1.X) || float.IsNaN(sum1.Y))
                    Debugger.Break();
            }

            var sum2 = System.Numerics.Vector2.Zero;
            foreach (var neighbor in fParticle.BoundaryNeighbors)
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                var kernelDerivative = fParticle.KernelDerivativ(neighbor);
                var combinedPressure = mirroring ? 2 * particlePressureOverDensity2 : particlePressureOverDensity2 + neighborPressureOverDensity2;
                sum2 += neighbor.Mass * combinedPressure * kernelDerivative;
                if (float.IsNaN(sum2.X) || float.IsNaN(sum2.Y))
                    Debugger.Break();
            }

            fParticle.PressureAcceleration = -sum1 - (gamma * sum2);
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
                if (float.IsNaN(res.X) || float.IsNaN(res.Y))
                    Debugger.Break();

                var viscosity = neighbor.IsBoundary ? boundaryViscosity : fluidViscosity;
                if (float.IsNaN(viscosity))
                    Debugger.Break();

                fParticle.ViscosityAcceleration += 2f * viscosity * res;
            }
        }
    }
}
