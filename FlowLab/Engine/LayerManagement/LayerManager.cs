// LayerManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Engine.LayerManagement;

public class LayerManager
{
    private readonly Game1 _game1;

    // layer stack
    private readonly LinkedList<Layer> _layerStack = new();
    private readonly List<Layer> _addedLayers = new();

    public LayerManager(Game1 game1)
    {
        _game1 = game1;
        var screenSize = _game1.GraphicsDevice.Viewport.Bounds.Size;
    }

    // add and remove layers from stack
    public void AddLayer(Layer layer) => _addedLayers.Add(layer);

    public void PopLayer()
    {
        if (_layerStack.First is null) return;
        _layerStack.First.Value.Dispose();
        _layerStack.RemoveFirst();
    }

    public void RemoveAllToItem(Layer layer)
    {
        if (layer == null) return;
        if (!_layerStack.Contains(layer)) return;
        var firstLayer = _layerStack.First();
        while (firstLayer != layer)
        {
            PopLayer();
            firstLayer = _layerStack.First();
        }
        PopLayer();
    }

    // update layers
    public void Update(GameTime gameTime, InputState inputState)
    {
        var addedLayers = _addedLayers.ToList();
        _addedLayers.Clear();
        foreach (var layer in addedLayers)
        {
            _layerStack.AddFirst(layer);
            layer.Initialize();
            layer.ApplyResolution(gameTime);
        }

        foreach (Layer layer in _layerStack.ToArray())
        {
            layer.Update(gameTime, inputState);
            if (!layer.UpdateBelow) break;
        }
    }

    // draw layers
    private readonly LinkedList<RenderTarget2D> _renderTargets = new();
    private readonly Dictionary<RenderTarget2D, Effect> _layerEffects = new();
    public void Draw(SpriteBatch spriteBatch)
    {
        if (_layerStack.Count == 0) return;
        var topLayer = _layerStack.First();
        var topRenderTarget = topLayer.RenderTarget(spriteBatch);

        if (topLayer.DrawBelow)
        {
            foreach (Layer layer in _layerStack)
            {
                if (layer == topLayer) continue;
                var renderTarget = layer.RenderTarget(spriteBatch);
                _renderTargets.AddFirst(renderTarget);
                _layerEffects.Add(renderTarget, layer.Effect);
                if (!layer.DrawBelow) break;
            }

            // Set Blur Render Target
            _game1.GraphicsDevice.SetRenderTarget(null);
            _game1.GraphicsDevice.Clear(Color.Transparent);
            foreach (var renderTarget in _renderTargets)
            {
                // Draw on Blur Render Target
                spriteBatch.Begin();
                spriteBatch.Draw(renderTarget, _game1.GraphicsDevice.Viewport.Bounds, Color.White);
                spriteBatch.End();
            }
            _renderTargets.Clear();
            _layerEffects.Clear();
        }

        spriteBatch.Begin(effect: topLayer.Effect);
        spriteBatch.Draw(topRenderTarget, _game1.GraphicsDevice.Viewport.Bounds, Color.White);
        spriteBatch.End();
    }

    // lifecycle methods
    public void Exit()
    {
        foreach (Layer layer in _layerStack)
            layer.Dispose();
        _game1.Exit();
    }

    // fullScreen stuff
    public void OnResolutionChanged(GameTime gameTime)
    {
        foreach (Layer layer in _layerStack)
            layer.ApplyResolution(gameTime);
    }

    public bool ContainsLayer(Layer layer) => _layerStack.Contains(layer);
}