// NeighbourList.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoKit.Ecs.Entities;

namespace FlowLab.Ecs.Components;

public struct NeighbourList()
{
    public List<Entity> Neighbours = [];
    public List<CachedKernel> CachedKernels = [];

    public void Clear()
    {
        Neighbours.Clear();
        CachedKernels.Clear();
    }
}

public struct CachedKernel
{
    public float CubicSpline;
    public Vector3 NablaCubicSpline;
}
