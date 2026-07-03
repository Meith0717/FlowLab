// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Linq;
using FlowLab.Config;
using FlowLab.Ecs.Tags;
using FlowLab.Monitoring;
using FlowLab.Sph;
using FlowLab.Sph.Passes;
using FlowLab.Sph.Passes.Utilities;
using MonoKit.Ecs;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

public class SimulationSystem(
    EcsSpatialHash3D spatialHash3D,
    Kernels kernels,
    SimConfig config,
    SimulationController controller,
    SimulationTracker simulationTracker
) : ISystem
{
    public int Priority => 1;
    private readonly SphPassContext _context = new();
    private EntityTypeTracker _entityTypeTracker;

    public void Initialize(World world)
    {
        _entityTypeTracker = world.TypeTracker;
        _context.Initialize(world.Components);
        var allSet = _entityTypeTracker.GetEntitiesWith<ParticleTag>().ToArray();
        var allChunking = new EntityChunking(allSet);
        VolumePass.RunForEach(allChunking, spatialHash3D, _context, kernels, config);
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

        var allSet = _entityTypeTracker.GetEntitiesWith<ParticleTag>().ToArray();
        var allChunk = new EntityChunking(allSet);

        var fSet = _entityTypeTracker.GetEntitiesWith<FluidTag>().ToArray();
        var fChunk = new EntityChunking(fSet);

        var bSet = _entityTypeTracker.GetEntitiesWith<BoundaryTag>().ToArray();
        var bChunk = new EntityChunking(bSet);

        VolumePass.RunForEach(allChunk, spatialHash3D, _context, kernels, config);
        NonPressureAccelerationPass.RunForEach(fChunk, _context, config);
        IiPressurePass.RunForEach(fChunk, bChunk, _context, config);
        PressureExtrapolationPass.RunForEach(bChunk, _context, config);
        PressureAccelerationPass.RunForEach(fChunk, _context, config);
        PositionUpdatePass.RunForEach(fChunk, _context, config);

        simulationTracker.Step();
    }
}
