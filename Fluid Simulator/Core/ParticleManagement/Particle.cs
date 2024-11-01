using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Fluid_Simulator.Core.ParticleManagement
{
    public class Particle
    {

        public readonly float Mass;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Density;
        public float Pressure;
        public readonly bool IsBoundary;

        public List<Particle> NeighborParticles= new();
        public Vector2 Acceleration;
        public float DiagonalElement;
        public float SourceTerm;
        public float Laplacian;

        public Particle(Vector2 position, float diameter, float fluidDensity, bool isBoundary)
        {
            Position = position;
            var volume = MathF.Pow(diameter, 2);
            Mass = volume * fluidDensity;
            IsBoundary = isBoundary;
        }
    }
}
