// LiveData.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;

namespace FlowLab.Monitoring;

public class Watcher(World world, SimConfig config)
{
    public int EntityCount { get; private set; }
    public float FluidMass { get; private set; }
    public float FluidInitVolume { get; private set; }
    public float CompressionError { get; private set; }
    public float Cfl { get; private set; }
    public float MaxVelocity { get; private set; }
    public float AvgVelocity { get; private set; }
    public int IterationCount { get; private set; }
    public int SimulationSteps { get; private set; }
    public double SimulationTime { get; private set; } // In Seconds
    public double RealTimeSeconds { get; private set; }

    public Action OnFullTimeStep { get; set; }

    private int _nextSimTime;
    private const int CoolDown = 50;
    private double _currentCoolDown = CoolDown;

    private readonly ComponentPool<ParticleProperties> _particlePropertiesPool =
        world.Components.GetOrCreatePool<ParticleProperties>();
    private readonly ComponentPool<KinematicState> _kinematicPool =
        world.Components.GetOrCreatePool<KinematicState>();
    private readonly ComponentPool<DiagnosticComponent> _diagnosticPool =
        world.Components.GetOrCreatePool<DiagnosticComponent>();

    private void Clear()
    {
        EntityCount = 0;
        FluidMass = 0;
        FluidInitVolume = 0;
        CompressionError = 0;
        Cfl = 0;
        MaxVelocity = 0;
        AvgVelocity = 0;
        IterationCount = 0;
    }

    public void Monitor(double elapsedMilliseconds)
    {
        _currentCoolDown -= elapsedMilliseconds;
        if (_currentCoolDown > 0)
            return;
        _currentCoolDown = CoolDown;

        Clear();
        var fluidEntityCollection = world.TypeTracker.GetEntitiesWith<FluidTag>();

        var restDensitySum = 0f;
        EntityCount = fluidEntityCollection.Count;
        foreach (var entity in fluidEntityCollection)
        {
            ref var particleProperties = ref _particlePropertiesPool.Get(entity.Id);
            ref var kinematicProperties = ref _kinematicPool.Get(entity.Id);
            ref var diagnosticComponent = ref _diagnosticPool.Get(entity.Id);

            restDensitySum += particleProperties.RestDensity;

            FluidMass += particleProperties.Mass;
            CompressionError += float.Max(0, particleProperties.DensityError);

            var velocity = kinematicProperties.Velocity.Length();
            AvgVelocity += velocity;
            MaxVelocity = float.Max(MaxVelocity, velocity);
            Cfl = float.Max(Cfl, diagnosticComponent.Cfl);
        }
        FluidInitVolume = FluidMass / (restDensitySum / EntityCount);
        CompressionError /= EntityCount;
        AvgVelocity /= EntityCount;
        IterationCount = Sph.Passes.IiPressurePass.LastIterationCount;
    }

    public void Step()
    {
        SimulationSteps++;
        SimulationTime += config.TimeStep;
    }

    public void UpdateRealTime(double elapsedMilliseconds)
    {
        var elapsedSeconds = elapsedMilliseconds / 1000d;
        RealTimeSeconds += elapsedSeconds;

        if (SimulationTime < _nextSimTime)
            return;

        _nextSimTime++;
        OnFullTimeStep?.Invoke();
    }

    public void Reset()
    {
        SimulationSteps = 0;
        SimulationTime = 0;
        RealTimeSeconds = 0;
        _nextSimTime = 0;
    }
}
