// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

using System;
using FlowLab.Config;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using FlowLab.Sph.Passes;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs;
using MonoKit.Ecs.Entities;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

public class SimulationSystem(
    ISpatialGrid3D spatialHash3D,
    Kernels kernels,
    SimConfig config,
    SimulationController controller
) : ISystem
{
    public int Priority => 1;
    private readonly SphPassContext _context = new();
    private EntityTypeTracker _tracker;

    // Reusable entity arrays - materialized once per frame
    private Entity[] _allEntities = Array.Empty<Entity>();
    private Entity[] _fluidEntities = Array.Empty<Entity>();
    private Entity[] _boundaryEntities = Array.Empty<Entity>();

    private const int ChunkSize = 512;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _context.Initialize(world.Components);

        MaterializeEntities();
        var boundaryChunking = new EntityChunking(_boundaryEntities, ChunkSize);
        BoundaryPass.RunForEach(boundaryChunking, spatialHash3D, _context, kernels, config);
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeContainer,
        InputHandler inputHandler
    )
    {
        if (controller.IsPaused)
            return;

        MaterializeEntities();

        var allChunking = new EntityChunking(_allEntities, ChunkSize);
        var fluidChunking = new EntityChunking(_fluidEntities, ChunkSize);

        NeighboursAndDensityPass.RunForEach(allChunking, spatialHash3D, _context, kernels, config);
        NonPressureAccelerationPass.RunForEach(fluidChunking, _context, config);
        IiPressurePass.RunForEach(fluidChunking, _context, config);
        PressureAccelerationPass.RunForEach(fluidChunking, _context, config);
        PositionUpdatePass.RunForEach(fluidChunking, _context, config);
    }

    private void MaterializeEntities()
    {
        // Materialize all particles
        var allSet = _tracker.GetEntitiesWith<ParticleTag>();
        if (allSet.Count > _allEntities.Length)
            _allEntities = new Entity[allSet.Count];
        var idx = 0;
        foreach (var e in allSet)
            _allEntities[idx++] = e;

        // Materialize fluid particles
        var fluidSet = _tracker.GetEntitiesWith<FluidTag>();
        if (fluidSet.Count > _fluidEntities.Length)
            _fluidEntities = new Entity[fluidSet.Count];
        idx = 0;
        foreach (var e in fluidSet)
            _fluidEntities[idx++] = e;

        // Materialize boundary particles
        var boundarySet = _tracker.GetEntitiesWith<BoundaryTag>();
        if (boundarySet.Count > _boundaryEntities.Length)
            _boundaryEntities = new Entity[boundarySet.Count];
        idx = 0;
        foreach (var e in boundarySet)
            _boundaryEntities[idx++] = e;
    }
}
