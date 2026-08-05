// ParticleProperties.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;

namespace FlowLab.Ecs.Components;

public struct ParticleProperties
{
    private readonly float _restDensity;
    public readonly Color ParticleColor;

    public float Size { get; private set; }
    public float RestVolume { get; private set; }
    public float Mass { get; private set; }
    public float Volume { get; set; }

    public ParticleProperties(Color particleColor, float volume, float restDensity)
    {
        _restDensity = restDensity;
        ParticleColor = particleColor;
        SetRestVolume(volume);
    }

    public void SetRestVolume(float volume)
    {
        RestVolume = volume;
        Size = float.RootN(volume, 3);
        Mass = volume * _restDensity;
    }
}
