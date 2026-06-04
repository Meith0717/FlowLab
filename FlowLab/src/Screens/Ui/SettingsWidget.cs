// SettingsWidget.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using MonoKit.Input;
using MonoKit.Ui;

namespace FlowLab.Screens.Ui;

public class SettingsWidget(Config.SimConfig simConfig)
{
    private UiFrame _settingsFrame;

    // Text entry fields for numeric values
    private UiTextEntry _fViscosityEntry;
    private UiTextEntry _bViscosityEntry;
    private UiTextEntry _timeStepEntry;
    private UiTextEntry _gravityEntry;

    public void Build(UiFrame root)
    {
        root.Add(
            _settingsFrame = new UiFrame
            {
                Align = Align.SE,
                Width = 250,
                Height = 350,
                Color = new Color(30, 30, 30, 200),
                HSpace = 15,
                VSpace = 12,
            }
        );

        _settingsFrame.Add(
            new UiText("consola", "SETTINGS")
            {
                Align = Align.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.2f,
                Color = Color.MonoGameOrange,
            }
        );

        _settingsFrame.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = 30,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        AddTextEntrySetting(
            "Fluid Vis.",
            60,
            ref _fViscosityEntry,
            simConfig.FViscosity.ToString()
        );
        AddTextEntrySetting(
            "Boundary Vis.",
            90,
            ref _bViscosityEntry,
            simConfig.BViscosity.ToString()
        );
        AddTextEntrySetting("Time Step", 120, ref _timeStepEntry, simConfig.TimeStep.ToString());
        AddTextEntrySetting("Gravity", 180, ref _gravityEntry, simConfig.Gravity.ToString());
    }

    private void AddTextEntrySetting(
        string label,
        int y,
        ref UiTextEntry field,
        string initialValue
    )
    {
        _settingsFrame.Add(
            new UiText("consola", label)
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        field = new UiTextEntry("consola")
        {
            Align = Align.Right,
            Y = y,
            HSpace = 10,
            Width = 120,
            Height = 20,
            Scale = 0.12f,
            Color = Color.White,
            BgColor = new Color(40, 40, 40, 200),
            FocusedBgColor = new Color(60, 60, 80, 220),
            PlaceholderColor = Color.DimGray,
            CursorColor = Color.MonoGameOrange,
            Text = initialValue,
            Padding = 4,
        };
        _settingsFrame.Add(field);
    }

    public void Update(InputHandler inputHandler)
    {
        // Sync text entry values to simConfig
        if (_fViscosityEntry != null && float.TryParse(_fViscosityEntry.Text, out var bViscosity))
            simConfig.FViscosity = MathHelper.Clamp(bViscosity, 0, 5);

        if (_bViscosityEntry != null && float.TryParse(_bViscosityEntry.Text, out var fViscosity))
            simConfig.BViscosity = MathHelper.Clamp(fViscosity, 0, 5);

        if (_timeStepEntry != null && float.TryParse(_timeStepEntry.Text, out var timeStep))
            simConfig.TimeStep = MathHelper.Clamp(timeStep, 0.001f, 0.5f);

        if (_gravityEntry != null && float.TryParse(_gravityEntry.Text, out var gravity))
            simConfig.Gravity = MathHelper.Clamp(gravity, 0, 1);
    }
}
