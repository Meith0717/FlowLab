// ColorPicker.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework;

namespace FlowLab.Monitoring.SensorPlanes;

public static class ColorPicker
{
    public static Color GetJetColor(float value)
    {
        if (value < 0.125f)
            return new Color((byte)0, (byte)(127.5f + 127.5f * (4f * value)), (byte)255);
        if (value < 0.375f)
            return new Color((byte)0, (byte)255, (byte)(255 - 255f * (4f * (value - 0.125f))));
        if (value < 0.625f)
            return new Color((byte)(255f * (4f * (value - 0.375f))), (byte)255, (byte)0);
        if (value < 0.875f)
            return new Color((byte)255, (byte)(255 - 255f * (4f * (value - 0.625f))), (byte)0);
        return new Color((byte)(255 - 127.5f * (4f * (value - 0.875f))), (byte)0, (byte)0);
    }

    public static Color GetGrayscaleColor(float value)
    {
        var b = (byte)(value * 255);
        return new Color(b, b, b);
    }

    public static Color GetViridisColor(float value)
    {
        if (value < 0.5f)
        {
            var c = value * 2f;
            return new Color((byte)(128f * c), (byte)(64f + 64f * c), (byte)(192f - 64f * c));
        }
        var c2 = (value - 0.5f) * 2f;
        return new Color((byte)(128f + 127f * c2), (byte)(192f - 64f * c2), (byte)(64f - 64f * c2));
    }

    public static Color GetHotColor(float value)
    {
        if (value < 0.333f)
            return new Color((byte)(255f * (value * 3f)), (byte)0, (byte)0);
        if (value < 0.666f)
            return new Color((byte)255, (byte)(255f * ((value - 0.333f) * 3f)), (byte)0);
        return new Color((byte)255, (byte)255, (byte)(255f * ((value - 0.666f) * 3f)));
    }
}
