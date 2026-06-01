// ParallelConfig.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Threading.Tasks;

namespace FlowLab.Sph.Passes.Utilities;

public static class ParallelConfig
{
    public static readonly ParallelOptions Options = new()
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount,
    };
}
