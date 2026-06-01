// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Concurrent;
using FlowLab.Config;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using FlowLab.Sph.Passes;
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
    SimulationController controller
) : ISystem
{
    public int Priority => 1;
    private readonly SphPassContext _context = new();
    private EntityTypeTracker _tracker;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _context.Initialize(world.Components);

        var bPartitioner = Partitioner.Create(_tracker.GetEntitiesWith<BoundaryTag>());
        BoundaryPass.RunForEach(bPartitioner, spatialHash3D, _context, kernels, config);
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

        var allPartitioner = Partitioner.Create(_tracker.GetEntitiesWith<ParticleTag>());
        var fluid = _tracker.GetEntitiesWith<FluidTag>();
        var fPartitioner = Partitioner.Create(fluid);

        NeighboursAndDensityPass.RunForEach(
            allPartitioner,
            spatialHash3D,
            _context,
            kernels,
            config
        );
        NonPressureAccelerationPass.RunForEach(fPartitioner, _context, config);
        IiPressurePass.RunForEach(fPartitioner, fluid.Count, _context, config);
        // WcPressurePass.RunForEach(fPartitioner, _context, config);
        PressureAccelerationPass.RunForEach(fPartitioner, _context, config);
        PositionUpdatePass.RunForEach(fPartitioner, _context, config);
    }
}
