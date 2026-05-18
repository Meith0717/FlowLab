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
    private readonly Config.SimConfig _simConfig = simConfig;
    private UiFrame _settingsFrame;

    // Text entry fields for numeric values
    private UiTextEntry _stiffnessEntry;
    private UiTextEntry _viscosityEntry;
    private UiTextEntry _timeStepEntry;
    private UiTextEntry _gravityEntry;
    private UiButton.Text _parallelToggle;
    private UiText _parallelText;

    public void Build(UiFrame root)
    {
        root.Add(
            _settingsFrame = new UiFrame
            {
                Allign = Allign.NW,
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
                Allign = Allign.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.2f,
                Color = Color.MonoGameOrange,
            }
        );

        _settingsFrame.Add(
            new UiFrame
            {
                Allign = Allign.CenterV,
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
            _simConfig.Stiffness.ToString("F2")
        );
        AddTextEntrySetting(
            "Viscosity",
            100,
            ref _viscosityEntry,
            _simConfig.Viscosity.ToString("F2")
        );
        AddTextEntrySetting(
            "Time Step",
            140,
            ref _timeStepEntry,
            _simConfig.TimeStep.ToString("F3")
        );
        AddTextEntrySetting("Gravity", 180, ref _gravityEntry, _simConfig.Gravity.ToString("F3"));
        AddParallelToggle(220);
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
                Allign = Allign.Left,
                HSpace = 10,
                Y = y,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        field = new UiTextEntry("consola")
        {
            Allign = Allign.Right,
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

    private void AddParallelToggle(int y)
    {
        _settingsFrame.Add(
            new UiText("consola", "Parallel")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = y,
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        _parallelText = new UiText("consola")
        {
            Allign = Allign.Right,
            HSpace = 10,
            Y = y,
            Scale = 0.15f,
            Color = _simConfig.UseParallel ? Color.Lime : Color.DimGray,
            TextProvider = () => _simConfig.UseParallel ? "ON" : "OFF",
        };
        _settingsFrame.Add(_parallelText);

        // Create clickable area for toggle
        var toggleText = new UiText("consola", " ")
        {
            Allign = Allign.Right,
            HSpace = 10,
            Y = y,
            Width = 40,
            Height = 20,
            Scale = 0.15f,
        };
        toggleText.Color = Color.Transparent;

        _parallelToggle = new UiButton.Text();
        _parallelToggle.OnClickAction = ToggleParallel;
        _parallelToggle.UiText = toggleText;

        var buttonFrame = new UiFrame
        {
            Allign = Allign.Right,
            HSpace = 10,
            Y = y - 2,
            Width = 40,
            Height = 20,
            Color = Color.Transparent,
        };
        buttonFrame.Add(_parallelToggle);
        _settingsFrame.Add(buttonFrame);
    }

    private void ToggleParallel()
    {
        _simConfig.UseParallel = !_simConfig.UseParallel;
        _parallelText.Color = _simConfig.UseParallel ? Color.Lime : Color.DimGray;
    }

    public void Update(InputHandler inputHandler)
    {
        // Sync text entry values to simConfig
        if (_stiffnessEntry != null && float.TryParse(_stiffnessEntry.Text, out var stiffness))
            _simConfig.Stiffness = MathHelper.Clamp(stiffness, 0, 200);

        if (_viscosityEntry != null && float.TryParse(_viscosityEntry.Text, out var viscosity))
            _simConfig.Viscosity = MathHelper.Clamp(viscosity, 0, 5);

        if (_timeStepEntry != null && float.TryParse(_timeStepEntry.Text, out var timeStep))
            _simConfig.TimeStep = MathHelper.Clamp(timeStep, 0.001f, 0.5f);

        if (_gravityEntry != null && float.TryParse(_gravityEntry.Text, out var gravity))
            _simConfig.Gravity = MathHelper.Clamp(gravity, 0, 1);
    }
}
