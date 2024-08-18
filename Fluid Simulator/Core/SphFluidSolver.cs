using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    public static class SphFluidSolver
    {
        private const float kernelCorrection = 0.04f / 0.0400344729f;

        public static float KernelAlpha(float particelDiameter)
            => 5 / (14 * MathF.PI * MathF.Pow(particelDiameter, 2));
        private static float DistanceOverH(Vector2 pos1, Vector2 pos2, float H)
            => Vector2.Distance(pos1, pos2) / H;
        public static float ComputeLocalPressure(float fluidStiffness, float fluidDensity, float localDensity)
            => MathF.Max(fluidStiffness * ((localDensity / fluidDensity) - 1), 0);

        public static float Kernel(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var alpha = KernelAlpha(particelDiameter);
            var distanceOverH = DistanceOverH(position1, position2, particelDiameter);
            var t1 = MathF.Max(1 - distanceOverH, 0);
            var t2 = MathF.Max(2 - distanceOverH, 0);
            var t3 = (t2 * t2 * t2) - 4 * (t1 * t1 * t1);
            return alpha * t3 * kernelCorrection;
        }

        public static Vector2 KernelDerivative(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var positionDifference = position1 - position2;
            var distanceOverH = DistanceOverH(position1, position2, particelDiameter);
            if (distanceOverH == 0) return Vector2.Zero;
            var t1 = MathF.Max(1 - distanceOverH, 0);
            var t2 = MathF.Max(2 - distanceOverH, 0);
            var t3 = (-3 * t2 * t2) + (12 * t1 * t1);
            return KernelAlpha(particelDiameter) * (positionDifference / (positionDifference.Length() * particelDiameter)) * t3;
        }

        public static float ComputeLocalDensity(float particelDiameter, Particle particle, List<Particle> neighbors)
        {
            var density = 0f;
            foreach (var neighbor in neighbors)
                density += neighbor.Mass * Kernel(particle.Position, neighbor.Position, particelDiameter);
            return density;
        }

        public static Vector2 GetPressureAcceleration(float particelDiameter, Particle particle, List<Particle> neighbors)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureBoundaryAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = particle.Pressure / MathF.Pow(particle.Density, 2);
            foreach (var neighbor in neighbors)
            {
                var kernelDerivative = KernelDerivative(particle.Position, neighbor.Position, particelDiameter);
                if (neighbor.IsBoundary)
                {
                    pressureBoundaryAcceleration += neighbor.Pressure * (2f * neighbor.Mass / MathF.Pow(particle.Density, 2)) * kernelDerivative;
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
                var kernelDerivative = KernelDerivative(particle.Position, neighbor.Position, particelDiameter);

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

        public static Vector2 GetSurfaceTensionAcceleration(float surfaceTension, float particleDiameter, Particle particle, List<Particle> neighbors)
        {
            var a = Vector2.Zero;
            foreach (var neighbor in neighbors)
            {
                if (neighbor.IsBoundary) continue;
                a += KernelDerivative(particle.Position, neighbor.Position, particleDiameter);
            }
            if (a.Length() < .0001f) return Vector2.Zero;
            return surfaceTension * a;
        }
    }
}
