// NeighbourList.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using MonoKit.Ecs.Entities;

namespace FlowLab.Ecs.Components;

public struct NeighbourList()
{
    public List<Entity> Neighbours = [];

    public void Clear()
    {
        Neighbours.Clear();
    }
}
