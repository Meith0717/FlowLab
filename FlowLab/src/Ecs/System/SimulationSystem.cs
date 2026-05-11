// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using FlowLab.Sph.Passes;
using MonoKit.Ecs;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

public class SimulationSystem(ISpatialGrid3D spatialHash3D, Kernels kernels, SimulationConfig config) : ISystem
{
    public int Priority => 1;
    private readonly SphPassContext _context = new();
    private EntityTypeTracker _tracker;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _context.Initialize(world.Components);
    }

    public void Update(double elapsedMs, World world)
    {
        var fEntities = _tracker.GetEntitiesWith<FluidTag>();
        var bEntities = _tracker.GetEntitiesWith<BoundaryTag>();
        
        BoundaryPass.Compute(bEntities, kernels, spatialHash3D, _context, config);
        DensityPass.Compute(fEntities, spatialHash3D, kernels, _context, config);
        NonPressureAccelerationPass.Compute(fEntities, kernels, _context, config);
        WcPressurePass.Compute(fEntities, _context, config);
        PressureAccelerationPass.Compute(fEntities, kernels, _context, config);

        foreach (var entity in fEntities)
        {
            ref var transform = ref _context.TransformPool.Get(entity.Id);
            ref var velocity = ref _context.VelocityPool.Get(entity.Id);
            var pos = transform.Position.ToNumerics();
            var vel = velocity.LinearVelocity.ToNumerics();
            transform.Position = (pos + vel * config.TimeStep).ToXna();
        }
    }
}
