using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Fluid_Simulator.Core
{
    public static class SphFluidSolver
    {        

        public static float KernelAlpha(float particelDiameter) => 5 / (14 * MathF.PI * MathF.Pow(particelDiameter, 2));
        private static float DistanceOverH(Vector2 pos1,  Vector2 pos2, float H) => Vector2.Distance(pos1, pos2) / H;

        public static float Kernel(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var alpha = KernelAlpha(particelDiameter);
            var distanceOverH = DistanceOverH(position1, position2, particelDiameter);
            var t1 = MathF.Max(1 - distanceOverH, 0);
            var t2 = MathF.Max(2 - distanceOverH, 0);
            return alpha * (MathF.Pow(t2, 3) - 4 * MathF.Pow(t1, 3));
        }

        public static Vector2 KernelDerivative(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var positionDifference = position1 - position2;
            var distanceOverH = DistanceOverH(position1, position2, particelDiameter);
            var bracketsValue =  distanceOverH switch
            {
                < 0 => throw new SystemException(),
                < 1 => -3 * MathF.Pow(2 - distanceOverH, 2) + (12 * MathF.Pow(1 - distanceOverH, 2)),
                < 2 => -3 * MathF.Pow(2 - distanceOverH, 2),
                >= 2 => 0,
                _ => throw new NotImplementedException()
            };
            if (positionDifference.Length() == 0) return Vector2.Zero;
            return KernelAlpha(particelDiameter) * (positionDifference /  (positionDifference.Length() * particelDiameter)) * bracketsValue;
        }

        public static float ComputeLocalDensity(float particelDiameter, Particle particle, List<Particle> neighbors)
        {
            var density = 0f;
            foreach (var neighbor in neighbors)
                density += neighbor.Mass * Kernel(particle.Position, neighbor.Position, particelDiameter);
            return density;
        }

        public static float ComputeLocalPressure(float fluidStiffness, float fluidDensity, float localDensity)
            => MathF.Max(fluidStiffness * ((localDensity / fluidDensity) - 1), 0);

        public static Vector2 GetPressureAcceleration(float particelDiameter, Particle particle, List<Particle> neighbors, Dictionary<Particle, float> localPressures, Dictionary<Particle, float> localDensitys)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = localPressures[particle] / MathF.Pow(localDensitys[particle], 2);
            foreach (var neighbor in neighbors)
            {
                var kernelDerivative = KernelDerivative(particle.Position, neighbor.Position, particelDiameter);
                if (neighbor.IsBoundary)
                {
                    pressureAcceleration -= neighbor.Mass * (2*pressureOverDensitySquared) * kernelDerivative;
                    continue;
                }
                var neighborPressureOverDensitySquared = localPressures[neighbor] / MathF.Pow(localDensitys[neighbor], 2);
                pressureAcceleration -= neighbor.Mass * (pressureOverDensitySquared + neighborPressureOverDensitySquared) * kernelDerivative;
            }
            return pressureAcceleration;
        }

        public static Vector2 GetViscosityAcceleration(float particelDiameter, float fluidViscosity, Particle particle, List<Particle> neighbors, Dictionary<Particle, float> localDensitys)
        {
            Vector2 sum = Vector2.Zero;
            foreach (var neighbor in neighbors) 
            {
                var v_ij = particle.Velocity - neighbor.Velocity;
                var x_ij = particle.Position - neighbor.Position;

                var massDensityRatio = neighbor.Mass / localDensitys[neighbor];
                var dotVelocityPosition = Vector2.Dot(v_ij, x_ij);
                var dotPositionPosition = Vector2.Dot(x_ij, x_ij);
                var scaledParticleDiameter = 0.01f * MathF.Pow(particelDiameter, 2);
                var kernelDerivative = KernelDerivative(particle.Position, neighbor.Position, particelDiameter);

                sum += massDensityRatio * (dotVelocityPosition / (dotPositionPosition + scaledParticleDiameter)) * kernelDerivative;
            }
            return 2 * fluidViscosity * sum;
        }
    }
}
