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
    public float FluidInitVolume { get; private set; }
    public float FluidVolume { get; private set; }
    public float CompressionError { get; private set; }
    public float AbsError { get; private set; }
    public float Cfl { get; private set; }
    public float MaxVelocity { get; private set; }
    public float AvgVelocity { get; private set; }
    public int IterationCount { get; private set; }

    private const int CoolDown = 50;
    private double _currentCoolDown = CoolDown;

    private readonly ComponentPool<MaterialComponent> _fluidPool =
        world.Components.GetOrCreatePool<MaterialComponent>();
    private readonly ComponentPool<KinematicState> _movementPool =
        world.Components.GetOrCreatePool<KinematicState>();

    public void Collect(double elapsedMilliseconds)
    {
        _currentCoolDown -= elapsedMilliseconds;
        if (_currentCoolDown > 0)
            return;
        _currentCoolDown = CoolDown;

        var fluidEntityCollection = world.TypeTracker.GetEntitiesWith<FluidTag>();

        FluidMass = FluidVolume = FluidInitVolume = 0;
        foreach (var entity in fluidEntityCollection)
        {
            ref var fluid = ref _fluidPool.Get(entity.Id);
            FluidMass += fluid.Mass;
            FluidInitVolume += fluid.RestVolume;
            FluidVolume += fluid.Volume;
        }

        EntityCount = _fluidPool.Count;
        AbsError = CompressionError = 0;
        foreach (var entity in fluidEntityCollection)
        {
            ref var fluid = ref _fluidPool.Get(entity.Id);
            var error = (fluid.RestVolume - fluid.Volume) / fluid.RestVolume;
            CompressionError += float.Max(error, 0);
            AbsError += float.Abs(error);
        }
        AbsError /= EntityCount;
        CompressionError /= EntityCount;

        var velocityComponentsSpan = _movementPool.AsSpan();
        AvgVelocity = MaxVelocity = 0;
        foreach (var velocityComponent in velocityComponentsSpan)
        {
            var velocity = velocityComponent.Velocity.Length();
            AvgVelocity += velocity;
            if (velocity < MaxVelocity)
                continue;
            MaxVelocity = velocity;
        }
        AvgVelocity /= EntityCount;
        Cfl = simConfig.TimeStep * MaxVelocity / simConfig.MaxParticleSize;
        IterationCount = Sph.Passes.IiPressurePass.LastIterationCount;
    }
}
