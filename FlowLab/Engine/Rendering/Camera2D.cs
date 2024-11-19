// Camera2D.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace FlowLab.Engine.Rendering
{
    public class Camera2D
    {
        public Vector2 Position;
        public float Zoom;
        public float Rotation;

        public RectangleF Bounds { get; private set; }
        public Matrix TransformationMatrix { get; private set; }

        public Camera2D()
        {
            Position = Vector2.Zero;
            Zoom = 1;
            Rotation = 0;
        }

        public Matrix Update(Rectangle worldBounds)
        {
            TransformationMatrix = Transformations.CreateViewTransformationMatrix(Position, Zoom, Rotation, worldBounds.Width, worldBounds.Height);

            Bounds = FrustumCuller.GetFrustum(worldBounds, TransformationMatrix);
            return TransformationMatrix;
        }
    }
}
