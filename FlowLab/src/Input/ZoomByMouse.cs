// ZoomByMouse.cs
// Copyright (c) 2023-2025 Thierry Meiers
// All rights reserved.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoKit.Graphics.Camera;
using MonoKit.Input;

namespace FlowLab.Input
{
    /// <summary>
    /// Smoothly zooms a 3D camera using the mouse scroll wheel by adjusting the field of view (FOV).
    /// </summary>
    public class ZoomByMouse(float smooth) : ICamera3dBehavior
    {
        private readonly float _smooth = smooth;
        private float _zoomTarget;
        private int _lastScrollValue;

        public void Initialize(Camera3D owner)
        {
            _zoomTarget = owner.Fov;
            _lastScrollValue = Mouse.GetState().ScrollWheelValue;
        }

        public void Update(Camera3D owner, InputHandler inputHandler, double elapsedGameTime)
        {
            var mouse = Mouse.GetState();
            var delta = mouse.ScrollWheelValue - _lastScrollValue;
            _lastScrollValue = mouse.ScrollWheelValue;

            if (delta != 0)
            {
                var zoomDelta = delta / 120f;
                _zoomTarget *= 1f + zoomDelta * 0.2f;
                _zoomTarget = MathHelper.Clamp(
                    _zoomTarget,
                    MathHelper.ToRadians(10f),
                    MathHelper.ToRadians(120f)
                );
            }

            var t = 1f - (float)Math.Exp(-_smooth * elapsedGameTime);
            owner.Fov = MathHelper.Lerp(owner.Fov, _zoomTarget, t);
        }
    }
}
