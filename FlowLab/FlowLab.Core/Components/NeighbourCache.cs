// FlowLab.Core - NeighbourList.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;

namespace FlowLab.Core.Components;

public struct NeighbourCache()
{
    public readonly List<float> CachedKernels = [];
    public readonly List<Vector3> CachedNablaKernels = [];

    public void Clear()
    {
        CachedKernels.Clear();
        CachedNablaKernels.Clear();
    }
}
