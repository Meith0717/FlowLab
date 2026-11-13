// FlowLab.Core - CachingAndDensitySystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using FlowLab.Core.Configs;
using FlowLab.Core.Kernels;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;
using MonoKit.Game.Spatial;

namespace FlowLab.Core.Systems;

/// <summary>
/// Does precomputation like caching neighbors and kernel results
/// </summary>
/// <param name="config"></param>
public class CachingAndDensitySystem(SimConfig config) : IUpdateSystem
{
    private readonly SpatialHash _spatialHash = new(config.SpatialHashQueryRadius);
    private readonly CubicSpline _kernel = new(config.MaxParticleSize);

    public void Initialize(World world) { }

    public void Update(double elapsedMs, World world)
    {
        _spatialHash.Rebuild<Components.Kinematics>(world, static l => l.Position);

        var kinematicsColumn = world.GetComponentColumn<Components.Kinematics>();
        var boundaryColumn = world.GetComponentColumn<Tags.BoundaryTag>();

        world.ForEachParallel(
            (
                Entity _,
                ref Components.Kinematics eKinematics,
                ref Components.Properties properties,
                ref Components.ComputationCache cache
            ) =>
            {
                cache.Clear();
                _spatialHash.Query(
                    eKinematics.Position,
                    config.SpatialHashQueryRadius,
                    cache.Neighbours
                );

                var fluidNeighbourCout = 0;
                for (var i = 0; i < cache.Neighbours.Count; i++)
                {
                    if (boundaryColumn.Has(cache.Neighbours[i]))
                        continue;

                    (cache.Neighbours[i], cache.Neighbours[fluidNeighbourCout]) = (
                        cache.Neighbours[fluidNeighbourCout],
                        cache.Neighbours[i]
                    );
                    fluidNeighbourCout++;
                }
                cache.FluidNeighbourCount = fluidNeighbourCout;

                properties.ParticleDensity = 0;
                for (var i = 0; i < cache.Neighbours.Count; i++)
                {
                    var nE = cache.Neighbours[i];
                    ref var nKinematics = ref kinematicsColumn.GetRef(nE);

                    cache.Kernels.Add(_kernel.Value(eKinematics.Position, nKinematics.Position));
                    cache.GradientKernels.Add(
                        _kernel.Gradient(eKinematics.Position, nKinematics.Position)
                    );

                    properties.ParticleDensity += cache.Kernels[i];
                }
            }
        );

        world.ForEachParallel(
            (Entity _, ref Tags.BoundaryTag _, ref Components.Properties properties) =>
            {
                var volume = 0.7f / properties.ParticleDensity;
                properties.ParticleDensity = 1f / volume;
            }
        );
    }

    public void Dispose() { }
}
