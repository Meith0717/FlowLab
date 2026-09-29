// UiConfig.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;

namespace FlowLab.Config;

public class UiConfig(ContentProvider contentProvider)
{
    public SpriteFont DefaultSpriteFont => contentProvider.Get<SpriteFont>("defaultFont");
    public Texture2D DefaultSelectorLeftTexture => contentProvider.Get<Texture2D>("arrowL");
    public Texture2D DefaultSelectorRightTexture => contentProvider.Get<Texture2D>("arrowR");
    public Texture2D EditButtonTexture => contentProvider.Get<Texture2D>("edit");
    public Texture2D AddButtonTexture => contentProvider.Get<Texture2D>("add");
}
