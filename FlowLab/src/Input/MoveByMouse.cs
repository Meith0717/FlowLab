// MoveByMouse.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoKit.Graphics.Camera;
using MonoKit.Input;

namespace FlowLab.Input
{
    public class MoveByMouse(float speed = 1) : ICamera3dBehavior
    {
        private MouseState _prevMouseState;
        private float _yaw;
        private float _pitch;
        private readonly float _speed = speed;

        public void Initialize(Camera3D owner)
        {
            var x = float.Cos(_pitch) * float.Sin(_yaw);
            var y = float.Sin(_pitch);
            var z = float.Cos(_pitch) * float.Cos(_yaw);
            owner.Forward = new Vector3(x, y, z);
            owner.Right = Vector3.Normalize(Vector3.Cross(owner.Forward, Vector3.Up));
            owner.Up = Vector3.Cross(owner.Right, owner.Forward);
        }

        public void Update(Camera3D owner, InputHandler inputHandler, double elapsedGameTime)
        {
            var ks = Keyboard.GetState();
            var mouseState = Mouse.GetState();
            var sensitivity = 0.002f;

            if (inputHandler.HasAction((byte)ActionType.MoveCameraByMouse))
            {
                var deltaX = mouseState.X - _prevMouseState.X;
                var deltaY = mouseState.Y - _prevMouseState.Y;
                _yaw -= deltaX * sensitivity;
                _pitch -= deltaY * sensitivity;

                _pitch = float.Clamp(_pitch, -float.Pi / 2 + 0.01f, float.Pi / 2 - 0.01f);

                var x = float.Cos(_pitch) * float.Sin(_yaw);
                var y = float.Sin(_pitch);
                var z = float.Cos(_pitch) * float.Cos(_yaw);
                owner.Forward = new Vector3(x, y, z);
                owner.Right = Vector3.Normalize(Vector3.Cross(owner.Forward, Vector3.Up));
                owner.Up = Vector3.Cross(owner.Right, owner.Forward);
            }

            var movingSpeed = _speed * .01f * (float)elapsedGameTime;
            if (ks.IsKeyDown(Keys.W))
                owner.Position += owner.Forward * movingSpeed;
            if (ks.IsKeyDown(Keys.S))
                owner.Position -= owner.Forward * movingSpeed;
            if (ks.IsKeyDown(Keys.A))
                owner.Position -= owner.Right * movingSpeed;
            if (ks.IsKeyDown(Keys.D))
                owner.Position += owner.Right * movingSpeed;
            if (ks.IsKeyDown(Keys.PageUp))
                owner.Position += owner.Up * movingSpeed;
            if (ks.IsKeyDown(Keys.PageDown))
                owner.Position -= owner.Up * movingSpeed;

            _prevMouseState = Mouse.GetState();
        }
    }
}
