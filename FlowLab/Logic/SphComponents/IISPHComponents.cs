// IISPHComponents.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Logic.ParticleManagement;
using MonoGame.Extended;

namespace FlowLab.Logic.SphComponents
{
    /// <summary>
    /// https://interactivecomputergraphics.github.io/physics-simulation/examples/iisph.html
    /// </summary>
    internal static class IISPHComponents
    {
        public static void ComputeDiagonalElement(Particle particle, float timeStep)
        {
            var KernelDerivativ = particle.KernelDerivativ;
            var sum1 = Utilitys.Sum(particle.Neighbors, neighbor => (neighbor.Mass * KernelDerivativ(neighbor)).SquaredNorm());

            var sum2 = Utilitys.Sum(particle.Neighbors, neighbor => neighbor.Mass * KernelDerivativ(neighbor)).SquaredNorm();

            var sum = sum1 + sum2;

            particle.AII = -timeStep / (particle.Density * particle.Density) * sum;
            if (float.IsNaN(particle.AII)) throw new System.Exception();
        }

        public static void ComputeSourceTerm(float timeStep, Particle particle)
        {
            var KernelDerivativ = particle.KernelDerivativ;

            var predDensityOfNonPVel = Utilitys.Sum(particle.Neighbors, neighbor =>
            {
                var velDif = particle.Velocity - neighbor.Velocity;
                return (neighbor.Mass * velDif).Dot(KernelDerivativ(neighbor));
            });
            var predDensity = particle.Density + (timeStep * predDensityOfNonPVel);
            particle.St = (particle.Density0 - predDensity) / timeStep;
            if (float.IsNaN(particle.St)) throw new System.Exception();
        }

        public static void ComputePressureAcceleration(Particle particle)
        {
            var KernelDerivativ = particle.KernelDerivativ;

            var particlePressureOverDensity2 = particle.Pressure / (particle.Density * particle.Density);
            particle.PressureAcceleration = -Utilitys.Sum(particle.Neighbors, neighbor =>
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                return neighbor.Mass * (particlePressureOverDensity2 + neighborPressureOverDensity2) * KernelDerivativ(neighbor);
            });
            if (float.IsNaN(particle.PressureAcceleration.X) || float.IsNaN(particle.PressureAcceleration.Y)) throw new System.Exception();
        }

        public static void ComputeLaplacian(Particle particle, float timeStep)
        {
            var KernelDerivativ = particle.KernelDerivativ;
            particle.Ap = timeStep * Utilitys.Sum(particle.Neighbors, neighbor => (neighbor.Mass * (particle.Acceleration - neighbor.Acceleration)).Dot(KernelDerivativ(neighbor)));
            if (float.IsNaN(particle.Ap)) throw new System.Exception();
        }
    }
}
