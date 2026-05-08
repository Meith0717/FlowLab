// SimulationConfig.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using Microsoft.Xna.Framework;

namespace FlowLab.Config;

/// <summary>
/// Configuration for the SPH fluid simulation.
/// All parameters can be tuned at runtime via UI.
/// </summary>
public class SimulationConfig
{
    public int ParticleSize { get; set; }
    public float FluidDensity { get; set; }
    public float Stiffness { get; set; }
    public float Viscosity { get; set; }
    public float TimeStep { get; set; }
    public int SpatialHashQueryRadius => ParticleSize * 2;
    public float ScaledParticleDiameter2 => 0.01f * (ParticleSize * ParticleSize);
    public Vector3 Gravity { get; set; }

    public static SimulationConfig Default =>
        new SimulationConfig
        {
            ParticleSize = 1,
            FluidDensity = 1f,
            Stiffness = 100f,
            Viscosity = 2f,
            TimeStep = 0.05f,
            Gravity = new Vector3(0, -0.05f, 0),
        };
}
