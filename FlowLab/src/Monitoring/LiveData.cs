// LiveData.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;

namespace FlowLab.Monitoring;

public class LiveData(World world, SimulationConfig config)
{
    public int EntityCount { get; private set; }
    public float CompressionError { get; private set; }
    public float TotalError { get; private set; }
    public float Cfl { get; private set; }

    public void Collect()
    {
        EntityCount = world.EntityCount;

        var fluidPool = world.Components.GetOrCreatePool<FluidComponent>();
        var fluidComponentsSpan = fluidPool.AsSpan();
        foreach (var fluidComponent in fluidComponentsSpan)
        {
            TotalError =
                (fluidComponent.Density - SimulationConfig.FluidDensity)
                / SimulationConfig.FluidDensity;
            CompressionError = float.Max(TotalError, 0);
        }
        TotalError /= EntityCount;
        CompressionError /= EntityCount;

        var velocityPool = world.Components.GetOrCreatePool<Velocity3D>();
        var velocityComponentsSpan = velocityPool.AsSpan();
        var maxVelocity = 0f;
        foreach (var velocityComponent in velocityComponentsSpan)
        {
            var velocity = velocityComponent.LinearVelocity.Length();
            if (velocity < maxVelocity)
                continue;
            maxVelocity = velocity;
        }

        Cfl = config.TimeStep * (maxVelocity / SimulationConfig.ParticleSize);
    }
}
