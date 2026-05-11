// FluidSimulation.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Input;
using FlowLab.Monitoring;
using Microsoft.Xna.Framework;
using MonoKit.Core.Diagnostics;
using MonoKit.Ecs;
using MonoKit.Ecs.Systems;
using MonoKit.Input;
using MonoKit.Spatial;

namespace FlowLab.Sph;

public class FluidSimulation
{
    private readonly World _world;
    public readonly LiveData LiveData;
    public readonly SimulationConfig Config;

    // Bounds of the particle cube
    private const float MinBound = 2f;
    private const float MaxBound = 23f;

    public FluidSimulation()
    {
        Config = SimulationConfig.Default;
        var kernels = new Kernels(SimulationConfig.ParticleSize);
        var spatialHash3D = new EcsSpatialHash3D(
            SimulationConfig.SpatialHashQueryRadius,
            1_000_000
        );
        var spatialHashSystem = new SpatialHashSystem3D(spatialHash3D);

        _world = new World();
        _world.Systems.Add(spatialHashSystem);
        _world.Systems.Add(new LifetimeSystem());
        _world.Systems.Add(new ParticleTransformSyncSystem());
        _world.Systems.Add(new SimulationSystem(spatialHashSystem.Grid, kernels, Config));
        LiveData = new LiveData(_world, Config);

        Vector3 position;
        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(i, 0.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, 40.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 40.5f; j++)
        {
            position = new Vector3(i, j, 0.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, j, 24.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 40.5f; j++)
        {
            position = new Vector3(0.5f, j, i);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(24.5f, j, i);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }
    }

    public void Update(double gameTime, InputHandler inputHandler)
    {
        if (inputHandler.HasAction((byte)ActionType.Test))
            AddBlueParticle();

        _world.Update(gameTime);
        LiveData.Collect(gameTime);
    }

    public ParticleShaderData[] InstanceData =>
        _world.Components.GetOrCreatePool<ParticleShaderData>().AsSpan().ToArray();

    private void AddBlueParticle()
    {
        var position =
            new Vector3(MinBound + MaxBound / 2, MinBound + MaxBound / 2, MinBound + MaxBound / 2)
            + new Vector3(0, 0, 0);

        for (var x = -10; x <= 10; x++)
        for (var y = -10; y <= 20; y++)
        for (var z = -10; z <= 10; z++)
            ParticleFactory.CreateFluidParticle(_world, position + new Vector3(x, y, z));
    }
}
