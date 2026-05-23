// SimConfig.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace FlowLab.Config;

public class SimConfig(float particleSize, float fluidDensity)
{
    public const int MaxParticles = 1_000_000;
    public const float Relaxation = .5f;

    public readonly float ParticleSize = particleSize;
    public readonly float FluidDensity = fluidDensity;
    public float SpatialHashQueryRadius => ParticleSize * 2f;
    public float ScaledParticleDiameter2 => 0.01f * (ParticleSize * ParticleSize);

    public float MaxCfl { get; set; }
    public float Stiffness { get; set; }
    public float Viscosity { get; set; }
    public float TimeStep { get; set; }
    public float Gravity { get; set; }
    public bool UseParallel { get; set; }
    public int MaxIterations { get; set; }
    public double MinDensityError { get; set; }

    public static SimConfig Default =>
        new(1, 1)
        {
            MaxCfl = .4f,
            Stiffness = 50f,
            Viscosity = .5f,
            TimeStep = 0.1f,
            Gravity = 0.0f,
            UseParallel = true,
            MaxIterations = 100,
            MinDensityError = 0.01f,
        };
}
