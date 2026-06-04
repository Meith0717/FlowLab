// SensorPlaneForm.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Input;
using FlowLab.Monitoring.SensorPlanes;
using FlowLab.Screens.Ui;
using Microsoft.Xna.Framework;
using MonoKit.Input;
using MonoKit.Screens;
using MonoKit.Ui;

namespace FlowLab.Screens;

public class SensorPlaneForm : Screen
{
    private readonly SensorPlaneData _sensorPlaneData;
    private readonly SensorPlaneManager _sensorPlaneManager;
    private readonly UiFrame _rootFrame;

    public SensorPlaneForm(
        GameServiceContainer appServices,
        SensorPlaneData? sensorData,
        SensorPlaneManager sensorPlaneManager
    )
        : base(appServices, false, true)
    {
        var title = sensorData == null ? "CREATE SENSOR PLANE" : "EDIT SENSOR PLANE";
        _sensorPlaneData = sensorData ?? SensorPlaneData.Default;
        _sensorPlaneManager = sensorPlaneManager;

        UiRoot.Add(
            _rootFrame = new UiFrame()
            {
                Align = Align.Center,
                Color = new Color(30, 30, 30),
                Width = 450,
                Height = 600,
            }
        );

        _rootFrame.Add(
            new UiText("defaultFont", title)
            {
                Align = Align.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.2f,
                Color = Color.Red,
            }
        );

        _rootFrame.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = 35,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        if (inputHandler.HasAction((byte)ActionType.Test))
            ScreenManager.PopScreen();

        base.Update(elapsedMilliseconds, inputHandler, uiScale);
    }
}
