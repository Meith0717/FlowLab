// HudScreen.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.Intrinsics.X86;
using FlowLab.Config;
using FlowLab.Monitoring;
using Microsoft.Xna.Framework;
using MonoKit.Core.Diagnostics;
using MonoKit.Input;
using MonoKit.Screens;
using MonoKit.Ui;

namespace FlowLab.Screens;

public class HudScreen : Screen
{
    private readonly LiveData _liveData;
    private readonly SimulationConfig _config;
    private readonly FrameCounter _frameCounter;
    private UiFrame _simMonitoring;
    private UiSlider _cflBar;

    public HudScreen(
        GameServiceContainer appServices,
        FrameCounter frameCounter,
        SimulationConfig config,
        LiveData liveData
    )
        : base(appServices, true, true)
    {
        _liveData = liveData;
        _frameCounter = frameCounter;
        _config = config;
        BuildUi();
    }

    private void BuildUi()
    {
        _simMonitoring = new UiFrame
        {
            Allign = Allign.NE,
            Width = 300,
            Height = 400,
            Color = new Color(30, 30, 30, 200),
            HSpace = 15,
            VSpace = 12,
        };

        UiRoot.Add(_simMonitoring);

        _simMonitoring.Add(
            new UiText("consola", "SIMULATION MONITOR")
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
                TextProvider = () => $"FPS: {(int)_frameCounter.CurrentFramesPerSecond}",
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
                TextProvider = () => $"Entities: #{_liveData.EntityCount}",
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
                TextProvider = () => $"{float.Round(_liveData.Cfl * 100, 1)}%",
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
                TextProvider = () => $"{_config.TimeStep}",
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
                TextProvider = () => $"{float.Round(_liveData.MaxVelocity, 2)} m/s",
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
                TextProvider = () => $"{float.Round(_liveData.AvgVelocity, 2)} m/s",
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
                TextProvider = () => $"{_liveData.FluidMass} kg",
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
                TextProvider = () => $"{SimulationConfig.FluidDensity} kg/m\u00B3",
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
                TextProvider = () =>
                    $"{_liveData.FluidMass * SimulationConfig.FluidDensity} m\u00B3",
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
                TextProvider = () => $"{float.Round(_liveData.FluidVolume)} m\u00B3",
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
                TextProvider = () => $"{float.Round(_liveData.CompressionError, 2)} %",
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
                TextProvider = () => $"{float.Round(_liveData.AbsError, 2)} %",
                Allign = Allign.Right,
                HSpace = 10,
                Y = y + 130,
                Scale = 0.16f,
                Color = Color.LightGray,
            }
        );
    }

    public override void Update(
        double elapsedMilliseconds,
        InputHandler inputHandler,
        float uiScale
    )
    {
        // Update CFL bar and text color
        var cfl = MathHelper.Clamp(_liveData.Cfl, 0, 1);
        var cflColor = CflColor(cfl);
        _cflBar.Value = cfl * 2;
        _cflBar.Color = cflColor;

        base.Update(elapsedMilliseconds, inputHandler, uiScale);
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
