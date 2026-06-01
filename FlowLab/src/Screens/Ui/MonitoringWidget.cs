// MonitoringWidget.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Monitoring;
using Microsoft.Xna.Framework;
using MonoKit.Core.Diagnostics;
using MonoKit.Ui;

namespace FlowLab.Screens.Ui;

public class MonitoringWidget(
    FrameCounter frameCounter,
    Config.SimConfig simConfig,
    LiveData liveData
)
{
    private UiFrame _simMonitoring;
    private UiSlider _cflBar;
    private UiSlider _errorBar;
    private UiSlider _iterationsBar;

    public void Build(UiFrame root)
    {
        root.Add(
            _simMonitoring = new UiFrame
            {
                Align = Align.NE,
                Width = 300,
                Height = 500,
                Color = new Color(30, 30, 30, 200),
                HSpace = 15,
                VSpace = 12,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "MONITORING")
            {
                Align = Align.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.2f,
                Color = Color.MonoGameOrange,
            }
        );

        _simMonitoring.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = 30,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola")
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
            new UiText("consola")
            {
                TextProvider = () => $"Entities: #{liveData.EntityCount}",
                Align = Align.Left,
                HSpace = 10,
                Y = 60,
                Scale = 0.15f,
                Color = Color.White,
            }
        );

        Stability(100);
        Solver(235);
        Fluid(340);
    }

    private void Stability(int y)
    {
        _simMonitoring.Add(
            new UiText("consola", "STABILITY")
            {
                Align = Align.Left,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.White,
            }
        );
        _simMonitoring.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = y + 20,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "CFL")
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
            new UiText("consola")
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
            new UiText("consola", "Time Step")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{simConfig.TimeStep}",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Max. Vel.")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{float.Round(liveData.MaxVelocity, 2)} m/s",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Avg. Vel.")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{float.Round(liveData.AvgVelocity, 2)} m/s",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
    }

    private void Solver(int y)
    {
        _simMonitoring.Add(
            new UiText("consola", "SOLVER")
            {
                Align = Align.Left,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.White,
            }
        );
        _simMonitoring.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = y + 20,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Iterations")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{liveData.IterationCount} / {simConfig.MaxIterations}",
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
                Y = y + 53,
                HSpace = 10,
                RelWidth = 1,
                Height = 15,
                BgColor = Color.Gray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                Text = "2",
                Align = Align.Left,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{simConfig.MaxIterations}",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
    }

    private void Fluid(int y)
    {
        _simMonitoring.Add(
            new UiText("consola", "FLUID")
            {
                Align = Align.Left,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.White,
            }
        );
        _simMonitoring.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = y + 20,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Mass")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
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
            new UiText("consola", "Density")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                Align = Align.Right,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.16f,
                Color = Color.White,
                TextProvider = () => $"{simConfig.FluidDensity} kg/m\u00B3",
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Init. Volume")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{liveData.FluidMass * simConfig.FluidDensity} m\u00B3",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Volume")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola")
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
            new UiText("consola", "Abs. Error")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 110,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{float.Round(liveData.AbsError * 100, 2)} %",
                Align = Align.Right,
                HSpace = 10,
                Y = y + 110,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Error")
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y + 130,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{float.Round(liveData.CompressionError * 100, 2)} %",
                Align = Align.Right,
                HSpace = 150,
                Y = y + 130,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            _errorBar = new UiSlider(false)
            {
                Align = Align.Right,
                Y = y + 132,
                HSpace = 10,
                Width = 130,
                Height = 15,
                BgColor = Color.Gray,
            }
        );
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

        var normalizedIterations = (float)liveData.IterationCount / simConfig.MaxIterations;
        var iterationColor = IterationsColor(normalizedIterations);
        _iterationsBar.Value = normalizedIterations;
        _iterationsBar.Color = iterationColor;
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
