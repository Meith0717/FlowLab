using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Fluid_Simulator.Core
{
    internal static class Utilitys
    {
        public static float Sum<T>(IEnumerable<T> scource, Func<T, float> body)
        {
            var sum = 0f;
            foreach ( var item in scource )
                sum += body(item);
            return sum;
        }

        public static Vector2 Sum<T>(IEnumerable<T> scource, Func<T, Vector2> body)
        {
            var sum = Vector2.Zero;
            foreach (var item in scource)
                sum += body(item);
            return sum;
        }
    }
}
