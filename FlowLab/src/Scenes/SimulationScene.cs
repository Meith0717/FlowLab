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
using FlowLab.Recording;
using FlowLab.Rigid_Bodies;
using FlowLab.Screens.Ui;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Core.IO;
using MonoKit.Ecs;
using MonoKit.Gameplay;
using MonoKit.Graphics;
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
    public readonly Watcher Watcher;
    public readonly SensorPlaneManager SensorManager;
    public readonly SimulationController SimController;

    // Render Stuff
    private readonly FluidRenderer _fluidRenderer;
    private readonly AxisRenderer _axisRenderer;
    private readonly BoundingBoxRenderer _boundingBoxRenderer;
    private readonly InstabilityRenderer _instabilityRenderer;

    // Recording
    private readonly Recorder _recorder;
    private RenderTarget2D _renderTarget;

    public SimulationScene(
        GraphicsDevice graphicsDevice,
        MessageDisplayer messageDisplayer,
        PathService<AppPaths> pathService
    )
    {
        _graphicsDevice = graphicsDevice;

        SimConfig = new SimConfig(1);
        _simRuntime = new GameRuntime3D(_graphicsDevice, SimConfig.SpatialHashQueryRadius);

        var spatialHashSystem = _simRuntime.Services.Get<EcsSpatialHash3D>();
        var kernels = new Kernels(SimConfig.MaxParticleSize);

        var camera3D = _simRuntime.Services.Get<Camera3D>();
        camera3D.AddBehaviour(new MoveByMouse());
        camera3D.AddBehaviour(new ZoomByMouse(.5f));

        var world = _simRuntime.Services.Get<World>();
        SimController = new SimulationController(world, messageDisplayer);
        _axisRenderer = new AxisRenderer(_graphicsDevice);

        _simRuntime.Services.AddService(new RigidBodyFactory(SimConfig));
        _simRuntime.Services.AddService(messageDisplayer);

        _fluidRenderer = new FluidRenderer(
            _graphicsDevice,
            world,
            spatialHashSystem,
            SimConfig.SpatialHashQueryRadius
        );
        Watcher = new Watcher(world, SimConfig);
        SensorManager = new SensorPlaneManager(
            _graphicsDevice,
            world,
            spatialHashSystem,
            kernels,
            SimConfig
        );
        // SensorManager.TryAdd(new SensorPlaneData("Plane 1", new(0, 0, 0), new(0, 0, 1), 30, 60, 3));

        var simDomain = new BoundingBox(new Vector3(-70, -100, -70), new Vector3(70, 100, 70));
        world.Systems.Add(new OutOfBoundsCleanupSystem(simDomain));
        _boundingBoxRenderer = new BoundingBoxRenderer(_graphicsDevice, simDomain);
        _instabilityRenderer = new InstabilityRenderer(_graphicsDevice, world, SimConfig);

        // Initialize recorder and render target
        _recorder = new Recorder(pathService, SimConfig, Watcher);
        CreateRenderTarget();

        world.Systems.Add(new StabilityChecker(SimConfig, SimController));
        world.Systems.Add(new RigidBodySystem(SimConfig, SimController, Watcher));
        world.Systems.Add(new ParticleTransformSyncSystem());
        world.Systems.Add(
            new DebugSystem(graphicsDevice, new ParticleRayChecker(world, spatialHashSystem))
        );
        world.Systems.Add(
            new SimulationSystem(spatialHashSystem, kernels, SimConfig, SimController, Watcher)
        );

        // Test
        SimController.OnPause = () => _recorder.StartRecording(Watcher.TimeSteps);
        Build(world);
    }

    private void CreateRenderTarget()
    {
        var width = _graphicsDevice.Viewport.Width;
        var height = _graphicsDevice.Viewport.Height;

        // Make sure dimensions are even (required for video encoding)
        if (width % 2 != 0)
            width--;
        if (height % 2 != 0)
            height--;

        _renderTarget?.Dispose();
        _renderTarget = new RenderTarget2D(
            _graphicsDevice,
            width,
            height,
            false,
            SurfaceFormat.Color,
            DepthFormat.Depth24
        );
    }

    public void ApplyResolution()
    {
        CreateRenderTarget();
    }

    private void Build(World world)
    {
        var rigidBodyFactory = _simRuntime.Services.Get<RigidBodyFactory>();
        var model = ObjLoader.Load(_graphicsDevice, Path.Combine("Content", "Models", "Cube.obj"));
        rigidBodyFactory.CreateStatic(
            world,
            model,
            Vector3.Zero,
            new Vector3(15, 50, 15),
            Matrix.Identity,
            1,
            1f
        );

        //AddFluidBlock(19, 19, 30, 4f, new Vector3(0, -4, 0), Color.DodgerBlue, 1);
        AddFluidBlock(29, 29, 30, 1f, new Vector3(0, -34, 0), Color.Orange, 0);

        model = ObjLoader.Load(_graphicsDevice, Path.Combine("Content", "Models", "Sphere.obj"));
        rigidBodyFactory.CreateDynamic(
            world,
            model,
            1f,
            new Vector3(0, 0, 0),
            new Vector3(5, 10, 5),
            Matrix.Identity,
            1
        );
    }

    public void LoadContent(ContentProvider contentProvider)
    {
        _fluidRenderer.LoadContent(contentProvider);
    }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler, float uiScale)
    {
        var camera3D = _simRuntime.Services.Get<Camera3D>();
        SimController.Update(elapsedMilliseconds, inputHandler);

        camera3D.Update(elapsedMilliseconds, inputHandler);
        _simRuntime.Update(elapsedMilliseconds, inputHandler);

        Watcher.Monitor(elapsedMilliseconds);
        if (!SimController.IsPaused)
            Watcher.UpdateRealTime(elapsedMilliseconds);

        SensorManager.Update(elapsedMilliseconds);
        _fluidRenderer.Update(SimController.HideBoundary);
        _fluidRenderer.ShowSpatialGrids = SimController.ShowSpatialGrids;

        if (inputHandler.HasAction((byte)ActionType.ToggleRecording))
            _recorder.ToggleRecording(Watcher.TimeSteps);

        _fluidRenderer.CrossSectionNormal = Vector3.UnitX;
        _fluidRenderer.CrossSectionPosition = Vector3.UnitX * 100;
        if (SensorManager.TryGetCurrentSensorPlaneData(out var data))
        {
            _fluidRenderer.CrossSectionNormal = data.Normal;
            _fluidRenderer.CrossSectionPosition = data.Position;
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        var camera3D = _simRuntime.Services.Get<Camera3D>();

        _graphicsDevice.SetRenderTarget(_renderTarget);
        _graphicsDevice.Clear(Color.Transparent);
        _fluidRenderer.Draw(camera3D);
        _graphicsDevice.SetRenderTarget(null);

        _recorder.Update(_renderTarget, Watcher.TimeSteps);

        // Draw render target to screen
        _boundingBoxRenderer.Draw(camera3D);
        _axisRenderer.Draw(camera3D);
        spriteBatch.Begin();
        spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
        spriteBatch.End();
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
        float colorId
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
                colorId
            );
    }

    public void Dispose()
    {
        _fluidRenderer.Dispose();
        SensorManager.Dispose();
        _axisRenderer.Dispose();
        _renderTarget?.Dispose();
        GC.SuppressFinalize(this);
    }
}
