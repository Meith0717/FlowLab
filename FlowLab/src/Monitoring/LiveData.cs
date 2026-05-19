// LiveData.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;

namespace FlowLab.Monitoring;

public class LiveData(World world, Config.SimConfig simConfig)
{
    public int EntityCount { get; private set; }
    public float FluidMass { get; private set; }
    public float FluidVolume { get; private set; }
    public float CompressionError { get; private set; }
    public float AbsError { get; private set; }
    public float Cfl { get; private set; }
    public float MaxVelocity { get; private set; }
    public float AvgVelocity { get; private set; }

    private const int CoolDown = 50;
    private double _currentCoolDown = CoolDown;

    public void Collect(double elapsedMilliseconds)
    {
        _currentCoolDown -= elapsedMilliseconds;
        if (_currentCoolDown > 0)
            return;

        _currentCoolDown = CoolDown;

        EntityCount = world.EntityCount;
        var fluidPool = world.Components.GetOrCreatePool<FluidComponent>();
        var fluidEntities = world.TypeTracker.GetEntitiesWith<FluidTag>();
        FluidMass = FluidVolume = 0;
        foreach (var entity in fluidEntities)
        {
            ref var fluid = ref fluidPool.Get(entity.Id);
            FluidMass += fluid.Mass;
            FluidVolume += fluid.Mass / fluid.Density;
        }

        var fluidComponentsSpan = fluidPool.AsSpan();
        AbsError = CompressionError = 0;
        foreach (var fluidComponent in fluidComponentsSpan)
        {
            var error = (fluidComponent.Density - simConfig.FluidDensity) / simConfig.FluidDensity;
            CompressionError += float.Max(error, 0);
            AbsError += float.Abs(error);
        }
        AbsError /= EntityCount;
        CompressionError /= EntityCount;

        var velocityPool = world.Components.GetOrCreatePool<Velocity3D>();
        var velocityComponentsSpan = velocityPool.AsSpan();
        AvgVelocity = MaxVelocity = 0;
        foreach (var velocityComponent in velocityComponentsSpan)
        {
            var velocity = velocityComponent.LinearVelocity.Length();
            AvgVelocity += velocity;
            if (velocity < MaxVelocity)
                continue;
            MaxVelocity = velocity;
        }
        AvgVelocity /= EntityCount;
        Cfl = simConfig.TimeStep * MaxVelocity / simConfig.ParticleSize;
    }
}
