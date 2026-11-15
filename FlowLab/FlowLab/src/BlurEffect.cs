// BlurEffect.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.App.Rendering;

namespace FlowLab;

public class BlurEffect(Effect blurShader) : IPostProcessingEffect
{
    public RenderTarget2D Apply(
        SpriteBatch spriteBatch,
        GraphicsDevice graphicsDevice,
        PostProcessingRunner pipeline,
        RenderTarget2D input
    )
    {
        // Step 1: Apply horizontal Gaussian blur to smooth bright areas
        blurShader.Parameters["kernel"].SetValue(GaussianKernel.Get1D(20, 10f));
        blurShader
            .Parameters["texelSize"]
            .SetValue(new Vector2(1f / input.Width, 1f / input.Height));
        blurShader.CurrentTechnique = blurShader.Techniques["GaussianBlurH"];
        var blurH = pipeline.Pass(blurShader, spriteBatch, graphicsDevice, input);

        // Step 2: Apply vertical Gaussian blur to complete the 2D blur
        blurShader.CurrentTechnique = blurShader.Techniques["GaussianBlurV"];
        return pipeline.Pass(blurShader, spriteBatch, graphicsDevice, blurH);
    }
}
