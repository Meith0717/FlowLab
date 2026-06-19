// FluidComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;

namespace FlowLab.Ecs.Components;

public struct MaterialComponent
{
    public float Volume;
    public float RestVolume;
    public readonly float ColorId;
    private readonly float _restDensity;

    public MaterialComponent(float colorId, float volume, float restDensity)
    {
        if (colorId is < 0 or > 1)
            throw new ArgumentException("Id needs to be normalized");
            
        ColorId = colorId;
        _restDensity = restDensity;
        Volume = volume;
        RestVolume = volume;
        
    }

    public readonly float Mass => RestVolume * _restDensity;
}
