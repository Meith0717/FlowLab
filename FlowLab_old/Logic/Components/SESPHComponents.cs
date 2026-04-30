// SESPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using System;

namespace FlowLab.Logic.SphComponents
{
    internal class SESPHComponents
    {
        public static void StateEquation(Particle particle, float fluidDensity, float fluidStiffness)
        {
            particle.Pressure = MathF.Max(fluidStiffness * ((particle.Density / fluidDensity) - 1), 0);
        }
    }
}
