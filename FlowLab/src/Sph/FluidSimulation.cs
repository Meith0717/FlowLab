// FluidSimulation.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Ecs.Systems;
using MonoKit.Input;
using SpatialHashSystem = FlowLab.Ecs.System.SpatialHashSystem;

namespace FlowLab.Sph;

public class FluidSimulation
{
    private readonly World _world;

    // Bounds of the particle cube
    private const float MinBound = 2f;
    private const float MaxBound = 23f;

    public FluidSimulation()
    {
        var config = SimulationConfig.Default;
        var spatialHashSystem = new SpatialHashSystem(SimulationConfig.SpatialHashQueryRadius);
        _world = new World();
        _world.Systems.Add(spatialHashSystem);
        _world.Systems.Add(new LifetimeSystem());
        _world.Systems.Add(new ParticleTransformSyncSystem());
        _world.Systems.Add(new SimulationSystem(spatialHashSystem.Grid, config));

        Vector3 position;
        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(i, 0.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, -0.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(i, j, 0.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, j, -0.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);

            position = new Vector3(i, j, 24.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, j, 25.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(0.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(-0.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);

            position = new Vector3(24.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(25.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }
        AddBlueParticle();
    }

    public void Update(double gameTime, InputHandler inputHandler)
    {
        if (inputHandler.HasAction((byte)ActionType.Test))
            AddBlueParticle();

        _world.Update(gameTime);
    }

    public ParticleShaderData[] InstanceData =>
        _world.Components.GetOrCreatePool<ParticleShaderData>().AsSpan().ToArray();

    private void AddBlueParticle()
    {
        var position = new Vector3(
            MinBound + MaxBound / 2,
            MinBound + MaxBound / 2,
            MinBound + MaxBound / 2
        );

        for (var x = -10; x <= 10; x++)
        for (var y = -10; y <= 10; y++)
        for (var z = -10; z <= 10; z++)
            ParticleFactory.CreateFluidParticle(_world, position + new Vector3(x, y, z));
    }
}
