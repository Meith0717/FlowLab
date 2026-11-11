// FlowLab.Core - ParticleProperties.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

namespace FlowLab.Core.Components;

public struct ParticleProperties(float particleSize, float restDensity)
{
    public float Pressure;
    public float DiagonalElement;
    public float SourceTherm;
    public float Laplacian;
    public float ParticleDensity;

    public float Size => particleSize;
    public float RestDensity => restDensity;
    public float Density => Mass * ParticleDensity;
    public float Mass => (particleSize * particleSize * particleSize) * RestDensity;
    public float DensityError => (Density - RestDensity) / RestDensity;
}
