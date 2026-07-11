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
    public readonly List<Entity> Neighbours = [];
    public int FluidNeighbourCount;
    public int NeighboursCount => Neighbours.Count;

    public readonly List<float> CachedKernels = [];
    public readonly List<Vector3> CachedNablaKernels = [];

    public void Clear()
    {
        Neighbours.Clear();
        CachedKernels.Clear();
        CachedNablaKernels.Clear();
    }
}
