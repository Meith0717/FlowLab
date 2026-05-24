// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

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

        var bEntities = _tracker.GetEntitiesWith<BoundaryTag>();
        BoundaryPass.RunForEach(bEntities, spatialHash3D, _context, simConfig);
    }

    public void Update(double elapsedMs, World world)
    {
        var allEntities = _tracker.GetEntitiesWith<ParticleTag>();
        var fEntities = _tracker.GetEntitiesWith<FluidTag>();

        DensityPass.RunForEach(allEntities, spatialHash3D, _context, simConfig);
        NonPressureAccelerationPass.RunForEach(fEntities, _context, simConfig);
        IiPressurePass.RunForEach(fEntities, _context, simConfig);
        // WcPressurePass.Run(fEntities, _context, simConfig);
        PressureAccelerationPass.RunForEach(fEntities, _context, simConfig);
        PositionUpdatePass.RunForEach(fEntities, _context, simConfig);
    }
}
