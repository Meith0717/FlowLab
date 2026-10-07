// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using System.Linq;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using FlowLab.Monitoring;
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
    EcsSpatialHash3D spatialHash3D,
    Kernels kernels,
    SimConfig config,
    SimulationController controller,
    Watcher watcher
) : ISystem
{
    private EntityTypeTracker _entityTypeTracker;
    private readonly SphPassContext _context = new();
    private readonly Entity[] _rBuffer = new Entity[2048];
    private readonly Stopwatch _pressureSolverStopwatch = new();
    private readonly Stopwatch _simulationStepStopwatch = new();

    public int Priority => 0;

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

        var allSet = _entityTypeTracker.GetEntitiesWith<ParticleProperties>().ToArray();
        var allChunk = new EntityChunking(allSet);

        var fSet = _entityTypeTracker.GetEntitiesWith<FluidTag>().ToArray();
        var fChunk = new EntityChunking(fSet);

        var bSet = _entityTypeTracker.GetEntitiesWith<BoundaryTag>().ToArray();
        var bChunk = new EntityChunking(bSet);

        _simulationStepStopwatch.Restart();
        DensityPass.RunForEach(allChunk, spatialHash3D, _context, kernels, config);
        NonPressureAccelerationPass.RunForEach(fChunk, _context, config);

        _pressureSolverStopwatch.Restart();
        IiPressurePass.RunForEach(fChunk, bChunk, _context, config);
        _pressureSolverStopwatch.Stop();

        PressureExtrapolationPass.RunForEach(bChunk, _context, config);
        PressureAccelerationPass.RunForEach(fChunk, _context, config);
        PositionUpdatePass.RunForEach(fChunk, _context, config);
        _simulationStepStopwatch.Stop();

        var elapsedPressureSolverTime = _pressureSolverStopwatch.Elapsed.TotalMilliseconds;
        var elapsedStepTime = _simulationStepStopwatch.Elapsed.TotalMilliseconds;
        watcher.FluidStep(elapsedPressureSolverTime, elapsedStepTime);
    }
}
