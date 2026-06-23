// MaterialComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace FlowLab.Ecs.Components;

public struct MaterialComponent
{
    private readonly float _restDensity;

    public readonly float ColorId;
    public float RestVolume { get; private set; }
    public float Mass { get; private set; }
    public float Volume { get; set; }

    public MaterialComponent(float colorId, float volume, float restDensity)
    {
        if (colorId is < 0 or > 1)
            throw new ArgumentException("Id needs to be normalized");

        _restDensity = restDensity;
        ColorId = colorId;
        SetRestVolume(volume);
    }

    public void SetRestVolume(float volume)
    {
        RestVolume = volume;
        Mass = volume * _restDensity;
    }
}
