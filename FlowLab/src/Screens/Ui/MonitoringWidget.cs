// MonitoringWidget.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Monitoring;
using Microsoft.Xna.Framework;
using MonoKit.Core.Diagnostics;
using MonoKit.Ui;

namespace FlowLab.Screens.Ui;

public class MonitoringWidget(FrameCounter frameCounter, Config.Config config, LiveData liveData)
{
    private UiFrame _simMonitoring;
    private UiSlider _cflBar;

    public void Build(UiFrame root)
    {
        root.Add(
            _simMonitoring = new UiFrame
            {
                Allign = Allign.NE,
                Width = 300,
                Height = 400,
                Color = new Color(30, 30, 30, 200),
                HSpace = 15,
                VSpace = 12,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "MONITORING")
            {
                Allign = Allign.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.2f,
                Color = Color.MonoGameOrange,
            }
        );

        _simMonitoring.Add(
            new UiFrame
            {
                Allign = Allign.CenterV,
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
                Allign = Allign.Left,
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
                Allign = Allign.Left,
                HSpace = 10,
                Y = 60,
                Scale = 0.15f,
                Color = Color.White,
            }
        );

        Stability(100);
        Fluid(230);
    }

    private void Stability(int y)
    {
        _simMonitoring.Add(
            new UiText("consola", "STABILITY")
            {
                Allign = Allign.Left,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.White,
            }
        );
        _simMonitoring.Add(
            new UiFrame
            {
                Allign = Allign.CenterV,
                Y = y + 20,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "CFL")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _cflBar = new UiSlider(false)
        {
            Allign = Allign.Right,
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
                Allign = Allign.Right,
                HSpace = 150,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => $"{float.Round(liveData.Cfl * 100, 1)}%",
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Time Step")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{config.TimeStep}",
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Max. Vel.")
            {
                Allign = Allign.Left,
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
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Avg. Vel.")
            {
                Allign = Allign.Left,
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
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
    }

    private void Fluid(int y)
    {
        _simMonitoring.Add(
            new UiText("consola", "FLUID")
            {
                Allign = Allign.Left,
                Y = y,
                HSpace = 10,
                Scale = 0.175f,
                Color = Color.White,
            }
        );
        _simMonitoring.Add(
            new UiFrame
            {
                Allign = Allign.CenterV,
                Y = y + 20,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Mass")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = y + 30,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                Allign = Allign.Right,
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
                Allign = Allign.Left,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 50,
                Scale = 0.16f,
                Color = Color.White,
                TextProvider = () => $"{config.FluidDensity} kg/m\u00B3",
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Init. Volume")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{liveData.FluidMass * config.FluidDensity} m\u00B3",
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 70,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Volume")
            {
                Allign = Allign.Left,
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
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 90,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Error")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = y + 110,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{float.Round(liveData.CompressionError, 2)} %",
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 110,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );

        _simMonitoring.Add(
            new UiText("consola", "Abs. Error")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = y + 130,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
        _simMonitoring.Add(
            new UiText("consola")
            {
                TextProvider = () => $"{float.Round(liveData.AbsError, 2)} %",
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 130,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
    }

    public void Update()
    {
        var cfl = MathHelper.Clamp(liveData.Cfl, 0, 1);
        var cflColor = CflColor(cfl);
        _cflBar.Value = cfl * 2;
        _cflBar.Color = cflColor;
    }

    private static Color CflColor(float cfl)
    {
        return cfl switch
        {
            < 0.4f => Color.Lime,
            < 0.5f => Color.Yellow,
            _ => Color.Red,
        };
    }
}
