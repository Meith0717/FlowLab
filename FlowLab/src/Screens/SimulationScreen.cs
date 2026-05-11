// SimulationScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Input;
using FlowLab.Monitoring;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Screens;

namespace FlowLab.Screens;

public class SimulationScreen : Screen
{
    private readonly Camera3D _camera3D;
    private readonly BasicEffect _effect;
    private readonly FluidRenderer _fluidRenderer;
    public readonly FluidSimulation FluidSimulation;

    public LiveData LiveData => FluidSimulation.LiveData;

    public SimulationScreen(GameServiceContainer appServices)
        : base(appServices, false, false)
    {
        _camera3D = new Camera3D(Vector3.Zero, GraphicsDevice);
        _camera3D.AddBehaviour(new MoveByMouse());
        _camera3D.AddBehaviour(new ZoomByMouse(.5f));
        _effect = new BasicEffect(GraphicsDevice);
        FluidSimulation = new FluidSimulation();
        _fluidRenderer = new FluidRenderer(GraphicsDevice);
    }

    public override void Initialize()
    {
        _fluidRenderer.Initialize();
        base.Initialize();
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        _camera3D.Update(elapsedMilliseconds, inputHandler);
        FluidSimulation.Update(elapsedMilliseconds, inputHandler);
        _fluidRenderer.Update(FluidSimulation.InstanceData);
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _fluidRenderer.Draw(_camera3D);
        base.Draw(spriteBatch);
    }
}
