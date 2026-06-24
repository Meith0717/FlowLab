// EntityChunking.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MonoKit.Ecs.Entities;

namespace FlowLab.Sph.Passes.Utilities;

/// <summary>
/// Provides static chunked ranges for entities to eliminate TPL partitioning overhead.
/// </summary>
public class EntityChunking(Entity[] entities, int size = 512)
{
    private readonly int _numChunks =
        entities.Length == 0 ? 0 : (int)Math.Ceiling((float)entities.Length / size);

    public Entity[] Entities { get; } = entities;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ParallelForEach(Action<int, int> chunkAction)
    {
        var entities = Entities;
        var chunkSize = size;

        Parallel.For(
            0,
            _numChunks,
            i =>
            {
                var start = i * chunkSize;
                var end = Math.Min(start + chunkSize, entities.Length);
                chunkAction(start, end);
            }
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ParallelForEach<TLocal>(
        Func<TLocal> localInit,
        Func<int, int, TLocal, TLocal> chunkAction,
        Action<TLocal> localFinally
    )
    {
        var entities = Entities;
        var chunkSize = size;

        Parallel.For(
            0,
            _numChunks,
            localInit,
            (i, _, localState) =>
            {
                var start = i * chunkSize;
                var end = Math.Min(start + chunkSize, entities.Length);
                return chunkAction(start, end, localState);
            },
            localFinally
        );
    }
}
