// FluidSimulation.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Input;
using FlowLab.Monitoring;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Systems;
using MonoKit.Input;
using MonoKit.Spatial;

namespace FlowLab.Sph;

public class FluidSimulation
{
    public readonly World World;
    public readonly LiveData LiveData;
    public readonly Config.Config Config;

    // Bounds of the particle cube
    private const float MinBound = 2f;
    private const float MaxBound = 23f;

    public FluidSimulation()
    {
        Config = FlowLab.Config.Config.Default;
        var kernels = new Kernels(Config.ParticleSize);
        var spatialHash3D = new EcsSpatialHash3D(Config.SpatialHashQueryRadius, 1_000_000);
        var spatialHashSystem = new SpatialHashSystem3D(spatialHash3D);

        World = new World();
        World.Systems.Add(spatialHashSystem);
        World.Systems.Add(new LifetimeSystem());
        World.Systems.Add(new ParticleTransformSyncSystem());
        World.Systems.Add(new SimulationSystem(spatialHashSystem.Grid, kernels, Config));
        LiveData = new LiveData(World, Config);

        Vector3 position;
        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(i, 0.5f, j);
            ParticleFactory.CreateBoundaryParticle(World, position, Config);
            position = new Vector3(i, 50.5f, j);
            ParticleFactory.CreateBoundaryParticle(World, position, Config);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 50.5f; j++)
        {
            position = new Vector3(i, j, 0.5f);
            ParticleFactory.CreateBoundaryParticle(World, position, Config);
            position = new Vector3(i, j, 24.5f);
            ParticleFactory.CreateBoundaryParticle(World, position, Config);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 50.5f; j++)
        {
            position = new Vector3(0.5f, j, i);
            ParticleFactory.CreateBoundaryParticle(World, position, Config);
            position = new Vector3(24.5f, j, i);
            ParticleFactory.CreateBoundaryParticle(World, position, Config);
        }
    }

    public void Update(double gameTime, InputHandler inputHandler)
    {
        if (inputHandler.HasAction((byte)ActionType.SpawnBlock))
            AddBlueParticle();

        World.Update(gameTime);
        LiveData.Collect(gameTime);
    }

    private void AddBlueParticle()
    {
        var position =
            new Vector3(MinBound + MaxBound / 2, MinBound + MaxBound / 2, MinBound + MaxBound / 2)
            + new Vector3(0, 0, 0);

        for (var x = -10; x <= 10; x++)
        for (var y = -10; y <= 35; y++)
        for (var z = -10; z <= 10; z++)
            ParticleFactory.CreateFluidParticle(World, position + new Vector3(x, y, z), Config);
    }
}
