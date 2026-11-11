// FlowLab.Core - SimulationSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.Numerics;
using FlowLab.Core.Components;
using FlowLab.Core.Components.Tags;
using FlowLab.Core.Kernels;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;

namespace FlowLab.Core.Systems;

public class SimulationSystem(ISphKernel kernel) : IUpdateSystem
{
    public void Initialize(World world)
    {
        throw new NotImplementedException();
    }

    public void Update(double elapsedMs, World world)
    {
        var spatial = world.SpatialQueries<KinematicState>();
        spatial.BeginFrame(world, 1, static k => k.Position);

        var kinematicsCache = spatial.GetComponentCache<KinematicState>();
        var propertiesCache = spatial.GetComponentCache<ParticleProperties>();
        var boundaryCache = spatial.GetComponentCache<BoundaryTag>();

        //------------------------------------------------------------------------------------------------- Density Pass
        // Clear all particle densities
        world.ForEachParallel(
            (Entity e, ref ParticleProperties particleProperties) =>
            {
                particleProperties.ParticleDensity = 0;
            }
        );

        // Compute particle densities
        spatial.ForEachSelfNeighborRawParallel(
            2,
            (
                (i, self, j, neighbor) =>
                {
                    var selfIsBoundary = boundaryCache.Has(i);
                    var neighbourIsBoundary = boundaryCache.Has(j);

                    if ((!selfIsBoundary || !neighbourIsBoundary) && selfIsBoundary)
                        return;

                    ref var sKinematic = ref kinematicsCache.GetRef(i);
                    ref var nKinematic = ref kinematicsCache.GetRef(j);
                    ref var sProperties = ref propertiesCache.GetRef(i);

                    sProperties.ParticleDensity += kernel.Value(
                        sKinematic.Position,
                        nKinematic.Position
                    );
                }
            )
        );

        // scale boundary particle density
        world.ForEachParallel(
            (Entity e, ref BoundaryTag _, ref ParticleProperties particleProperties) =>
            {
                var volume = 0.7f / particleProperties.ParticleDensity;
                particleProperties.ParticleDensity = 1f / volume;
            }
        );

        //-------------------------------------------------------------------------------------------------- Gravitation
        world.ForEachParallel(
            (Entity e, ref BoundaryTag _, ref ParticleProperties particleProperties) =>
            {
                var volume = 0.7f / particleProperties.ParticleDensity;
                particleProperties.ParticleDensity = 1f / volume;
            }
        );

        //---------------------------------------------------------------------------------------------------- Viscosity
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
