// SensorPlaneForm.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Config;
using FlowLab.Monitoring.SensorPlanes;
using Microsoft.Xna.Framework;
using MonoKit.Screens;
using MonoKit.Ui;

namespace FlowLab.Screens;

public class SensorPlaneForm : Screen
{
    private readonly UiVariableSelector<string> _sensorPlaneSelector;
    private readonly SensorPlaneManager _sensorPlaneManager;
    private readonly SensorPlaneData _sensorPlaneData;
    private readonly UiFrame _rootFrame;
    private readonly bool _override;

    private UiTextEntry _idEntry;
    private UiTextEntry _posXEntry,
        _posYEntry,
        _posZEntry;
    private UiTextEntry _normXEntry,
        _normYEntry,
        _normZEntry;
    private UiTextEntry _widthEntry,
        _heightEntry;
    private UiTextEntry _resolutionEntry;

    public SensorPlaneForm(
        GameServiceContainer appServices,
        SensorPlaneData? sensorData,
        SensorPlaneManager sensorPlaneManager,
        UiVariableSelector<string> sensorPlaneSelector
    )
        : base(appServices, false, true)
    {
        var uiConfig = appServices.GetService<UiConfig>();

        _sensorPlaneSelector = sensorPlaneSelector;
        _sensorPlaneData = sensorData ?? SensorPlaneData.Default;
        _sensorPlaneManager = sensorPlaneManager;
        _override = sensorData != null;

        var title = sensorData == null ? "CREATE SENSOR PLANE" : "EDIT SENSOR PLANE";

        UiRoot.Add(
            _rootFrame = new UiFrame
            {
                Align = Align.Center,
                Color = new Color(30, 30, 30, 230),
                Width = 400,
                Height = 440,
                HSpace = 12,
                VSpace = 10,
            }
        );

        _rootFrame.Add(
            new UiText(uiConfig.DefaultSpriteFont, title)
            {
                Align = Align.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.22f,
                Color = Color.MonoGameOrange,
            }
        );

        _rootFrame.Add(
            new UiFrame
            {
                Align = Align.CenterV,
                Y = 32,
                RelWidth = 0.95f,
                Height = 2,
                Color = Color.DimGray,
            }
        );

        var y = 40;

        // ID
        AddLabeledField(uiConfig, "ID", ref _idEntry, _sensorPlaneData.Id, y, 0.0f, 0.8f);

        // Position row
        y += 30;
        AddSubtitle(uiConfig, "Position", y);
        y += 30;
        AddLabeledField(
            uiConfig,
            "X",
            ref _posXEntry,
            _sensorPlaneData.Position.X.ToString(),
            y,
            0.0f,
            0.30f
        );
        AddLabeledField(
            uiConfig,
            "Y",
            ref _posYEntry,
            _sensorPlaneData.Position.Y.ToString(),
            y,
            0.35f,
            0.65f
        );
        AddLabeledField(
            uiConfig,
            "Z",
            ref _posZEntry,
            _sensorPlaneData.Position.Z.ToString(),
            y,
            0.70f,
            1.0f
        );
        y += 30;

        // Normal row
        AddSubtitle(uiConfig, "Normal", y);
        y += 30;
        AddLabeledField(
            uiConfig,
            "X",
            ref _normXEntry,
            _sensorPlaneData.Normal.X.ToString(),
            y,
            0.0f,
            0.30f
        );
        AddLabeledField(
            uiConfig,
            "Y",
            ref _normYEntry,
            _sensorPlaneData.Normal.Y.ToString(),
            y,
            0.35f,
            0.65f
        );
        AddLabeledField(
            uiConfig,
            "Z",
            ref _normZEntry,
            _sensorPlaneData.Normal.Z.ToString(),
            y,
            0.70f,
            1.0f
        );
        y += 30;

        // Size row
        AddSubtitle(uiConfig, "Size", y);
        y += 30;
        AddLabeledField(
            uiConfig,
            "Width",
            ref _widthEntry,
            _sensorPlaneData.Width.ToString(),
            y,
            0.0f,
            0.45f
        );
        AddLabeledField(
            uiConfig,
            "Height",
            ref _heightEntry,
            _sensorPlaneData.Height.ToString(),
            y,
            0.50f,
            1.0f
        );
        y += 30;

        // Resolution
        AddSubtitle(uiConfig, "Resolution", y);
        y += 30;
        AddLabeledField(
            uiConfig,
            "N",
            ref _resolutionEntry,
            _sensorPlaneData.Resolution.ToString(),
            y,
            0.0f,
            0.30f
        );

        // Buttons
        var buttonFrame = new UiFrame
        {
            Align = Align.S,
            RelWidth = 1.0f,
            Height = 36,
            Color = Color.Transparent,
            HSpace = 12,
            VSpace = 6,
        };
        _rootFrame.Add(buttonFrame);

        var cancelButton = new UiButton.Text
        {
            Align = Align.W,
            RelY = 0.5f,
            Width = 75,
            Height = 28,
            HSpace = 6,
            UiText = new UiText(uiConfig.DefaultSpriteFont, "CANCEL")
            {
                Scale = 0.14f,
                Color = Color.LightGray,
            },
            OnClickAction = () => ScreenManager.PopScreen(),
        };
        buttonFrame.Add(cancelButton);

        var saveButton = new UiButton.Text
        {
            Align = Align.E,
            RelY = 0.5f,
            Width = 75,
            Height = 28,
            HSpace = 6,
            UiText = new UiText(uiConfig.DefaultSpriteFont, "SAVE")
            {
                Scale = 0.14f,
                Color = Color.LightGray,
            },
            OnClickAction = Save,
        };
        buttonFrame.Add(saveButton);
    }

    private void AddSubtitle(UiConfig uiConfig, string text, int y)
    {
        _rootFrame.Add(
            new UiText(uiConfig.DefaultSpriteFont, text)
            {
                Align = Align.Left,
                HSpace = 10,
                Y = y,
                Scale = 0.14f,
                Color = Color.LightGray,
            }
        );
    }

    private void AddLabeledField(
        UiConfig uiConfig,
        string label,
        ref UiTextEntry field,
        string value,
        int y,
        float relX,
        float relWidth
    )
    {
        var labelEntry = new UiText(uiConfig.DefaultSpriteFont, label + ":")
        {
            RelX = relX,
            Y = y,
            Width = 35,
            Scale = 0.12f,
            HSpace = 15,
            Color = Color.LightGray,
        };
        _rootFrame.Add(labelEntry);

        field = new UiTextEntry(uiConfig.DefaultSpriteFont)
        {
            RelX = relX + 0.20f,
            Y = y,
            RelWidth = relWidth - 0.10f,
            HSpace = 15,
            Height = 18,
            Scale = 0.11f,
            Color = Color.White,
            BgColor = new Color(40, 40, 40, 200),
            FocusedBgColor = new Color(60, 60, 80, 220),
            CursorColor = Color.MonoGameOrange,
            Text = value,
            Padding = 3,
        };
        _rootFrame.Add(field);
    }

    private void Save()
    {
        var data = new SensorPlaneData(
            _override ? _sensorPlaneData.Id : _idEntry.Text,
            new Vector3(
                float.Parse(_posXEntry.Text),
                float.Parse(_posYEntry.Text),
                float.Parse(_posZEntry.Text)
            ),
            Vector3.Normalize(
                new Vector3(
                    float.Parse(_normXEntry.Text),
                    float.Parse(_normYEntry.Text),
                    float.Parse(_normZEntry.Text)
                )
            ),
            int.Parse(_widthEntry.Text),
            int.Parse(_heightEntry.Text),
            int.Parse(_resolutionEntry.Text)
        );

        _sensorPlaneManager.TryAdd(data, _override);
        _sensorPlaneManager.TrySetCurrentPlane(data.Id);
        ScreenManager.PopScreen();
        _sensorPlaneSelector.Items = _sensorPlaneManager.PlaneIds;
    }
}
