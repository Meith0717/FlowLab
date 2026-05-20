// SimulationScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.System;
using FlowLab.Ecs.Tags;
using FlowLab.Input;
using FlowLab.Monitoring;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Gameplay;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Screens;
using MonoKit.Spatial;

namespace FlowLab.Screens;

public class SimulationScreen : Screen
{
    private readonly SimConfig _simConfig;
    private readonly Camera3D _camera3D;
    private readonly World _world;
    private readonly FluidRenderer _fluidRenderer;
    private readonly GameRuntime3D _simRuntime;
    private readonly LiveData _liveData;
    private readonly SensorPlaneManager _sensorManager;

    public SimulationScreen(GameServiceContainer appServices)
        : base(appServices, false, false)
    {
        _simConfig = SimConfig.Default;
        _simRuntime = new GameRuntime3D(GraphicsDevice, _simConfig.SpatialHashQueryRadius);

        _camera3D = _simRuntime.Services.Get<Camera3D>();
        _camera3D.AddBehaviour(new MoveByMouse());
        _camera3D.AddBehaviour(new ZoomByMouse(.5f));

        _world = _simRuntime.Services.Get<World>();
        _world.Systems.Add(new ParticleTransformSyncSystem());
        var kernels = new Kernels(_simConfig.ParticleSize);
        var spatialHashSystem = _simRuntime.Services.Get<EcsSpatialHash3D>();
        _world.Systems.Add(new SimulationSystem(spatialHashSystem, kernels, _simConfig));

        _fluidRenderer = new FluidRenderer(GraphicsDevice, _world);
        _liveData = new(_world, _simConfig);

        _sensorManager = new SensorPlaneManager(GraphicsDevice);
        var sensorPlane = new SensorPlane(
            _world,
            spatialHashSystem,
            kernels,
            _simConfig,
            new Vector3(0, 25, 0),
            Vector3.UnitX,
            new Size(60, 60),
            120
        );
        _sensorManager.Add("Plane 1", sensorPlane);

        SpawnBox(25, 25, 60, 1f);
    }

    public override void Initialize()
    {
        _fluidRenderer.Initialize();
        ScreenManager.AddScreen(new HudScreen(AppServices, _simConfig, _liveData, _sensorManager));
        base.Initialize();
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        if (inputHandler.HasAction((byte)ActionType.ToggleBoundaryDraw))
            _fluidRenderer.HideBoundary = !_fluidRenderer.HideBoundary;

        if (inputHandler.HasAction((byte)ActionType.DeleteFluid))
            ClearFluid();

        if (inputHandler.HasAction((byte)ActionType.SpawnBlock))
            AddBlueParticle(21, 21, 45);

        _camera3D.Update(elapsedMilliseconds, inputHandler);
        _simRuntime.Update(elapsedMilliseconds, inputHandler);
        _liveData.Collect(elapsedMilliseconds);
        _fluidRenderer.Update();
        _sensorManager.Update();
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _fluidRenderer.Draw(_camera3D);
        _sensorManager.Draw(_camera3D);
        base.Draw(spriteBatch);
    }

    public void ClearFluid()
    {
        var fluidCollection = _world.TypeTracker.GetEntitiesWith<FluidTag>();
        var lifePool = _world.Components.GetOrCreatePool<Lifetime>();
        foreach (var fluidEntity in fluidCollection)
        {
            ref var lifeTime = ref lifePool.Get(fluidEntity.Id);
            lifeTime.DestroyNow = true;
        }
    }

    private void AddBlueParticle(float width, float depth, float height)
    {
        var halfWidth = width / 2;
        var halfDepth = depth / 2;

        for (var x = -halfWidth; x <= halfWidth; x++)
        for (var z = -halfDepth; z <= halfDepth; z++)
        for (var y = 0; y <= height; y++)
            ParticleFactory.CreateFluidParticle(_world, new Vector3(x, y + 1, z), _simConfig);
    }

    private void SpawnBox(float width, float depth, float height, float particleSize)
    {
        Vector3 position;
        var halfWidth = width / 2;
        var halfDepth = depth / 2;

        // Top & Bottom
        for (var i = -halfWidth; i < halfWidth; i += particleSize)
        for (var j = -halfDepth; j < halfDepth; j += particleSize)
        {
            position = new Vector3(i, 0, j);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                _simConfig.FluidDensity
            );
            position = new Vector3(i, height - particleSize, j);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                _simConfig.FluidDensity
            );
        }

        for (var i = -halfWidth; i < halfWidth; i += particleSize)
        for (var j = 0f; j < height; j += particleSize)
        {
            position = new Vector3(i, j, -halfDepth);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                _simConfig.FluidDensity
            );
            position = new Vector3(i, j, halfDepth - particleSize);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                _simConfig.FluidDensity
            );
        }

        for (var i = -halfDepth; i < halfDepth; i += particleSize)
        for (var j = 0f; j < height; j += particleSize)
        {
            position = new Vector3(-halfWidth, j, i);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                _simConfig.FluidDensity
            );
            position = new Vector3(halfWidth - particleSize, j, i);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                _simConfig.FluidDensity
            );
        }
    }

    public override void Dispose()
    {
        _fluidRenderer.Dispose();
        _sensorManager.Dispose();
        base.Dispose();
    }
}
