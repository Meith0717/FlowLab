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
    public float CompressionError { get; private set; }
    public float Cfl { get; private set; }
    public int IterationCount { get; private set; }
    public int SimulationSteps { get; private set; }
    public float TimeSteps { get; private set; }
    public double RealTimeSeconds { get; private set; }
    public double PressureSolverTime { get; private set; }
    public double SimStepTime { get; private set; }

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
        CompressionError = 0;
        Cfl = 0;
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

        EntityCount = fluidEntityCollection.Count;
        foreach (var entity in fluidEntityCollection)
        {
            ref var particleProperties = ref _particlePropertiesPool.Get(entity.Id);
            ref var diagnosticComponent = ref _diagnosticPool.Get(entity.Id);
            CompressionError += float.Max(0, particleProperties.DensityError);
            Cfl = float.Max(Cfl, diagnosticComponent.Cfl);
        }
        CompressionError /= EntityCount;
        IterationCount = Sph.Passes.IiPressurePass.LastIterationCount;
    }

    public void FluidStep(double pressureSolverTime, double stepTime)
    {
        SimulationSteps++;
        TimeSteps += config.TimeStep;
        PressureSolverTime = pressureSolverTime;
        SimStepTime = stepTime;
    }

    public void RigidStep(double stepTime)
    {
        SimStepTime += stepTime;
    }

    public void UpdateRealTime(double elapsedMilliseconds)
    {
        var elapsedSeconds = elapsedMilliseconds / 1000d;
        RealTimeSeconds += elapsedSeconds;
    }

    public void Reset()
    {
        SimulationSteps = 0;
        RealTimeSeconds = 0;
        TimeSteps = 0;
    }
}
