// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Concurrent;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using FlowLab.Sph.Passes;
using MonoKit.Ecs;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

public class SimulationSystem(
    ISpatialGrid3D spatialHash3D,
    Kernels kernels,
    Config.SimConfig simConfig
) : ISystem
{
    public int Priority => 1;
    private readonly SphPassContext _context = new();
    private EntityTypeTracker _tracker;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _context.Initialize(world.Components, kernels);

        var bPartitioner = Partitioner.Create(_tracker.GetEntitiesWith<BoundaryTag>());
        BoundaryPass.RunForEach(bPartitioner, spatialHash3D, _context, simConfig);
    }

    public void Update(double elapsedMs, World world)
    {
        var allPartitioner = Partitioner.Create(_tracker.GetEntitiesWith<ParticleTag>());
        var fluid = _tracker.GetEntitiesWith<FluidTag>();
        var fPartitioner = Partitioner.Create(fluid);

        DensityPass.RunForEach(allPartitioner, spatialHash3D, _context, simConfig);
        NonPressureAccelerationPass.RunForEach(fPartitioner, _context, simConfig);
        IiPressurePass.RunForEach(fPartitioner, fluid.Count, _context, simConfig);
        // WcPressurePass.Run(fEntities, _context, simConfig);
        PressureAccelerationPass.RunForEach(fPartitioner, _context, simConfig);
        PositionUpdatePass.RunForEach(fPartitioner, _context, simConfig);
    }
}
