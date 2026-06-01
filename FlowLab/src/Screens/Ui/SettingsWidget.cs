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
    private UiTextEntry _stiffnessEntry;
    private UiTextEntry _viscosityEntry;
    private UiTextEntry _timeStepEntry;
    private UiTextEntry _gravityEntry;

    public void Build(UiFrame root)
    {
        root.Add(
            _settingsFrame = new UiFrame
            {
                Align = Align.NW,
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
            "Stiffness",
            60,
            ref _stiffnessEntry,
            simConfig.Stiffness.ToString("F2")
        );
        AddTextEntrySetting(
            "Viscosity",
            100,
            ref _viscosityEntry,
            simConfig.Viscosity.ToString("F2")
        );
        AddTextEntrySetting(
            "Time Step",
            140,
            ref _timeStepEntry,
            simConfig.TimeStep.ToString("F3")
        );
        AddTextEntrySetting("Gravity", 180, ref _gravityEntry, simConfig.Gravity.ToString("F3"));
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
            Height = 24,
            Scale = 0.15f,
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
        if (_stiffnessEntry != null && float.TryParse(_stiffnessEntry.Text, out var stiffness))
            simConfig.Stiffness = MathHelper.Clamp(stiffness, 0, 200);

        if (_viscosityEntry != null && float.TryParse(_viscosityEntry.Text, out var viscosity))
            simConfig.Viscosity = MathHelper.Clamp(viscosity, 0, 5);

        if (_timeStepEntry != null && float.TryParse(_timeStepEntry.Text, out var timeStep))
            simConfig.TimeStep = MathHelper.Clamp(timeStep, 0.001f, 0.5f);

        if (_gravityEntry != null && float.TryParse(_gravityEntry.Text, out var gravity))
            simConfig.Gravity = MathHelper.Clamp(gravity, 0, 1);
    }
}
