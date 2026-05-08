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
    public const int ParticleSize = 1;
    public const float FluidDensity = 1;
    public static int SpatialHashQueryRadius => ParticleSize * 2;
    public static float ScaledParticleDiameter2 => 0.01f * (ParticleSize * ParticleSize);

    public float Stiffness { get; set; }
    public float Viscosity { get; set; }
    public float TimeStep { get; set; }
    public Vector3 Gravity { get; set; }
    public bool UseParallel { get; set; }

    public static SimulationConfig Default =>
        new SimulationConfig
        {
            Stiffness = 100f,
            Viscosity = 2f,
            TimeStep = 0.05f,
            Gravity = new Vector3(0, -0.05f, 0),
            UseParallel = false,
        };
}
