// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.

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
    ISpatialGrid3D spatialHash3D,
    Kernels kernels,
    SimConfig config,
    SimulationController controller,
    SimulationTracker simulationTracker
) : ISystem
{
    public int Priority => 1;
    private readonly SphPassContext _context = new();
    private EntityTypeTracker _entityTypeTracker;

    private const int ChunkSize = 512;

    public void Initialize(World world)
    {
        _entityTypeTracker = world.TypeTracker;
        _context.Initialize(world.Components);
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
        var fluidSet = _entityTypeTracker.GetEntitiesWith<FluidTag>().ToArray();
        var allChunking = new EntityChunking(allSet, ChunkSize);
        var fluidChunking = new EntityChunking(fluidSet, ChunkSize);

        VolumePass.RunForEach(allChunking, spatialHash3D, _context, kernels, config);
        NonPressureAccelerationPass.RunForEach(fluidChunking, _context, config);
        IiPressurePass.RunForEach(fluidChunking, _context, config);
        PressureAccelerationPass.RunForEach(fluidChunking, _context, config);
        PositionUpdatePass.RunForEach(fluidChunking, _context, config);

        simulationTracker.Step();
    }
}
