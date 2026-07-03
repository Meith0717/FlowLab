// SimulationScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.IO;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Geometry;
using FlowLab.Input;
using FlowLab.Monitoring;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Rigid_Bodies;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    private readonly GameRuntime3D _simRuntime;
    private readonly Camera3D _camera3D;
    private readonly FluidRenderer _fluidRenderer;
    private readonly LiveData _liveData;
    private readonly SensorPlaneManager _sensorManager;
    private readonly SimulationController _simController;
    private readonly SimulationTracker _simTracker;
    private readonly BoundingBoxRenderer _boundingBoxRenderer;
    private readonly AxisRenderer _axisRenderer;
    private readonly RigidBodyRenderer _rigidBodyRenderer;

    public SimulationScreen(GameServiceContainer appServices)
        : base(appServices, false, false)
    {
        _simConfig = SimConfig.Default;
        _simRuntime = new GameRuntime3D(GraphicsDevice, _simConfig.SpatialHashQueryRadius);
        var world = _simRuntime.Services.Get<World>();
        var spatialHashSystem = _simRuntime.Services.Get<EcsSpatialHash3D>();
        var kernels = new Kernels(_simConfig.MaxParticleSize);

        _camera3D = _simRuntime.Services.Get<Camera3D>();
        _camera3D.AddBehaviour(new MoveByMouse());
        _camera3D.AddBehaviour(new ZoomByMouse(.5f));

        _simController = new SimulationController(world);
        _simTracker = new SimulationTracker(_simConfig);
        _axisRenderer = new AxisRenderer(GraphicsDevice);
        _boundingBoxRenderer = new BoundingBoxRenderer(
            GraphicsDevice,
            new BoundingBox(new Vector3(-50, -100, -50), new Vector3(50, 100, 50))
        );
        _fluidRenderer = new FluidRenderer(
            GraphicsDevice,
            world,
            spatialHashSystem,
            _simConfig.SpatialHashQueryRadius
        );
        _rigidBodyRenderer = new RigidBodyRenderer(world, GraphicsDevice);
        _liveData = new LiveData(world, _simConfig);
        _sensorManager = new SensorPlaneManager(
            GraphicsDevice,
            world,
            spatialHashSystem,
            kernels,
            _simConfig
        );

        world.Systems.Add(new DomainSystem(_boundingBoxRenderer.BoundingBox));
        world.Systems.Add(new DiagnosticSystem(_simConfig, _simController));
        world.Systems.Add(new RigidBodySystem(_simConfig));
        world.Systems.Add(new ParticleTransformSyncSystem());
        world.Systems.Add(
            new SimulationSystem(
                spatialHashSystem,
                kernels,
                _simConfig,
                _simController,
                _simTracker
            )
        );

        // Test
        var model = ObjLoader.Load(GraphicsDevice, Path.Combine("Content", "Models", "Sphere.obj"));
        var e = world.CreateEntity();
        world.Components.Add(e, new Transform3D(Vector3.Zero, Matrix.Identity, Vector3.One * 10));
        world.Components.Add(e, new Velocity3D(Vector3.Zero, Vector3.Zero));
        world.Components.Add(
            e,
            new RigidBodyComponent(model, 10, Matrix.CreateRotationX(0), Vector3.Zero)
        );
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
        _rigidBodyRenderer.Draw(_camera3D);
        _boundingBoxRenderer.Draw(_camera3D);
        _axisRenderer.Draw(_camera3D);
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
        var world = _simRuntime.Services.Get<World>();

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
                world,
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
        var world = _simRuntime.Services.Get<World>();

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
                world,
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
        var world = _simRuntime.Services.Get<World>();

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
                world,
                position,
                particleSize,
                restDensity,
                materialId
            );
            position = new Vector3(i, height, j);
            ParticleFactory.CreateBoundaryParticle(
                world,
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
                world,
                position,
                particleSize,
                restDensity,
                materialId
            );
            position = new Vector3(i, j, startDepth);
            ParticleFactory.CreateBoundaryParticle(
                world,
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
                world,
                position,
                particleSize,
                restDensity,
                materialId
            );
            position = new Vector3(startWidth, j, i);
            ParticleFactory.CreateBoundaryParticle(
                world,
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
        _axisRenderer.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
