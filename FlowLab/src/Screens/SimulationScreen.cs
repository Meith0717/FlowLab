// SimulationScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Input;
using FlowLab.Sph;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using MonoKit.Screens;
using MonoKit.Ui;

namespace FlowLab.Screens;

public class SimulationScreen : Screen
{
    private readonly Camera3D _camera3D;
    private readonly BasicEffect _effect;
    private readonly FluidSimulation _fluidSimulation;
    private readonly FluidRenderer _fluidRenderer;

    public SimulationScreen(GameServiceContainer appServices)
        : base(appServices, false, false)
    {
        _camera3D = new Camera3D(Vector3.Zero, GraphicsDevice);
        _camera3D.AddBehaviour(new MoveByMouse());
        _camera3D.AddBehaviour(new ZoomByMouse(.5f));
        _effect = new BasicEffect(GraphicsDevice);
        _fluidSimulation = new FluidSimulation();
        _fluidRenderer = new FluidRenderer(GraphicsDevice);

        var layer = new UiFrame()
        {
            Allign = Allign.E,
            Width = 300,
            RelHeight = .8f,
            Color = Color.DimGray,
            HSpace = 10,
        };
        UiRoot.Add(layer);

        var liveData = _fluidSimulation.LiveData;
        layer.Add(new UiText("consola") { TextProvider = () => $"EntityCount: {liveData.EntityCount} ", RelY = .1f, Scale = .15f});
        layer.Add(new UiText("consola") { TextProvider = () => $"CompressionError: {liveData.CompressionError}", RelY = .2f, Scale = .15f });
        layer.Add(new UiText("consola") { TextProvider = () => $"TotalError: {liveData.TotalError }", RelY = .3f, Scale = .15f });
        layer.Add(new UiText("consola") { TextProvider = () => $"Cfl: {liveData.Cfl}", RelY = .4f, Scale = .15f });
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
        _fluidSimulation.Update(elapsedMilliseconds, inputHandler);
        _fluidRenderer.Update(_fluidSimulation.InstanceData);
        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        _fluidRenderer.Draw(_camera3D);
        base.Draw(spriteBatch);
    }
}
