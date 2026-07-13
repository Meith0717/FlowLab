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
    public readonly float ColorId;
    public readonly Color Color;

    public float Size { get; private set; }
    public float RestVolume { get; private set; }
    public float Mass { get; private set; }
    public float Volume { get; set; }

    public ParticleProperties(Color color, float colorId, float volume, float restDensity)
    {
        if (colorId is < 0 or > 1)
            throw new ArgumentException("Id needs to be normalized");

        _restDensity = restDensity;
        ColorId = colorId;
        Color = color;
        SetRestVolume(volume);
    }

    public void SetRestVolume(float volume)
    {
        RestVolume = volume;
        Size = float.RootN(volume, 3);
        Mass = volume * _restDensity;
    }
}
