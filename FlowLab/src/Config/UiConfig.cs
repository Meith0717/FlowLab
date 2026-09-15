// UiConfig.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;

namespace FlowLab.Config;

public static class UiConfig
{
    public static SpriteFont DefaultSpriteFont => ContentProvider.Get<SpriteFont>("defaultFont");

    public static Texture2D DefaultSelectorLeftTexture => ContentProvider.Get<Texture2D>("arrowL");
    public static Texture2D DefaultSelectorRightTexture => ContentProvider.Get<Texture2D>("arrowR");
    public static Texture2D EditButtonTexture => ContentProvider.Get<Texture2D>("edit");
    public static Texture2D AddButtonTexture => ContentProvider.Get<Texture2D>("add");
}
