// SimConfig.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

namespace FlowLab.Config;

public class SimConfig(float maxParticleSize, float fluidDensity)
{
    public const int MaxParticles = 1_000_000;
    public const float Relaxation = .5f;

    public readonly float MaxParticleSize = maxParticleSize;
    public readonly float VolumeBeta = .15f * (maxParticleSize * maxParticleSize * maxParticleSize);
    public readonly float SpatialHashQueryRadius = maxParticleSize * 2f;
    public readonly float TensionEpsilon = 1e-3f / maxParticleSize;
    public readonly float ScaledParticleDiameter2 = 0.01f * (maxParticleSize * maxParticleSize);

    public float MaxCfl { get; set; }
    public float FViscosity { get; set; }
    public float BViscosity { get; set; }
    public float TimeStep { get; set; }
    public float TimeStepSquared => TimeStep * TimeStep;
    public float Gravity { get; set; }
    public int MaxIterations { get; set; }
    public double MinVolumeError { get; set; }
    public float InterfaceTension { get; set; }

    public static SimConfig Default =>
        new(1, 1)
        {
            MaxCfl = .4f,
            FViscosity = 1f,
            BViscosity = 0,
            TimeStep = 0.05f,
            Gravity = 0.5f,
            MaxIterations = 100,
            MinVolumeError = .1f,
            InterfaceTension = 0f,
        };
}
