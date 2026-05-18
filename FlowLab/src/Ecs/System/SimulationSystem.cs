// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Threading.Tasks;
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
        _context.Initialize(world.Components);

        var bEntities = _tracker.GetEntitiesWith<BoundaryTag>();
        BoundaryPass.Compute(bEntities, kernels, spatialHash3D, _context, simConfig);
    }

    public void Update(double elapsedMs, World world)
    {
        var fEntities = _tracker.GetEntitiesWith<FluidTag>();
        DensityPass.Compute(fEntities, spatialHash3D, kernels, _context, simConfig);
        NonPressureAccelerationPass.Compute(fEntities, kernels, _context, simConfig);
        WcPressurePass.Compute(fEntities, _context, simConfig);
        PressureAccelerationPass.Compute(fEntities, kernels, _context, simConfig);

        Parallel.ForEach(
            fEntities,
            entity =>
            {
                ref var transform = ref _context.TransformPool.Get(entity.Id);
                ref var velocity = ref _context.VelocityPool.Get(entity.Id);
                var pos = transform.Position;
                var vel = velocity.LinearVelocity;
                transform.Position = pos + vel * simConfig.TimeStep;
            }
        );
    }
}
