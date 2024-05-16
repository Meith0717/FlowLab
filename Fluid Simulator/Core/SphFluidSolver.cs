using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Fluid_Simulator.Core
{
    public static class SphFluidSolver
    {        

        private static float KernelAlpha(float particelDiameter) => 5 / (14 * MathF.PI * MathF.Pow(particelDiameter, 2));

        public static float Kernel(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var distanceOverH = Vector2.Distance(position1, position2) / particelDiameter;
            var bracketsValue = distanceOverH switch
                {
                    < 0 => throw new SystemException(),
                    < 1 => MathF.Pow(2 - distanceOverH, 3) - (4 * MathF.Pow(1 - distanceOverH, 3)),
                    < 2 => MathF.Pow(2 - distanceOverH, 3),
                    >= 2 => 0,
                    _ => throw new NotImplementedException()
                };
            var alpha = KernelAlpha(particelDiameter);
            return alpha * bracketsValue;
        }

        public static Vector2 KernelDerivative(Vector2 position1, Vector2 position2, float particelDiameter)
        {
            var positionDifference = position1 - position2;
            var distance = Vector2.Distance(position1, position2);
            var distanceOverH = distance / particelDiameter;
            var bracketsValue =  distanceOverH switch
            {
                < 0 => throw new SystemException(),
                < 1 => -3 * MathF.Pow(2 - distanceOverH, 2) + (12 * MathF.Pow(1 - distanceOverH, 2)),
                < 2 => -3 * MathF.Pow(2 - distanceOverH, 2),
                >= 2 => 0,
                _ => throw new NotImplementedException()
            };
            if (distance == 0) return Vector2.Zero;
            return KernelAlpha(particelDiameter) * (positionDifference /  (distance * particelDiameter)) * bracketsValue;
        }

        public static float ComputeLocalDensity(float particelDiameter, Particle particle, List<Particle> neighbors)
        {
            var density = 0f;
            foreach (var neighbor in neighbors)
                density += neighbor.Mass * Kernel(particle.Position, neighbor.Position, particelDiameter);
            return density;
        }

        public static float ComputeLocalPressure(float fluidStiffness, float fluidDensity, float localDensity)
            => fluidStiffness * ((localDensity / fluidDensity) - 1);

        public static Vector2 GetPressureAcceleration(float particelDiameter, Dictionary<Particle, float> localPressures, Dictionary<Particle, float> localDensitys, Particle particle, List<Particle> neighbors)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = localPressures[particle] / MathF.Pow(localDensitys[particle], 2);
            foreach (var neighbor in neighbors)
            {
                var neighborPressureOverDensitySquared = localPressures[neighbor] / MathF.Pow(localDensitys[neighbor], 2);
                var kernelDerivative = KernelDerivative(particle.Position, neighbor.Position, particelDiameter);
                pressureAcceleration += neighbor.Mass * (pressureOverDensitySquared + neighborPressureOverDensitySquared) * kernelDerivative;
            }
            return - pressureAcceleration;
        }

        public static Vector2 GetViscosityAcceleration(float particelDiameter, float fluidViscosity, Particle particle, List<Particle> neighbors, Dictionary<Particle, float> localDensitys)
        {
            Vector2 sum = Vector2.Zero;
            foreach (var neighbor in neighbors) 
            {
                var velocityDifference = particle.Velocity - neighbor.Velocity;
                var positionDifference = particle.Position - neighbor.Position;

                var massDensityRatio = neighbor.Mass / localDensitys[neighbor];
                var dotVelocityPosition = Vector2.Dot(velocityDifference, positionDifference);
                var dotPositionPosition = Vector2.Dot(positionDifference, positionDifference);
                var scaledParticleDiameter = 0.01f * particelDiameter;
                var kernelDerivative = KernelDerivative(particle.Position, neighbor.Position, particelDiameter);

                sum += massDensityRatio * (dotVelocityPosition / (dotPositionPosition + scaledParticleDiameter)) * kernelDerivative;
            }
            return 2 * fluidViscosity * sum;
        }
    }
}
