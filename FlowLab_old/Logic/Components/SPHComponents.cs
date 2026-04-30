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
            foreach (var neighbour in particle.neighbours)
            {
                var density = neighbour.Mass * particle.Kernel(neighbour);
                particle.Density += neighbour.IsBoundary ? gamma * density : density;
                if (float.IsNaN(particle.Density))
                    Debugger.Break();
            }
        }

        public static void PressureExtrapolation(Particle particle, float gravitation)
        {
            particle.Pressure = 0;
            if (particle.Fluidneighbours.Count == 0) return;

            var s1 = 0f;
            foreach (var neighbour in particle.Fluidneighbours)
            {
                s1 += neighbour.Pressure * particle.Kernel(neighbour);
                if (float.IsNaN(s1))
                    Debugger.Break();
            }

            var s2 = System.Numerics.Vector2.Zero;
            foreach (var neighbour in particle.Fluidneighbours)
                s2 += neighbour.Density * (particle.Position - neighbour.Position) * particle.Kernel(neighbour);

            var s3 = 0f;
            foreach (var neighbour in particle.Fluidneighbours)
                s3 += particle.Kernel(neighbour);

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
            foreach (var neighbour in fParticle.Fluidneighbours)
            {
                var neighbourPressureOverDensity2 = neighbour.Pressure / (neighbour.Density * neighbour.Density);
                var kernelDerivative = fParticle.KernelDerivativ(neighbour);
                var combinedPressure = particlePressureOverDensity2 + neighbourPressureOverDensity2;
                sum1 += neighbour.Mass * combinedPressure * kernelDerivative;
                if (float.IsNaN(sum1.X) || float.IsNaN(sum1.Y))
                    Debugger.Break();
            }

            var sum2 = System.Numerics.Vector2.Zero;
            foreach (var neighbour in fParticle.Boundaryneighbours)
            {
                var neighbourPressureOverDensity2 = neighbour.Pressure / (neighbour.Density * neighbour.Density);
                var kernelDerivative = fParticle.KernelDerivativ(neighbour);
                var combinedPressure = mirroring ? 2 * particlePressureOverDensity2 : particlePressureOverDensity2 + neighbourPressureOverDensity2;
                sum2 += neighbour.Mass * combinedPressure * kernelDerivative;
                if (float.IsNaN(sum2.X) || float.IsNaN(sum2.Y))
                    Debugger.Break();
            }

            fParticle.PressureAcceleration = -sum1 - (gamma * sum2);
        }

        public static void ComputeViscosityAcceleration(float h, float boundaryViscosity, float fluidViscosity, Particle fParticle)
        {
            float scaledParticleDiameter2 = 0.01f * (h * h);
            foreach (var neighbour in fParticle.neighbours)
            {
                var x_ij = fParticle.Position - neighbour.Position;
                var dotPositionPosition = System.Numerics.Vector2.Dot(x_ij, x_ij) + scaledParticleDiameter2;

                var v_ij = fParticle.Velocity - neighbour.Velocity;
                var dotVelocityPosition = System.Numerics.Vector2.Dot(v_ij, x_ij);

                var massOverDensity = neighbour.Mass / neighbour.Density;
                var kernelDerivative = fParticle.KernelDerivativ(neighbour);
                var res = massOverDensity * (dotVelocityPosition / dotPositionPosition) * kernelDerivative;
                if (float.IsNaN(res.X) || float.IsNaN(res.Y))
                    Debugger.Break();

                var viscosity = neighbour.IsBoundary ? boundaryViscosity : fluidViscosity;
                if (float.IsNaN(viscosity))
                    Debugger.Break();

                fParticle.ViscosityAcceleration += 2f * viscosity * res;
            }
        }
    }
}
