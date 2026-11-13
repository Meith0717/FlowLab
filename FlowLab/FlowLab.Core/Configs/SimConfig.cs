// FlowLab.Core - SimConfig.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace FlowLab.Core.Configs;

[Serializable]
public class SimConfig(float maxParticleSize)
{
    public readonly float MaxParticleSize = maxParticleSize;
    public readonly float SpatialHashQueryRadius = maxParticleSize * 2f;
    public readonly float ScaledParticleDiameter2 = 0.01f * (maxParticleSize * maxParticleSize);

    public float FViscosity { get; set; } = 1f;
    public float BViscosity { get; set; } = 1f;
    public float TimeStep { get; set; } = .005f;
    public float Gravity { get; set; } = 9.81f;
    public int MaxIterations { get; set; } = 100;
    public int MinIterations { get; set; } = 2;
    public double MinDensityError { get; set; } = 0.1f;
    public float Stiffness { get; set; } = 50000f;
}
