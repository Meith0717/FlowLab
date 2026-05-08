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

public class SimulationSystem(EcsSpatialHash3D spatialHash3D, SimulationConfig config) : ISystem
{
    public int Priority => 1;
    private readonly Kernels _kernels = new Kernels(SimulationConfig.ParticleSize);
    private readonly SphPassContext _context = new SphPassContext();
    private EntityTypeTracker _tracker;

    public void Initialize(World world)
    {
        _tracker = world.TypeTracker;
        _context.Initialize(world.Components);
    }

    public void Update(double elapsedMs, World world)
    {
        var fluidEntities = _tracker.GetEntitiesWith<FluidTag>();
        DensityPass.Compute(fluidEntities, spatialHash3D, _kernels, _context, config);
        NonPressureAccelerationPass.Compute(fluidEntities, _kernels, _context, config);
        WcPressurePass.Compute(fluidEntities, _context, config);
        PressureAccelerationPass.Compute(fluidEntities, _kernels, _context, config);

        foreach (var entity in fluidEntities)
        {
            ref var transform = ref _context.TransformPool.Get(entity.Id);
            ref var velocity = ref _context.VelocityPool.Get(entity.Id);
            transform.Position += velocity.LinearVelocity * config.TimeStep;
        }
    }
}
