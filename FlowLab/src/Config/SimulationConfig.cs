// Config.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Config;

/// <summary>
/// Configuration for the SPH fluid simulation.
/// All parameters can be tuned at runtime via UI.
/// </summary>
public class Config(float particleSize, float fluidDensity)
{
    public readonly float ParticleSize = particleSize;
    public readonly float FluidDensity = fluidDensity;
    public float SpatialHashQueryRadius => ParticleSize * 2f;
    public float ScaledParticleDiameter2 => 0.01f * (ParticleSize * ParticleSize);

    public float Stiffness { get; set; }
    public float Viscosity { get; set; }
    public float TimeStep { get; set; }
    public float Gravity { get; set; }
    public bool UseParallel { get; set; }

    public static Config Default =>
        new Config(1, 1)
        {
            Stiffness = 50f,
            Viscosity = .5f,
            TimeStep = 0.1f,
            Gravity = 0.05f,
            UseParallel = true,
        };
}
