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
    Watcher watcher,
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
        var uiConfig = services.GetService<UiConfig>();

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
            new UiText(uiConfig.DefaultSpriteFont, "MONITORING")
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
            new UiText(uiConfig.DefaultSpriteFont)
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
            new UiText(uiConfig.DefaultSpriteFont)
            {
                TextProvider = () => $"Entities: #{watcher.EntityCount}",
                Align = Align.Left,
                HSpace = 10,
                Y = 70,
                Scale = 0.15f,
                Color = Color.White,
            }
        );

        Stability(uiConfig, 100);
        Fluid(uiConfig, 250);
        Solver(uiConfig, 470);
        Sensors(uiConfig, 580);

        return this;
    }

    private void Stability(UiConfig uiConfig, int y)
    {
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "STABILITY")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "CFL")
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
            new UiText(uiConfig.DefaultSpriteFont)
            {
                Align = Align.Right,
                HSpace = 150,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => $"{float.Round(watcher.Cfl, 2)}",
            }
        );

        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "Time Step")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 60,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont)
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
            new UiText(uiConfig.DefaultSpriteFont, "Max. Vel.")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont)
            {
                TextProvider = () => $"{float.Round(watcher.MaxVelocity, 2)} m/s",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "Avg. Vel.")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 120,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont)
            {
                TextProvider = () => $"{float.Round(watcher.AvgVelocity, 2)} m/s",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 120,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
    }

    private void Solver(UiConfig uiConfig, int y)
    {
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "SOLVER")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "Iterations")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont)
            {
                TextProvider = () => $"{watcher.IterationCount} / {config.MaxIterations}",
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
            new UiText(uiConfig.DefaultSpriteFont)
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
            new UiText(uiConfig.DefaultSpriteFont)
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

    private void Fluid(UiConfig uiConfig, int y)
    {
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "FLUID")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "Mass")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont)
            {
                Align = Align.Right,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.16f,
                Color = Color.White,
                TextProvider = () => $"{watcher.FluidMass} kg",
            }
        );

        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "Init. Volume")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 60,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont)
            {
                TextProvider = () => $"{watcher.FluidInitVolume} m\u00B3",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 60,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "Compression")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 180,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont)
            {
                TextProvider = () => $"{float.Round(watcher.CompressionError * 100, 2)} %",
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

    private void Sensors(UiConfig uiConfig, int y)
    {
        _simMonitoring.Add(
            new UiText(uiConfig.DefaultSpriteFont, "SENSORS")
            {
                Align = Align.Right,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.Red,
            }
        );

        _simMonitoring.Add(
            new UiButton.Sprite(uiConfig.EditButtonTexture)
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
            new UiButton.Sprite(uiConfig.AddButtonTexture)
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
                uiConfig.DefaultSelectorLeftTexture,
                uiConfig.DefaultSelectorRightTexture,
                uiConfig.DefaultSpriteFont,
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
            _sensorTextureComment = new UiText(uiConfig.DefaultSpriteFont)
            {
                Align = Align.Center,
                Text = "No Sensor Plane SetRestVolume",
                Color = Color.LightGray,
                Scale = .15f,
            }
        );

        _simMonitoring.Add(
            new UiVariableSelector<PropertyType>(
                uiConfig.DefaultSelectorLeftTexture,
                uiConfig.DefaultSelectorRightTexture,
                uiConfig.DefaultSpriteFont,
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
                uiConfig.DefaultSelectorLeftTexture,
                uiConfig.DefaultSelectorRightTexture,
                uiConfig.DefaultSpriteFont,
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
        var cfl = float.Clamp(watcher.Cfl, 0, 1);
        var cflColor = CflColor(cfl);
        _cflBar.Value = cfl * 2;
        _cflBar.Color = cflColor;

        var error = float.Clamp(float.RootN(watcher.CompressionError, 3), 0, 1);
        var errorColor = ErrorColor(watcher.CompressionError);
        _errorBar.Value = error;
        _errorBar.Color = errorColor;

        var normalizedIterations = (float)watcher.IterationCount / config.MaxIterations;
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

        _planeSprite.Texture2D = texture;
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
