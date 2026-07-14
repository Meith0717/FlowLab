// SimulationScene.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.IO;
using FlowLab.Config;
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
using MonoKit.Gameplay;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Spatial;

namespace FlowLab.Scenes;

public class SimulationScene : IDisposable
{
    public readonly SimConfig SimConfig;

    // Core
    private readonly GraphicsDevice _graphicsDevice;
    private readonly GameRuntime3D _simRuntime;

    // Other Stuff
    public readonly LiveData LiveData;
    public readonly SensorPlaneManager SensorManager;
    public readonly SimulationController SimController;
    public readonly SimulationTracker SimTracker;

    // Render Stuff
    private readonly FluidRenderer _fluidRenderer;
    private readonly AxisRenderer _axisRenderer;
    private readonly RigidBodyRenderer _rigidBodyRenderer;
    private readonly BoundingBoxRenderer _boundingBoxRenderer;
    private readonly InstabilityRenderer _instabilityRenderer;

    public SimulationScene(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        SimConfig = SimConfig.Default;
        _simRuntime = new GameRuntime3D(_graphicsDevice, SimConfig.SpatialHashQueryRadius);

        var spatialHashSystem = _simRuntime.Services.Get<EcsSpatialHash3D>();
        var kernels = new Kernels(SimConfig.MaxParticleSize);

        var camera3D = _simRuntime.Services.Get<Camera3D>();
        camera3D.AddBehaviour(new MoveByMouse());
        camera3D.AddBehaviour(new ZoomByMouse(.5f));

        var world = _simRuntime.Services.Get<World>();
        SimController = new SimulationController(world);
        SimTracker = new SimulationTracker(SimConfig);
        _axisRenderer = new AxisRenderer(_graphicsDevice);

        _simRuntime.Services.AddService(new RigidBodyFactory(SimConfig));

        _fluidRenderer = new FluidRenderer(
            _graphicsDevice,
            world,
            spatialHashSystem,
            SimConfig.SpatialHashQueryRadius
        );
        _rigidBodyRenderer = new RigidBodyRenderer(world, _graphicsDevice);
        LiveData = new LiveData(world, SimConfig);
        SensorManager = new SensorPlaneManager(
            _graphicsDevice,
            world,
            spatialHashSystem,
            kernels,
            SimConfig
        );

        var simDomain = new BoundingBox(new Vector3(-50, -100, -50), new Vector3(50, 100, 50));
        world.Systems.Add(new OutOfBoundsCleanupSystem(simDomain));
        _boundingBoxRenderer = new BoundingBoxRenderer(_graphicsDevice, simDomain);
        _instabilityRenderer = new InstabilityRenderer(_graphicsDevice, world, SimConfig);

        world.Systems.Add(new StabilityChecker(SimConfig, SimController));
        world.Systems.Add(new RigidBodySystem(SimConfig, SimController));
        world.Systems.Add(new ParticleTransformSyncSystem());
        world.Systems.Add(
            new DebugSystem(graphicsDevice, new ParticleRayChecker(world, spatialHashSystem))
        );
        world.Systems.Add(
            new SimulationSystem(spatialHashSystem, kernels, SimConfig, SimController, SimTracker)
        );

        // Test
        Build(world);
    }

    private void Build(World world)
    {
        var rigidBodyFactory = _simRuntime.Services.Get<RigidBodyFactory>();
        var model = ObjLoader.Load(_graphicsDevice, Path.Combine("Content", "Models", "Cube.obj"));
        rigidBodyFactory.CreateStatic(
            world,
            model,
            Vector3.Zero,
            new Vector3(10, 35, 10),
            Matrix.Identity,
            1,
            1,
            1
        );

        AddFluidBlock(15, 15, 30, 1f, new Vector3(0, -17, 0), Color.DodgerBlue, 0);

        // model = ObjLoader.Load(_graphicsDevice, Path.Combine("Content", "Models", "Sphere.obj"));
        // RigidBodyFactory.CreateDynamic(
        //     world,
        //     model,
        //     Vector3.Zero,
        //     new Vector3(1),
        //     Matrix.Identity,
        //     1
        // );
    }

    public void LoadContent()
    {
        _fluidRenderer.LoadContent();
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler, float uiScale)
    {
        var camera3D = _simRuntime.Services.Get<Camera3D>();
        var world = _simRuntime.Services.Get<World>();

        SimController.Update(elapsedMilliseconds, inputHandler);

        camera3D.Update(elapsedMilliseconds, inputHandler);
        _simRuntime.Update(elapsedMilliseconds, inputHandler);

        if (!SimController.IsPaused)
            SimTracker.UpdateRealTime(elapsedMilliseconds);

        LiveData.Collect(elapsedMilliseconds);
        SensorManager.Update(elapsedMilliseconds);
        _fluidRenderer.Update(SimController.HideBoundary);
        _fluidRenderer.ShowSpatialGrids = SimController.ShowSpatialGrids;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var camera3D = _simRuntime.Services.Get<Camera3D>();

        _fluidRenderer.Draw(camera3D);
        _rigidBodyRenderer.Draw(camera3D);
        _boundingBoxRenderer.Draw(camera3D);
        _axisRenderer.Draw(camera3D);
        _instabilityRenderer.Draw(camera3D);
        SensorManager.Draw(camera3D);
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

        var particleSize = SimConfig.MaxParticleSize;
        var halfParticleSize = particleSize / 2f;
        var halfWidth = width / 2;
        var halfDepth = depth / 2;
        var halfHeight = height / 2;

        var startWidth = halfWidth - halfParticleSize;
        var stopWidth = halfWidth + halfParticleSize;
        var startDepth = halfDepth - halfParticleSize;
        var stopDepth = halfDepth + halfParticleSize;

        for (var x = -startWidth; x < stopWidth; x += particleSize)
        for (var z = -startDepth; z < stopDepth; z += particleSize)
        for (var y = -halfHeight; y < halfHeight; y += particleSize)
            ParticleFactory.CreateFluidParticle(
                world,
                position + new Vector3(x, y, z),
                particleSize,
                restDensity,
                color,
                materialId
            );
    }

    public void Dispose()
    {
        _fluidRenderer.Dispose();
        SensorManager.Dispose();
        _axisRenderer.Dispose();
        GC.SuppressFinalize(this);
    }
}
