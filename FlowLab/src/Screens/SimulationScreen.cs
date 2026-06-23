// SimulationScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Input;
using FlowLab.Monitoring;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Gameplay;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Screens;
using MonoKit.Spatial;

namespace FlowLab.Screens;

public class SimulationScreen : Screen
{
    private readonly Random _random = new Random();
    private readonly SimConfig _simConfig;
    private readonly Camera3D _camera3D;
    private readonly World _world;
    private readonly FluidRenderer _fluidRenderer;
    private readonly GameRuntime3D _simRuntime;
    private readonly LiveData _liveData;
    private readonly SensorPlaneManager _sensorManager;
    private readonly SimulationController _simController;
    private readonly SimulationTracker _simTracker;
    private readonly BoundingBoxRenderer _boundingBoxRenderer;

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
        var kernels = new Kernels(_simConfig.MaxParticleSize);
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
        var domain = new BoundingBox(new Vector3(-20, -10, -20), new Vector3(20, 100, 20));
        _world.Systems.Add(new DomainSystem(domain));

        _boundingBoxRenderer = new BoundingBoxRenderer(GraphicsDevice, domain);
        _fluidRenderer = new FluidRenderer(
            GraphicsDevice,
            _world,
            spatialHashSystem,
            _simConfig.SpatialHashQueryRadius
        );
        _liveData = new LiveData(_world, _simConfig);

        _sensorManager = new SensorPlaneManager(
            GraphicsDevice,
            _world,
            spatialHashSystem,
            kernels,
            _simConfig
        );

        SpawnBox(22, 22, 100, 1f, 1, 0);
    }

    public override void Initialize()
    {
        _fluidRenderer.Initialize();
        // ScreenManager.AddScreen(
        //     new HudScreen(AppServices, _simConfig, _liveData, _simTracker, _sensorManager)
        // );
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
        {
            AddFluidBlock(20, 20, 20, 1, new Vector3(0, 40, 0), Color.DodgerBlue, 0);
        }

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
        _boundingBoxRenderer.Draw(_camera3D);
        _sensorManager.Draw(_camera3D);
        base.Draw(spriteBatch);
    }

    private void AddFluidBlock(
        float width,
        float depth,
        float height,
        float restDensity,
        Vector3 position,
        Color color,
        float materialId
    )
    {
        var particleSize = _simConfig.MaxParticleSize;
        var halfParticleSize = particleSize / 2f;
        var halfWidth = width / 2;
        var halfDepth = depth / 2;
        var halfHeight = height / 2;

        var startWidth = halfWidth - halfParticleSize;
        var stopWidth = halfWidth + halfParticleSize;
        var startDepth = halfDepth - halfParticleSize;
        var stopDepth = halfDepth + halfParticleSize;
        var startHeight = halfHeight;
        var stopHeight = halfHeight;

        for (var x = -startWidth; x < stopWidth; x += particleSize)
        for (var z = -startDepth; z < stopDepth; z += particleSize)
        for (var y = -startHeight; y < stopHeight; y += particleSize)
            ParticleFactory.CreateFluidParticle(
                _world,
                position + new Vector3(x, y, z),
                particleSize,
                restDensity,
                color,
                materialId
            );
    }

    private void SpawnBox(
        float width,
        float depth,
        float height,
        float particleSize,
        float restDensity,
        float materialId
    )
    {
        Vector3 position;
        var halfParticleSize = particleSize / 2f;
        var halfWidth = width / 2;
        var halfDepth = depth / 2;

        var startWidth = halfWidth - halfParticleSize;
        var stopWidth = halfWidth + halfParticleSize;
        var startDepth = halfDepth - halfParticleSize;
        var stopDepth = halfDepth + halfParticleSize;

        // Top & Bottom
        for (var i = -startWidth; i < stopWidth; i += particleSize)
        for (var j = -startDepth; j < stopDepth; j += particleSize)
        {
            position = new Vector3(i, 0, j);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                restDensity,
                materialId
            );
            // position = new Vector3(i, height, j);
            // ParticleFactory.CreateBoundaryParticle(
            //     _world,
            //     position,
            //     particleSize,
            //     restDensity,
            //     materialId
            // );
        }

        for (var i = -startWidth; i < stopWidth; i += particleSize)
        for (var j = 0f; j < height; j += particleSize)
        {
            position = new Vector3(i, j, -startDepth);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                restDensity,
                materialId
            );
            position = new Vector3(i, j, startDepth);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                restDensity,
                materialId
            );
        }

        for (var i = -startDepth; i < stopDepth; i += particleSize)
        for (var j = 0f; j < height; j += particleSize)
        {
            position = new Vector3(-startWidth, j, i);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                restDensity,
                materialId
            );
            position = new Vector3(startWidth, j, i);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                restDensity,
                materialId
            );
        }
    }

    public override void Dispose()
    {
        _fluidRenderer.Dispose();
        _sensorManager.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
