// Camera2DMover.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Engine.Rendering
{
    public static class Camera2DMover
    {
        public static void ControllZoom(GameTime gameTime, InputState inputState, Camera2D camera2D, float minZoom, float maxZoom)
        {
            var zoom = 0f;
            var multiplier = 1;

            inputState.DoAction(ActionType.CameraZoomIn, () => zoom += 100f / (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            inputState.DoAction(ActionType.CameraZoomOut, () => zoom -= 100f / (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            if (zoom == 0) return;

            camera2D.Zoom *= 1 + zoom * multiplier * (float)(0.001 * gameTime.ElapsedGameTime.TotalMilliseconds);
            camera2D.Zoom = MathHelper.Clamp(camera2D.Zoom, minZoom, maxZoom);
        }

        private static Vector2 lastMousePosition;

        public enum DragInput { LeftHold, RightHold }

        public static bool UpdateCameraByMouseDrag(InputState inputState, Camera2D camera)
        {
            var wasMoved = false;
            if (inputState.HasAction(ActionType.RightClickHold))
            {
                Vector2 delta = inputState.MousePosition - lastMousePosition;
                camera.Position -= delta / camera.Zoom;
                wasMoved = true;
            }

            lastMousePosition = inputState.MousePosition;
            return wasMoved;
        }

        public static void EdgeScrolling(InputState inputState, GameTime gameTime, GraphicsDevice graphicsDevice, Camera2D camera)
        {
            var screen = graphicsDevice.Viewport.Bounds;
            var edge = screen.Size.ToVector2() * 0.10f;
            var bounds = new RectangleF(screen.Location.ToVector2() + edge / 2, screen.Size.ToVector2() - edge);
            if (bounds.Contains(inputState.MousePosition)) return;
            var center = screen.Center;
            var dir = Vector2.Normalize(inputState.MousePosition - center.ToVector2());
            camera.Position += dir * .5f * (float)gameTime.ElapsedGameTime.TotalMilliseconds / camera.Zoom;
        }
    }
}
