// MovementController.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;

namespace FlowLab.Utilities
{
    public static class MovementController
    {

        public static float GetRotationUpdate(float currentRotation, Vector2 currentPosition, Vector2 targetPosition)
        {
            var targetRotation = Geometry.AngleBetweenVectors(currentPosition, targetPosition);
            float delta = Geometry.AngleDegDelta(MathHelper.ToDegrees(currentRotation), MathHelper.ToDegrees(targetRotation));
            delta = MathHelper.ToRadians(delta);
            return delta switch
            {
                float.NaN => 0,
                _ => delta
            };
        }

        public static float GetRotationUpdate(float currentRotation, float targetRotation, float rotationSmoothingFactor)
        {
            float delta = Geometry.AngleDegDelta(MathHelper.ToDegrees(currentRotation), MathHelper.ToDegrees(targetRotation));
            delta = MathHelper.ToRadians(delta);
            return delta switch
            {
                float.NaN => 0,
                _ => delta * rotationSmoothingFactor,
            };
        }

        public static float GetVelocity(GameTime gameTime, float currentVelocity, float targetVelocity, float acceleration)
        {
            var a = MathF.Abs(acceleration);
            if (targetVelocity < currentVelocity) a = -a;

            var newVelocity = currentVelocity + a * (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            return a switch
            {
                < 0f => MathF.Max(0f, newVelocity),
                > 0f => MathF.Min(newVelocity, targetVelocity),
                _ => currentVelocity
            };
        }
    }
}
