// FlowLab - GaussianKernel.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;

namespace FlowLab;

internal class GaussianKernel
{
    public static float[] Get1D(int radius, double sigma)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be non-negative.");
        if (sigma <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be positive.");

        var length = radius * 2 + 1;
        var kernel = new float[length];

        var twoSigmaSq = 2.0 * sigma * sigma;
        var sum = 0.0;

        for (var x = -radius; x <= radius; x++)
        {
            var w = Math.Exp(-(x * x) / twoSigmaSq);
            kernel[x + radius] = (float)w;
            sum += w;
        }

        var invSum = (float)(1.0 / sum);
        for (var i = 0; i < length; i++)
            kernel[i] *= invSum;

        return kernel;
    }
}
