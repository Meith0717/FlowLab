using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace StellarLiberation.Game.Core.Visuals.Rendering
{
    public class Camera
    {
        public Vector2 Position;
        public float Zoom;
        public float Rotation;

        public RectangleF Bounds { get; private set; }
        public Matrix TransformationMatrix { get; private set; }

        public Camera()
        {
            Position = Vector2.Zero;
            Zoom = 1;
            Rotation = 0;
        }

        public void Update(GraphicsDevice graphicsDevice) => TransformationMatrix = CreateViewTransformationMatrix(Position, Zoom, Rotation, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height);

        private static Matrix CreateViewTransformationMatrix(Vector2 cameraPosition, float cameraZoom, float cameraRotation,
                int screenWidth, int screenHeight)
        {
            Matrix translationMatrix = Matrix.CreateTranslation(new Vector3(-cameraPosition.X, -cameraPosition.Y, 0));
            Matrix scaleMatrix = Matrix.CreateScale(cameraZoom, cameraZoom, 1);
            Matrix rotationMatrix = Matrix.CreateRotationZ(cameraRotation);
            Matrix screenCenterMatrix = Matrix.CreateTranslation(new Vector3(screenWidth / 2f, screenHeight / 2f, 0));

            return translationMatrix * scaleMatrix * rotationMatrix * screenCenterMatrix;
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition) => Vector2.Transform(screenPosition, Matrix.Invert(TransformationMatrix));

        public Vector2 WorldToScreen(Vector2 worldPosition) =>  Vector2.Transform(worldPosition, TransformationMatrix);
    }
}
