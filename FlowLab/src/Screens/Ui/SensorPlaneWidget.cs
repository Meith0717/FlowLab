// SensorPlaneWidget.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Input;
using MonoKit.Ui;
using FlowLab.Monitoring.SensorPlanes;

namespace FlowLab.Screens.Ui;

/// <summary>
/// UI widget for displaying and controlling sensor plane visualization.
/// Shows the sensor plane as a UiSprite and allows switching between planes.
/// </summary>
public class SensorPlaneWidget(SensorPlaneManager sensorPlaneManager)
{
    private UiFrame _sensorFrame;
    private UiFrame _textureFrame;
    private UiSprite _planeSprite;
    private UiButton.Text _nextPlaneButton;
    private UiButton.Text _cyclePropertyButton;
    private UiButton.Text _cycleColorButton;

    public void Build(UiFrame root)
    {
        // Initialize current plane index if planes exist
        var planeIds = sensorPlaneManager.PlaneIds;
        if (planeIds.Count > 0 && sensorPlaneManager.CurrentPlaneIndex < 0)
        {
            sensorPlaneManager.CurrentPlaneIndex = 0;
        }

        // Main container frame
        root.Add(
            _sensorFrame = new UiFrame
            {
                Allign = Allign.SW,       // Bottom-left
                Width = 300,
                Height = 420,  // Increased height for better spacing
                Color = new Color(30, 30, 30, 200),
                HSpace = 15,
                VSpace = 12,
            }
        );

        // Title
        _sensorFrame.Add(
            new UiText("consola", "SENSOR PLANE")
            {
                Allign = Allign.N,
                HSpace = 5,
                VSpace = 5,
                Scale = 0.2f,
                Color = Color.MonoGameOrange,
            }
        );

        // Separator
        _sensorFrame.Add(
            new UiFrame
            {
                Allign = Allign.CenterV,
                Y = 30,
                RelWidth = .95f,
                Height = 4,
                Color = Color.DimGray,
            }
        );

        // Texture display frame (for the plane sprite) - made wider and moved down
        _sensorFrame.Add(
            _textureFrame = new UiFrame
            {
                Allign = Allign.CenterV,
                Y = 50,
                RelWidth = 0.975f,
                Height = 250,
                Color = Color.Transparent,
            }
        );

        // Plane info: Label
        _sensorFrame.Add(
            new UiText("consola", "Plane:")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = 320,  // +15px
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        // Plane info: Current plane name and index
        _sensorFrame.Add(
            new UiText("consola")
            {
                Allign = Allign.Right,
                HSpace = 80,
                Y = 320,  // +15px
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => GetCurrentPlaneInfo(),
            }
        );
        
        // Navigation buttons: Next plane
        AddNavigationButton("NEXT", 320, ref _nextPlaneButton, NextPlane);

        // Separator for property section
        _sensorFrame.Add(
            new UiFrame
            {
                Allign = Allign.CenterV,
                Y = 355,  // +15px
                RelWidth = .95f,
                Height = 2,
                Color = Color.DimGray,
            }
        );

        // Property info
        _sensorFrame.Add(
            new UiText("consola", "Property:")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = 365,  // +15px
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        // Property value display
        _sensorFrame.Add(
            new UiText("consola")
            {
                Allign = Allign.Right,
                HSpace = 80,
                Y = 365,  // +15px
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => sensorPlaneManager.PropertyType.ToString(),
            }
        );

        // Cycle property button
        AddNavigationButton("CYCLE", 365, ref _cyclePropertyButton, CycleProperty);

        // Color scheme info
        _sensorFrame.Add(
            new UiText("consola", "Color:")
            {
                Allign = Allign.Left,
                HSpace = 10,
                Y = 395,  // +15px
                Scale = 0.15f,
                Color = Color.LightGray,
            }
        );

        // Color value display
        _sensorFrame.Add(
            new UiText("consola")
            {
                Allign = Allign.Right,
                HSpace = 80,
                Y = 395,  // +15px
                Scale = 0.15f,
                Color = Color.White,
                TextProvider = () => sensorPlaneManager.ColorScheme.ToString(),
            }
        );

        // Cycle color button
        AddNavigationButton("CYCLE", 395, ref _cycleColorButton, CycleColor);
        
        // Create the initial plane sprite
        CreatePlaneSprite();
    }

    /// <summary>
    /// Creates the plane sprite if it doesn't exist.
    /// </summary>
    private void CreatePlaneSprite()
    {
        if (_planeSprite == null)
        {
            var texture = sensorPlaneManager.GetCurrentTexture();
            if (texture != null && !texture.IsDisposed)
            {
                _planeSprite = new UiSprite(texture, scale: 2f, color: Color.White)
                {
                    Allign = Allign.Center,
                    FillScale = FillScale.Fit,
                };
                _textureFrame.Add(_planeSprite);
            }
        }
    }

    private void AddNavigationButton(string text, int y, ref UiButton.Text button, Action onClick)
    {
        var buttonText = new UiText("consola", text)
        {
            Allign = Allign.Center,
            Width = 50,
            Height = 20,
            Scale = 0.13f,
            Color = Color.White,
        };

        button = new UiButton.Text()
        {
            OnClickAction = onClick,
            UiText = buttonText,
            Allign = Allign.Right,
            HSpace = 10,
            Y = y,
            Width = 52,
            Height = 22,
        };

        _sensorFrame.Add(button);
    }

    private string GetCurrentPlaneInfo()
    {
        var planeIds = sensorPlaneManager.PlaneIds;
        if (planeIds.Count == 0)
            return "No planes";

        var currentIndex = sensorPlaneManager.CurrentPlaneIndex;
        if (currentIndex < 0 || currentIndex >= planeIds.Count)
            return "Invalid plane";

        return $"{planeIds[currentIndex]} ({currentIndex + 1}/{planeIds.Count})";
    }

    /// <summary>
    /// Updates the plane sprite with the current texture.
    /// </summary>
    private void UpdatePlaneSprite()
    {
        var texture = sensorPlaneManager.GetCurrentTexture();
        
        if (texture != null && !texture.IsDisposed)
        {
            // Create sprite if it doesn't exist
            CreatePlaneSprite();
            
            // Update the sprite's texture
            if (_planeSprite != null)
            {
                _planeSprite.SpriteTexture = texture;
            }
        }
    }
    
    private void NextPlane()
    {
        var planeIds = sensorPlaneManager.PlaneIds;
        if (planeIds.Count == 0) return;
        sensorPlaneManager.CurrentPlaneIndex = 
            (sensorPlaneManager.CurrentPlaneIndex + 1) % planeIds.Count;
        UpdatePlaneSprite();
    }

    private void CycleProperty()
    {
        var values = Enum.GetValues(typeof(PropertyType));
        var currentIndex = Array.IndexOf(values, sensorPlaneManager.PropertyType);
        var nextIndex = (currentIndex + 1) % values.Length;
        sensorPlaneManager.PropertyType = (PropertyType)values.GetValue(nextIndex);
        UpdatePlaneSprite();
    }

    private void CycleColor()
    {
        var values = Enum.GetValues(typeof(ColorScheme));
        var currentIndex = Array.IndexOf(values, sensorPlaneManager.ColorScheme);
        var nextIndex = (currentIndex + 1) % values.Length;
        sensorPlaneManager.ColorScheme = (ColorScheme)values.GetValue(nextIndex);
        // Update sprite to reflect new color scheme
        UpdatePlaneSprite();
    }

    public void Update(InputHandler inputHandler)
    {
        // Update the sprite each frame to show the latest texture data
        UpdatePlaneSprite();
    }
}
