// Helper.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes.Utilities;

internal static class Helper
{
    internal static void ForEach(
        bool parallel,
        IReadOnlyCollection<Entity> entities,
        Action<Entity> action
    )
    {
        if (parallel)
        {
            Parallel.ForEach(entities, action);
            return;
        }

        foreach (var entity in entities)
            action.Invoke(entity);
    }
}
