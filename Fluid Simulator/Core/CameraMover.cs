using Microsoft.Xna.Framework;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;

namespace Fluid_Simulator.Core
{
    public static class CameraMover
    {
        public static void ControllZoom(GameTime gameTime, InputState inputState, Camera camera2D, float minZoom, float maxZoom)
        {
            var zoom = 0f;
            var multiplier = 1;

            inputState.DoAction(ActionType.CameraZoomIn, () => zoom += 100f / (float)gameTime.ElapsedGameTime.TotalMilliseconds);
            inputState.DoAction(ActionType.CameraZoomOut, () => zoom -= 100f / (float)gameTime.ElapsedGameTime.TotalMilliseconds);

            camera2D.Zoom *= 1 + (zoom * multiplier) * (float)(0.001 * gameTime.ElapsedGameTime.TotalMilliseconds);
            camera2D.Zoom = MathHelper.Clamp(camera2D.Zoom, minZoom, maxZoom);
        }
    }
}
