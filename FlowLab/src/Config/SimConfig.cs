// SimConfig.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace FlowLab.Config;

[Serializable]
public class SimConfig(float maxParticleSize)
{
    public readonly float MaxParticleSize = maxParticleSize;
    public readonly float SpatialHashQueryRadius = maxParticleSize * 2.1f;
    public readonly float ScaledParticleDiameter2 = 0.01f * (maxParticleSize * maxParticleSize);

    public float FViscosity { get; set; } = 1f;
    public float BViscosity { get; set; } = 1f;
    public float TimeStep { get; set; } = 0.03f;
    public float Gravity { get; set; } = 9.81f;
    public int MaxIterations { get; set; } = 100;
    public double MinDensityError { get; set; } = 0.1f;

    public float Stiffness { get; set; } = 5000f;
}
