// SimulationScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Specialized;
using System.IO;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Geometry;
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
    private readonly WireframeRenderer _wireframeRenderer;
    private readonly AxisRenderer _axisRenderer;

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
        var domain = new BoundingBox(new Vector3(-50, -100, -50), new Vector3(50, 100, 50));
        _world.Systems.Add(new DomainSystem(domain));
        _world.Systems.Add(new DiagnosticSystem(_simConfig, _simController));

        _boundingBoxRenderer = new BoundingBoxRenderer(GraphicsDevice, domain);
        _axisRenderer = new AxisRenderer(GraphicsDevice) { AxisLength = 10f };
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

        // SpawnBox(30, 30, 200, 1f, 1, 1);
        var model = ObjLoader.Load(Path.Combine("Content", "Models", "Cube.obj"));

        var transform = Matrix.CreateScale(new Vector3(12, 20, 12));
        _wireframeRenderer = new WireframeRenderer(GraphicsDevice, model)
        {
            World = transform,
            Color = Color.Orange,
        };

        var lst = MeshParticleSampler.SampleSurface(
            model,
            _simConfig.MaxParticleSize / 1f,
            transform: transform
        );

        foreach (var vector4 in lst)
        {
            var position = new Vector3(vector4.X, vector4.Y, vector4.Z);
            ParticleFactory.CreateBoundaryParticle(_world, position, vector4.W, 1, 1);
        }
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
        {
            AddFluidBlock(10, 10, 25, 1f, Vector3.Zero, Color.DodgerBlue, 1);
        }

        if (inputHandler.HasAction((byte)ActionType.Test))
        {
            AddFluidBlock(10, 10, 25, .1f, Vector3.Zero, Color.Orange, 1);
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
        _axisRenderer.Draw(_camera3D);
        _wireframeRenderer.Draw(_camera3D);
        _sensorManager.Draw(_camera3D);
        base.Draw(spriteBatch);
    }

    private void AddMovingFluidBlock(
        float width,
        float depth,
        float restDensity,
        Vector3 position,
        Color color,
        float materialId,
        Vector3 velocity
    )
    {
        var particleSize = _simConfig.MaxParticleSize;
        var halfParticleSize = particleSize / 2f;
        var halfWidth = width / 2;
        var halfDepth = depth / 2;

        var startWidth = halfWidth - halfParticleSize;
        var stopWidth = halfWidth + halfParticleSize;
        var startDepth = halfDepth - halfParticleSize;
        var stopDepth = halfDepth + halfParticleSize;

        for (var x = -startWidth; x < stopWidth; x += particleSize)
        for (var z = -startDepth; z < stopDepth; z += particleSize)
            ParticleFactory.CreateMovingFluidParticle(
                _world,
                position + new Vector3(x, 0, z),
                particleSize,
                restDensity,
                color,
                materialId,
                velocity
            );
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
            position = new Vector3(i, height, j);
            ParticleFactory.CreateBoundaryParticle(
                _world,
                position,
                particleSize,
                restDensity,
                materialId
            );
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
        _wireframeRenderer.Dispose();
        _axisRenderer.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
