using Fluid_Simulator.Core.ParticleManagement;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Fluid_Simulator.Core.SphComponents
{
    internal static class IISPHComponents
    {
        //Eq 49 Techniques for the Physics Based Simulation of Fluids and Solids
        public static void ComputeDiagonalElement(Particle particle, float particleDiameter, float timeStep, out float value)
        {
          
            var innerSum1 = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                var massOverDensity2 = neighbor.Mass / (neighbor.Density * neighbor.Density);
                return massOverDensity2 * nablaCubicSpline;
            });

            var sum1 = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighbor.Mass * Vector2.Negate(innerSum1).Dot(nablaCubicSpline);
            });

            var particleMassOverDensity2 = particle.Mass / (particle.Density * particle.Density);
            var sum2 = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                var prod = particleMassOverDensity2 * Vector2.Negate(nablaCubicSpline);
                return neighbor.Mass * prod.Dot(nablaCubicSpline);
            });

            var timeStep2 = (timeStep * timeStep);
            value = timeStep2 * (sum1 + sum2);
            particle.DiagonalElement = value;
        }

        /// <summary>
        /// (Eq 39 from Techniques for the Physics Based Simulation of Fluids and Solids)
        /// </summary>
        /// <param name="timeStep"></param>
        /// <param name="fluidDensity"></param>
        /// <param name="particle"></param>
        /// <param name="particleDiameter"></param>
        /// <param name="value"></param>
        public static void ComputeSourceTerm(float timeStep, float fluidDensity, Particle particle, float particleDiameter, out float value)
        {
            var sum = timeStep * Utilitys.Sum(particle.NeighborParticles, neighbor => neighbor.Mass * Vector2.Subtract(particle.Velocity, neighbor.Velocity).Dot(SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter)));
            value = fluidDensity - particle.Density - sum;
            particle.SourceTerm = value;
        }

        /// <summary>
        /// (Eq 41 Techniques for the Physics Based Simulation of Fluids and Solids)
        /// </summary>
        /// <param name="particle"></param>
        /// <param name="particleDiameter"></param>
        /// <returns></returns>
        public static void ComputePressureAcceleration(Particle particle, float particleDiameter, out Vector2 value)
        {
            var particlePressureOverDensity2 = particle.Pressure / (particle.Density * particle.Density) ;
            var res = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var neighborPressureOverDensity2 = neighbor.Pressure / (neighbor.Density * neighbor.Density);
                var pressureOverDensity2Sum = particlePressureOverDensity2 + neighborPressureOverDensity2;
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return neighbor.Mass * pressureOverDensity2Sum * nablaCubicSpline;
            });
            value = Vector2.Negate(res);
            particle.Acceleration = value;
        }

        /// <summary>
        /// (Eq 40 Techniques for the Physics Based Simulation of Fluids and Solids)
        /// </summary>
        /// <param name="particle"></param>
        /// <param name="timeStep"></param>
        /// <param name="particleDiameter"></param>
        /// <returns></returns>
        public static void ComputeLaplacian(Particle particle, float timeStep, float particleDiameter, out float value)
        {
            var timeStep2 = (timeStep * timeStep);
            value = timeStep2 * Utilitys.Sum(particle.NeighborParticles, neighbor => neighbor.Mass * Vector2.Subtract(particle.Acceleration, neighbor.Acceleration).Dot(SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter)));
        }

        /// <summary>
        /// Updates pressure for next iteration based on source term, diagonal element and laplacian.
        /// (Eq 48 from Techniques for the Physics Based Simulation of Fluids and Solids)
        /// </summary>
        /// <param name="particle"></param>
        /// <param name="laplacian"></param>
        /// <param name="omega"></param>
        public static void UpdatePressure(Particle particle, float laplacian, float omega = .5f)
        {
            var sourceTerm = particle.SourceTerm;
            var diagonalElement = particle.DiagonalElement;
            var pressure = particle.Pressure;

            var diff = sourceTerm - laplacian;
            var k = omega / diagonalElement;
            var value = float.Max(pressure + (k * diff), 0);
            particle.Pressure = (diagonalElement == 0) ? 0 : value;
        }
    }
}
