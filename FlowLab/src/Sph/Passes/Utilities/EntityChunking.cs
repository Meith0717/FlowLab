// EntityChunking.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System;
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

    public void ParallelForEach(Action<int, int, int> chunkAction)
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
                chunkAction(start, end, i);
            }
        );
    }

    public void ParallelForEach<TLocal>(
        Func<TLocal> localInit,
        Func<int, int, TLocal, TLocal> chunkAction,
        Action<TLocal> localFinally
    )
    {
        var entities = Entities;
        var chunkSize = size;
        var numChunks = _numChunks;

        Parallel.For(
            0,
            numChunks,
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

    public void ParallelForEach<TLocal>(
        Func<TLocal> localInit,
        Func<int, int, int, TLocal, TLocal> chunkAction,
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
                return chunkAction(start, end, i, localState);
            },
            localFinally
        );
    }
}
