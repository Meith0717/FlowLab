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
    public float SpatialHashQueryRadius => ParticleSize * 2f;
    public float ScaledParticleDiameter2 => 0.01f * (ParticleSize * ParticleSize);

    public float MaxCfl { get; set; }
    public float FViscosity { get; set; }
    public float BViscosity { get; set; }
    public float TimeStep { get; set; }
    public float TimeStepSquared => TimeStep * TimeStep;
    public float Gravity { get; set; }
    public int MaxIterations { get; set; }
    public double MinVolumeError { get; set; }

    public static SimConfig Default =>
        new(1, 1)
        {
            MaxCfl = 0.4f,
            FViscosity = .5f,
            BViscosity = 0,
            TimeStep = 0.03f,
            Gravity = 0.5f,
            MaxIterations = 100,
            MinVolumeError = 0.1f,
        };
}
