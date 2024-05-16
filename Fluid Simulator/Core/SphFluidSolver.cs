using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Fluid_Simulator.Core
{
    public static class SphFluidSolver
    {
        public static float GetMass(float H, float density) => GetCircleVolume(H / 2) * density;

        private static float GetCircleVolume(float radius) => 4 / 3 * MathF.PI * MathF.Pow(radius, 3);  
        
        public static float Kernel(Vector2 position1, Vector2 position2, float H)
        {
            var distanceOverH = Vector2.Distance(position1, position2) / (float)H;
            var _const = 5 / (14 * MathF.PI * MathF.Pow(H, 2));
            var bracketsValue = distanceOverH switch
                {
                    < 0 => throw new SystemException(),
                    < 1 => MathF.Pow(2 - distanceOverH, 3) - (4 * MathF.Pow(1 - distanceOverH, 3)),
                    < 2 => MathF.Pow(2 - distanceOverH, 3),
                    >= 2 => 0,
                    _ => throw new NotImplementedException()
                };
            return _const * bracketsValue;
        }

        public static Vector2 KernelDerivative(Vector2 position1, Vector2 position2, float H)
        {
            var positionDifference = position1 - position2;
            var distance = Vector2.Distance(position1, position2);
            var distanceOverH = distance / (float)H;
            var _const = 5 / (14 * MathF.PI * MathF.Pow((float)H, 3));
            var bracketsValue =  distanceOverH switch
            {
                < 0 => throw new SystemException(),
                < 1 => -3 * MathF.Pow(2 - distanceOverH, 2) + (12 * MathF.Pow(1 - distanceOverH, 2)),
                < 2 => -3 * MathF.Pow(2 - distanceOverH, 2),
                >= 2 => 0,
                _ => throw new NotImplementedException()
            };
            return (positionDifference / _const * distance) * bracketsValue;
        }

        public static float GetDensity(float H, float globalDensity, Particle particle, List<Particle> neighbors)
        {
            // IMPROVEMENT Parallel?
            var density = 0f;
            foreach (var neighbor in neighbors)
            {
                var mass = GetMass(H, globalDensity);
                var kernel = Kernel(particle.Position, neighbor.Position, H);
                density += mass * kernel;
            }
            
            return density;
        }

        public static float GetPressure(float stiffness, float localDensity, float globalDensity) => stiffness * ((localDensity / globalDensity) - 1);

        public static Vector2 GetPressureAcceleration(float H, float globalDensity, Dictionary<Particle, float> localPressures, Dictionary<Particle, float> localDensitys, Particle particle, List<Particle> neighbors)
        {
            var pressureAcceleration = Vector2.Zero;
            var pressureOverDensitySquared = localPressures[particle] / MathF.Pow(localDensitys[particle], 2);
            foreach (var neighbor in neighbors)
            {
                var mass = GetMass(H, globalDensity);
                var neighborPressureOverDensitySquared = localPressures[neighbor] / MathF.Pow(localDensitys[neighbor], 2);
                var kernelDerivative = KernelDerivative(particle.Position, neighbor.Position, H);
                pressureAcceleration += mass * (pressureOverDensitySquared + neighborPressureOverDensitySquared) * kernelDerivative;
            }
            return -pressureAcceleration;
        }

        public static double GetViscosityAcceleration(float viscosity, Particle particle, List<Particle> neighbors)
        {
            var sum = 0;
            foreach (var neighbor in neighbors)
            {

            }

            return 2 * viscosity * sum;
        }


    }
}
