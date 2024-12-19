// SESPHComponents.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using System;

namespace FlowLab.Logic.SphComponents
{
    internal class SESPHComponents
    {
        public static void ComputeLocalPressure(Particle particle, float fluidStiffness)
        {
            particle.Pressure = MathF.Max(fluidStiffness * ((particle.Density / particle.Density0) - 1), 0);
        }
    }
}
