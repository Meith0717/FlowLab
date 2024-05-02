using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Fluid_Simulator.Core
{
    public static class SphFluidSolver
    {
        public static double GetMass(double H, double density) => GetCircleVolume(H / 2) * density;
        private static double GetCircleVolume(double radius) => 4 / 3 * Math.PI * Math.Pow(radius, 3);      
        
        public static double KernelFunction(Vector2 position1, Vector2 position2, double H)
        {
            var distance = Vector2.Distance(position1, position2);
            var distanceOverH = distance / H;

            if (distanceOverH < 0) return 0;
            if (distanceOverH < 1) return Math.Pow(2 - distanceOverH, 3) - (4 * Math.Pow(1 - distanceOverH, 3));
            if (distanceOverH < 2) return Math.Pow(2 - distanceOverH, 3);
            return 0;
        }

        public static double Solver(Particle particle, List<Particle> neighbors)
        {
             
            return 0;
        }

    }
}
