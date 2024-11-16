// Layer.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core;
using FlowLab.Core.InputManagement;
using FlowLab.Game.Engine.UserInterface;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;

namespace FlowLab.Engine.LayerManagement;

[Serializable]
public abstract class Layer : IDisposable
{
    [JsonIgnore] public readonly bool UpdateBelow;
    [JsonIgnore] public readonly bool DrawBelow;
    [JsonIgnore] public readonly LayerManager LayerManager;
    [JsonIgnore] protected readonly Game1 Game1;
    [JsonIgnore] protected readonly GraphicsDevice GraphicsDevice;
    [JsonIgnore] protected readonly PersistenceManager PersistenceManager;
    [JsonIgnore] private RenderTarget2D _renderTarget2D;

    [JsonIgnore] private readonly Canvas _layerCanvas = new();
    [JsonIgnore] public readonly UiLayer UiRoot;

    [JsonIgnore] public Effect Effect { get; protected set; }

    protected Layer(Game1 game1, bool updateBelow, bool drawBelow)
    {
        Game1 = game1;
        LayerManager = game1.LayerManager;
        GraphicsDevice = game1.GraphicsDevice;
        PersistenceManager = game1.PersistenceManager;
        UpdateBelow = updateBelow;
        DrawBelow = drawBelow;

        UiRoot = new(_layerCanvas) { Alpha = 0 };
        UiRoot.Place(fillScale: FillScale.Both, anchor: Anchor.Center);

        _renderTarget2D = new(game1.GraphicsDevice,
            game1.GraphicsManager.PreferredBackBufferWidth,
            game1.GraphicsManager.PreferredBackBufferHeight,
            false,
            GraphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24,
            0,
            RenderTargetUsage.DiscardContents);
    }

    public virtual void Initialize() {; }

    public virtual void Update(GameTime gameTime, InputState inputState)
    {
        UiRoot.Update(inputState, inputState.MousePosition, gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch) {; }

    public RenderTarget2D RenderTarget(SpriteBatch spriteBatch)
    {
        UiRoot.Render(GraphicsDevice, spriteBatch);
        GraphicsDevice.SetRenderTarget(_renderTarget2D);
        GraphicsDevice.Clear(Color.Transparent);
        Draw(spriteBatch);
        spriteBatch.Begin();
        UiRoot.Draw(spriteBatch);
        spriteBatch.End();
        GraphicsDevice.SetRenderTarget(null);
        return _renderTarget2D;
    }

    public virtual void ApplyResolution(GameTime gameTime)
    {
        _layerCanvas.UpdateFrame(GraphicsDevice.Viewport.Bounds, 1, fillScale: FillScale.Both);
        UiRoot.ApplyResolution(1);

        _renderTarget2D.Dispose();
        _renderTarget2D = new(Game1.GraphicsDevice, Game1.GraphicsDevice.Viewport.Width, Game1.GraphicsDevice.Viewport.Height, false, GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24, 0, RenderTargetUsage.DiscardContents);
    }

    public virtual void Dispose()
    {
        _renderTarget2D.Dispose();
        _renderTarget2D = null;
        UiRoot.Dispose();
    }
}