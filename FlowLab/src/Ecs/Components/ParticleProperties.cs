// ParticleProperties.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;

namespace FlowLab.Ecs.Components;

public struct ParticleProperties
{
    public readonly Color ParticleColor;

    public float Pressure;
    public float DiagonalElement;
    public float SourceTherm;
    public float Laplacian;
    public float ColorId;

    public float Size { get; private set; }
    public float RestDensity { get; }
    public float Density { get; set; }
    public float ParticleDensity { get; set; }
    public float Mass { get; private set; }
    public float DensityError => (Density - RestDensity) / RestDensity;

    public ParticleProperties(Color particleColor, float volume, float restDensity, float colorId)
    {
        ColorId = colorId;
        ParticleColor = particleColor;
        RestDensity = Density = restDensity;
        SetNewVolume(volume);
    }

    public void SetNewVolume(float volume)
    {
        Size = float.RootN(volume, 3);
        Mass = volume * RestDensity;
    }
}
