// MouseHelper.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlowLab.Input;

public static class MouseHelper
{
    public static Ray GetMouseRay(Matrix projection, Matrix view, Viewport viewport)
    {
        var mousePosition = Mouse.GetState().Position.ToVector2();
        var mouseX = mousePosition.X - viewport.X;
        var mouseY = mousePosition.Y - viewport.Y;
        var nearPoint = new Vector3(mouseX, mouseY, 0);
        var farPoint = new Vector3(mouseX, mouseY, 1);
        var nearWorld = viewport.Unproject(nearPoint, projection, view, Matrix.Identity);
        var farWorld = viewport.Unproject(farPoint, projection, view, Matrix.Identity);
        return new Ray(nearWorld, Vector3.Normalize(farWorld - nearWorld));
    }
}
