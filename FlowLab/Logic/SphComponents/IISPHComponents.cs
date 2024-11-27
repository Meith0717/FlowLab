// IISPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using FlowLab.Logic.ParticleManagement;
using MonoGame.Extended;
using System.Linq;

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
            var sum1 = Utilitys.Sum(particle.Neighbors.Where(p => !p.IsBoundary), n => (n.Mass * KernelDerivativ(n)).SquaredNorm());
            var sum2 = Utilitys.Sum(particle.Neighbors, n => n.Mass * KernelDerivativ(n));
            sum1 += sum2.SquaredNorm();

            particle.AII = - timeStep / (particle.Density * particle.Density) * sum1;
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

        public static void ComputeLaplacian(Particle particle, float timeStep)
        {
            var KernelDerivativ = particle.KernelDerivativ;
            particle.Ap = timeStep * Utilitys.Sum(particle.Neighbors, neighbor => (neighbor.Mass * (particle.Acceleration - neighbor.Acceleration)).Dot(KernelDerivativ(neighbor)));
            if (float.IsNaN(particle.Ap)) throw new System.Exception();
        }
    }
}
