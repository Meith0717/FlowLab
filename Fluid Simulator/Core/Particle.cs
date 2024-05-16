﻿using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;

namespace Fluid_Simulator.Core
{
    public class Particle
    {
        public Vector2 Position;
        public readonly float Mass;

        public Vector2 Velocity;
        public Color Color;
        public readonly bool IsBorder;

        public Particle(Vector2 position, Color? color = null, bool isBorder = false)
        {
            var diameter = SimulationConfig.ParticleDiameter;
            var density = SimulationConfig.FluidDensity;

            Position = position;
            var volume = MathF.Pow(diameter, 2);
            Mass = volume * density;
            Color = color is null ? Color.White : (Color)color;
            IsBorder = isBorder;
        }
    }
}