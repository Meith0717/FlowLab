// SimulationSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Diagnostics;
using System.Threading.Tasks;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using FlowLab.Sph.Passes;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Querying;
using MonoKit.Ecs.Systems;
using MonoKit.Spatial;

namespace FlowLab.Ecs.System;

public class SimulationSystem(
    EcsSpatialHash3D spatialHash3D,
    float particleSize,
    float fluidDensity,
    float stiffness,
    float viscosity,
    float timeStep
) : ISystem
{
    public int Priority => 1;
    private readonly Kernels _kernels = new Kernels(particleSize);
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

        DensityPass.Compute(fluidEntities, spatialHash3D, _kernels, _context);

        NonPressurePass.Compute(
            fluidEntities,
            _kernels,
            _context,
            particleSize,
            viscosity,
            timeStep
        );

        WcPressurePass.Compute(fluidEntities, _context, stiffness, fluidDensity);

        PressurePass.Compute(fluidEntities, _kernels, _context, timeStep);

        foreach (var entity in fluidEntities)
        {
            ref var transform = ref _context.TransformPool.Get(entity.Id);
            ref var velocity = ref _context.VelocityPool.Get(entity.Id);
            transform.Position += velocity.LinearVelocity * timeStep;
        }
    }
}
