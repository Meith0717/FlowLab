// MonitoringWidget.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Linq;
using FlowLab.Config;
using FlowLab.Monitoring;
using FlowLab.Monitoring.SensorPlanes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Core.Diagnostics;
using MonoKit.Screens;
using MonoKit.Ui;

namespace FlowLab.Screens.Ui;

public class MonitoringWidget(
    GameServiceContainer services,
    ScreenManager screenManager,
    SimConfig config,
    LiveData liveData,
    SensorPlaneManager sensorPlaneManager
)
{
    private UiFrame _simMonitoring;
    private UiSlider _cflBar;
    private UiSlider _errorBar;
    private UiSlider _iterationsBar;
    private UiFrame _sensorTextureFrame;
    private UiText _sensorTextureComment;
    private UiSprite _planeSprite;
    private UiVariableSelector<string> _sensorSelector;

    public MonitoringWidget Build(UiFrame root)
    {
        root.Add(
            _simMonitoring = new UiFrame
            {
                Align = Align.SW,
                Width = 375,
                RelHeight = 0.955f,
                HSpace = 7,
                VSpace = 10,
                Color = new Color(30, 30, 30, 200),
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "MONITORING")
            {
                Align = Align.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.2f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = 35,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        var frameCounter = services.GetService<FrameCounter>();
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"FPS: {(int)frameCounter.CurrentFramesPerSecond}",
                Align = Align.Left,
                HSpace = 10,
                Y = 40,
                Scale = 0.15f,
                Color = Color.White,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"Entities: #{liveData.EntityCount}",
                Align = Align.Left,
                HSpace = 10,
                Y = 70,
                Scale = 0.15f,
                Color = Color.White,
            }
        );

        Stability(100);
        Fluid(250);
        Solver(470);
        Sensors(580);

        return this;
    }

    private void Stability(int y)
    {
        _simMonitoring.Add(
            new UiText("defaultFont", "STABILITY")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "CFL")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _cflBar = new UiSlider(false)
        {
            Align = Align.Right,
            Y = y + 32,
            HSpace = 10,
            Width = 130,
            Height = 15,
            BgColor = Color.Gray,
        };
        _simMonitoring.Add(_cflBar);
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                Align = Align.Right,
                HSpace = 150,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => $"{float.Round(liveData.Cfl, 2)}",
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Time Step")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 60,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{config.TimeStep}",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 60,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Max. Vel.")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{float.Round(liveData.MaxVelocity, 2)} m/s",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Avg. Vel.")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 120,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{float.Round(liveData.AvgVelocity, 2)} m/s",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 120,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
    }

    private void Solver(int y)
    {
        _simMonitoring.Add(
            new UiText("defaultFont", "SOLVER")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Iterations")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{liveData.IterationCount} / {config.MaxIterations}",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            _iterationsBar = new UiSlider(false)
            {
                Align = Align.Left,
                Y = y + 63,
                HSpace = 10,
                RelWidth = 1,
                Height = 15,
                BgColor = Color.Gray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                Text = "2",
                Align = Align.Left,
                HSpace = 10,
                Y = y + 80,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{config.MaxIterations}",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 80,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
    }

    private void Fluid(int y)
    {
        _simMonitoring.Add(
            new UiText("defaultFont", "FLUID")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Mass")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                Align = Align.Right,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.16f,
                Color = Color.White,
                TextProvider = () => $"{liveData.FluidMass} kg",
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Init. Volume")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 60,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{float.NaN} m\u00B3",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 60,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Volume")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{float.Round(liveData.FluidVolume)} m\u00B3",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Abs. Error")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 150,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{float.Round(liveData.AbsError * 100, 2)} %",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 150,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("defaultFont", "Compression")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 180,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("defaultFont")
            {
                TextProvider = () => $"{float.Round(liveData.CompressionError * 100, 2)} %",
                Align = Align.Right,
                HSpace = 150,
                Y = y + 180,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            _errorBar = new UiSlider(false)
            {
                Align = Align.Right,
                Y = y + 182,
                HSpace = 10,
                Width = 130,
                Height = 15,
                BgColor = Color.Gray,
            }
        );
    }

    private void Sensors(int y)
    {
        _simMonitoring.Add(
            new UiText("defaultFont", "SENSORS")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiButton.Sprite("edit")
            {
                Align = Align.Left,
                Y = y,
                HSpace = 10,
                Scale = 0.75f,
                OnClickAction = () =>
                {
                    if (sensorPlaneManager.TryGetCurrentSensorPlaneData(out var data))
                        screenManager.AddScreen(
                            new SensorPlaneForm(services, data, sensorPlaneManager, _sensorSelector)
                        );
                },
            }
        );

        _simMonitoring.Add(
            new UiButton.Sprite("add")
            {
                Align = Align.Left,
                Y = y,
                HSpace = 10 + 10 + 32,
                Scale = 0.75f,
                OnClickAction = () =>
                {
                    screenManager.AddScreen(
                        new SensorPlaneForm(services, null, sensorPlaneManager, _sensorSelector)
                    );
                },
            }
        );

        _simMonitoring.Add(
            _sensorSelector = new UiVariableSelector<string>(
                "arrowL",
                "arrowR",
                "defaultFont",
                sensorPlaneManager.PlaneIds.ToArray()
            )
            {
                Align = Align.CenterV,
                HSpace = 10,
                Y = y + 30,
                ButtonScale = .75f,
                RelWidth = 1,
                TextScale = 0.15f,
                TextColor = Color.LightGray,
                OnClickAction = value => sensorPlaneManager.TrySetCurrentPlane(value),
            }
        );

        _simMonitoring.Add(
            _sensorTextureFrame = new UiFrame
            {
                Align = Align.CenterV,
                Y = y + 60,
                Width = 350,
                Height = 350,
                Color = Color.Transparent,
            }
        );
        _sensorTextureFrame.Add(
            _sensorTextureComment = new UiText("defaultFont")
            {
                Align = Align.Center,
                Text = "No Sensor Plane Set",
                Color = Color.LightGray,
                Scale = .15f,
            }
        );

        _simMonitoring.Add(
            new UiVariableSelector<PropertyType>(
                "arrowL",
                "arrowR",
                "defaultFont",
                Enum.GetValues<PropertyType>()
            )
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 420,
                RelWidth = .45f,
                ButtonScale = .75f,
                TextScale = 0.15f,
                TextColor = Color.LightGray,
                OnClickAction = value => sensorPlaneManager.PropertyType = value,
            }
        );
        _simMonitoring.Add(
            new UiVariableSelector<ColorScheme>(
                "arrowL",
                "arrowR",
                "defaultFont",
                Enum.GetValues<ColorScheme>()
            )
            {
                Align = Align.Right,
                HSpace = 10,
                Y = y + 420,
                RelWidth = .45f,
                ButtonScale = .75f,
                TextScale = 0.15f,
                TextColor = Color.LightGray,
                OnClickAction = value => sensorPlaneManager.ColorScheme = value,
            }
        );
        CreatePlaneSprite();
    }

    public void Update()
    {
        var cfl = float.Clamp(liveData.Cfl, 0, 1);
        var cflColor = CflColor(cfl);
        _cflBar.Value = cfl * 2;
        _cflBar.Color = cflColor;

        var error = float.Clamp(float.RootN(liveData.CompressionError, 3), 0, 1);
        var errorColor = ErrorColor(liveData.CompressionError);
        _errorBar.Value = error;
        _errorBar.Color = errorColor;

        var normalizedIterations = (float)liveData.IterationCount / config.MaxIterations;
        var iterationColor = IterationsColor(normalizedIterations);
        _iterationsBar.Value = normalizedIterations;
        _iterationsBar.Color = iterationColor;

        _sensorTextureComment.Color =
            sensorPlaneManager.Count <= 0 ? Color.LightGray : Color.Transparent;

        UpdatePlaneSprite();
    }

    private void CreatePlaneSprite()
    {
        if (_planeSprite != null)
            return;

        _planeSprite = new UiSprite((Texture2D)null, scale: 2f, color: Color.White)
        {
            Align = Align.Center,
            FillScale = FillScale.Fit,
        };
        _sensorTextureFrame.Add(_planeSprite);
    }

    private void UpdatePlaneSprite()
    {
        if (!sensorPlaneManager.GetCurrentTexture(out var texture))
            return;

        _planeSprite.SpriteTexture = texture;
    }

    private static Color IterationsColor(float normalizedIterations)
    {
        return normalizedIterations switch
        {
            < 0.75f => Color.Lerp(Color.Lime, Color.Yellow, normalizedIterations / 0.75f),
            < 0.95f => Color.Lerp(Color.Yellow, Color.Red, (normalizedIterations - 0.75f) / 0.2f),
            _ => Color.Red,
        };
    }

    private static Color CflColor(float cfl)
    {
        return cfl switch
        {
            < 0.4f => Color.Lerp(Color.Lime, Color.Yellow, cfl / 0.4f),
            < 0.5f => Color.Lerp(Color.Yellow, Color.Red, (cfl - 0.4f) / 0.1f),
            _ => Color.Red,
        };
    }

    private static Color ErrorColor(float error)
    {
        return error switch
        {
            < 0.01f => Color.Lerp(Color.Lime, Color.Yellow, error / 0.01f),
            < 0.03f => Color.Lerp(Color.Yellow, Color.Red, (error - 0.01f) / 0.02f),
            _ => Color.Red,
        };
    }
}
