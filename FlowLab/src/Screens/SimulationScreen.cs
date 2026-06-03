// SimulationScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Input;
using FlowLab.Monitoring;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoKit.Ecs;
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
    private readonly SimulationController _simController;
    private readonly SimulationTracker _simTracker;

    public SimulationScreen(GameServiceContainer appServices)
        : base(appServices, false, false)
    {
        _simConfig = SimConfig.Default;
        _simRuntime = new GameRuntime3D(GraphicsDevice, _simConfig.SpatialHashQueryRadius);
        _world = _simRuntime.Services.Get<World>();
        _simController = new SimulationController(_world);
        _simTracker = new SimulationTracker(_simConfig);
        _camera3D = _simRuntime.Services.Get<Camera3D>();
        _camera3D.AddBehaviour(new MoveByMouse());
        _camera3D.AddBehaviour(new ZoomByMouse(.5f));

        _world.Systems.Add(new ParticleTransformSyncSystem());
        var kernels = new Kernels(_simConfig.ParticleSize);
        var spatialHashSystem = _simRuntime.Services.Get<EcsSpatialHash3D>();
        _world.Systems.Add(
            new SimulationSystem(
                spatialHashSystem,
                kernels,
                _simConfig,
                _simController,
                _simTracker
            )
        );
        _world.Components.Add(_world.WorldEntity, new DebugComponent());

        _fluidRenderer = new FluidRenderer(
            GraphicsDevice,
            _world,
            spatialHashSystem,
            _simConfig.SpatialHashQueryRadius
        );
        _liveData = new LiveData(_world, _simConfig);

        _sensorManager = new SensorPlaneManager(GraphicsDevice);

        SpawnBox(25, 25, 60, 1f);
    }

    public override void Initialize()
    {
        _fluidRenderer.Initialize();
        ScreenManager.AddScreen(
            new HudScreen(AppServices, _simConfig, _liveData, _simTracker, _sensorManager)
        );
        base.Initialize();
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        _simController.Update(elapsedMilliseconds, inputHandler);

        if (inputHandler.HasAction((byte)ActionType.SpawnBlock))
            AddFluidBlock(12, 12, 100);

        _camera3D.Update(elapsedMilliseconds, inputHandler);
        _simRuntime.Update(elapsedMilliseconds, inputHandler);

        if (!_simController.IsPaused)
            _simTracker.UpdateRealTime(elapsedMilliseconds);

        _liveData.Collect(elapsedMilliseconds);
        _sensorManager.Update(elapsedMilliseconds);
        _fluidRenderer.Update(_simController.HideBoundary);
        _fluidRenderer.ShowSpatialGrids = _simController.ShowSpatialGrids;
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _fluidRenderer.Draw(_camera3D);
        _sensorManager.Draw(_camera3D);
        base.Draw(spriteBatch);
    }

    private void AddFluidBlock(float width, float depth, float height)
    {
        var halfWidth = width / 2;
        var halfDepth = depth / 2;

        for (var x = -halfWidth; x <= halfWidth; x++)
        for (var z = -halfDepth; z <= halfDepth; z++)
        for (var y = 0; y <= height; y++)
            ParticleFactory.CreateFluidParticle(_world, new Vector3(x, y + 10, z), _simConfig);
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
            // position = new Vector3(i, height - particleSize, j);
            // ParticleFactory.CreateBoundaryParticle(
            //     _world,
            //     position,
            //     particleSize,
            //     _simConfig.FluidDensity
            // );
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
