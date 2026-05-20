// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Threading.Tasks;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using FlowLab.Sph.Passes;
using FlowLab.Sph.Passes.Utilities;
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
        Helper.ForEach(
            simConfig.UseParallel,
            bEntities,
            e => BoundaryPass.ComputeEntity(e, spatialHash3D, _context, simConfig)
        );
    }

    public void Update(double elapsedMs, World world)
    {
        var fEntities = _tracker.GetEntitiesWith<FluidTag>();
        Helper.ForEach(
            simConfig.UseParallel,
            fEntities,
            e => DensityPass.ComputeEntity(e, spatialHash3D, _context, simConfig)
        );

        Helper.ForEach(
            simConfig.UseParallel,
            fEntities,
            e => NonPressureAccelerationPass.ComputeEntity(e, _context, simConfig)
        );

        Helper.ForEach(
            simConfig.UseParallel,
            fEntities,
            e => WcPressurePass.ComputeEntity(e, _context, simConfig)
        );

        Helper.ForEach(
            simConfig.UseParallel,
            fEntities,
            e => PressureAccelerationPass.ComputeEntity(e, _context, simConfig)
        );

        Helper.ForEach(
            simConfig.UseParallel,
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
