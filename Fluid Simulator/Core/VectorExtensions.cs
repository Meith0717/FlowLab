using Microsoft.Xna.Framework;
using System;

namespace Fluid_Simulator.Core
{
    public static class VectorExtensions
    {
        public static float AngleBetween(Vector2 from, Vector2 to)
        {
            float dotProduct = Vector2.Dot(from, to);
            float magnitudeProduct = from.Length() * to.Length();

            if (magnitudeProduct == 0)
                throw new InvalidOperationException("Cannot calculate an angle with a zero-length vector.");

            float cosAngle = dotProduct / magnitudeProduct;

            cosAngle = MathHelper.Clamp(cosAngle, -1f, 1f);

            return (float)Math.Acos(cosAngle);
        }
    }
}
