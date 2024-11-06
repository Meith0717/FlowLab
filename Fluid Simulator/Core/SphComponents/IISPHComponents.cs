using Fluid_Simulator.Core.ParticleManagement;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;

namespace Fluid_Simulator.Core.SphComponents
{
    internal static class IISPHComponents
    {
        //Eq 49 Techniques for the Physics Based Simulation of Fluids and Solids
        private static void ComputeDiagonalElement1(Particle particle, float particleDiameter, float timeStep, out float value)
        {
            var particleMassOverDensity2 = particle.Mass / (particle.Density * particle.Density);
            var sum1 = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return (neighbor.Mass * (particleMassOverDensity2 * nablaCubicSpline)).Dot(nablaCubicSpline);
            });

            var sum2 = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return (neighbor.Mass / (neighbor.Density * neighbor.Density)) * nablaCubicSpline;
            }); 

            value = (timeStep * timeStep) * Utilitys.Sum(particle.NeighborParticles, neighbor =>
            {
                var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
                return (neighbor.Mass * Vector2.Negate(sum2)).Dot(nablaCubicSpline) + ((timeStep * timeStep) * sum1);
            });

            particle.DiagonalElement = value;
        }

        public static void ComputeDiagonalElement(Particle particle, float particleDiameter, float timeStep, out float value)
        {
            //var innerSum = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            //{
            //    var massOverDensity2 = neighbor.Mass / (neighbor.Density * neighbor.Density);
            //    var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
            //    return massOverDensity2 * nablaCubicSpline;
            //});

            //var sum1 = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            //{
            //    var nablaCubicSpline = SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter);
            //    return Vector2.Dot(neighbor.Mass * innerSum, nablaCubicSpline);
            //});

            //var massOverDensity2 = particle.Mass / (particle.Density * particle.Density);
            //var sum2 = Utilitys.Sum(particle.NeighborParticles, neighbor =>
            //{
            //    var nablaCubicSpline = SphKernel.NablaCubicSpline(neighbor.Position, particle.Position, particleDiameter);
            //    return Vector2.Dot(neighbor.Mass * (massOverDensity2 * nablaCubicSpline), nablaCubicSpline);
            //});

            //var timeStep2 = timeStep * timeStep;
            value = 1f;// -timeStep2 * (sum1 + sum2);
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
        private static void ComputeSourceTerm(float timeStep, float fluidDensity, Particle particle, float particleDiameter, out float value)
        {
            var sum = timeStep * Utilitys.Sum(particle.NeighborParticles, neighbor => (neighbor.Mass * (particle.Velocity - neighbor.Velocity)).Dot(SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter)));
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
        private static void ComputeLaplacian(Particle particle, float timeStep, float particleDiameter, out float value)
        { 
            value = (timeStep * timeStep) * Utilitys.Sum(particle.NeighborParticles, neighbor => (neighbor.Mass * (particle.Acceleration - neighbor.Acceleration)).Dot(SphKernel.NablaCubicSpline(particle.Position, neighbor.Position, particleDiameter)));
        }

        /// <summary>
        /// Updates pressure for next iteration based on source term, diagonal element and laplacian.
        /// (Eq 48 from Techniques for the Physics Based Simulation of Fluids and Solids)
        /// </summary>
        /// <param name="particle"></param>
        /// <param name="laplacian"></param>
        /// <param name="omega"></param>
        private static void UpdatePressure(Particle particle, float laplacian, float omega = .5f)
        {
            var sourceTerm = particle.SourceTerm;
            var diagonalElement = particle.DiagonalElement;
            var pressure = particle.Pressure;

            var diff = (sourceTerm - laplacian) / diagonalElement;
            var value = float.Max(pressure + (omega * diff), 0);
            particle.Pressure = (diagonalElement == 0) ? 0 : value;
        }

        /// <summary>
        /// This solver implements the Jacobi method for solving the Pressure Poisson Equation (PPE)
        /// </summary>
        /// <param name="particles"></param>
        /// <param name="particleDiameter"></param>
        /// <param name="timeStep"></param>
        /// <param name="fluidDensity"></param>
        /// <param name="i"></param>
        /// <param name="avError"></param>
        public static void SolveLocalPressures(List<Particle> particles, float particleDiameter, float timeStep, float fluidDensity, out int i, out float avError)
        {
            var test = new List<float>(); 
            foreach (var particle in particles)
            {
                ComputeSourceTerm(timeStep, fluidDensity, particle, particleDiameter, out var sT); // Nothing Found, Works fine
                ComputeDiagonalElement(particle, particleDiameter, timeStep, out var dE);
                particle.Pressure = 0;
                UpdatePressure(particle, 0); // Nothing Found, Works fine
                test.Add(dE);
            }

            i = 0;
            avError = float.PositiveInfinity;
            var errors = new List<float>();

            while (avError >= float.PositiveInfinity || i < 10)
            {
                foreach (var particle in particles)
                    ComputePressureAcceleration(particle, particleDiameter, out var aP); // Nothing Found, Works fine

                var errorSum = 0f;
                foreach (var particle in particles)
                {
                    ComputeLaplacian(particle, timeStep, particleDiameter, out var laplacian); // Nothing Found, Works fine
                    UpdatePressure(particle, laplacian, .5f); // Nothing Found, Works fine
                    errorSum += Math.Abs(laplacian - particle.SourceTerm) / fluidDensity;
                }

                avError = errorSum / particles.Count;
                errors.Add(avError);
                i++;
            }
        }
    }
}
