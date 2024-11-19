// UiLayer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.InputManagement;
using FlowLab.Game.Engine.UserInterface.Components;
using FlowLab.Game.Engine.UserInterface.Utilities;
using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace FlowLab.Game.Engine.UserInterface
{
    public class UiLayer : UiElement
    {
        private readonly List<UiElement> _elementChilds = new();
        private readonly UiScrollBar _scrollBar;
        private RenderTarget2D _renderTarget;
        private bool _wasRenderUpdated;
        private Matrix _scrollMatrix;

        private int _actualVirtualHeight;

        public Matrix ScrollMatrix => _scrollMatrix;

        public UiLayer(UiLayer root) : base(root.Canvas)
        {
            root.Add(this);
            _scrollBar = new(this);
        }

        public UiLayer(Canvas canvas) : base(canvas)
        {
            _scrollBar = new(this);
        }

        public override void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = 0.1F, float relHeight = 0.1F, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {

            base.Place(x, y, width, height, relX, relY, relWidth, relHeight, hSpace, vSpace, anchor, fillScale);
            _scrollBar.Place(anchor: Anchor.E, relHeight: .9f, hSpace: 5, width: 10);
        }

        public void Add(UiElement child)
            => _elementChilds.Add(child);

        public void Add(UiLayer child)
            => _elementChilds.Add(child);

        public override void Update(InputState inputState, Vector2 transformedMousePosition, GameTime gameTime)
        {
            var mousePosition = Transformations.ScreenToWorld(_scrollMatrix, transformedMousePosition);

            var virtualHeight = 0;
            foreach (var child in _elementChilds)
            {
                var virtualChildBounds = child.Canvas.GetRelativeBounds();
                if (virtualHeight < virtualChildBounds.Bottom)
                    virtualHeight = virtualChildBounds.Bottom;

                virtualChildBounds = child.Canvas.GetGlobalBounds();
                virtualChildBounds.Y += _actualVirtualHeight;
                if (!Canvas.GetGlobalBounds().Intersects(virtualChildBounds)) continue;
                child.Update(inputState, mousePosition, gameTime);
            }

            _scrollBar.VirtualHeight = virtualHeight;
            _scrollBar.ApplyResolution(UiScale);
            _scrollBar.Update(inputState, transformedMousePosition, gameTime);
            if (Canvas.GetGlobalBounds().Contains(inputState.MousePosition))
                _scrollBar.ScrollByMouse(inputState);
            _scrollMatrix = Matrix.CreateTranslation(0, _actualVirtualHeight = _scrollBar.Value, 0);
        }

        public override void ApplyResolution(float uiScale)
        {
            base.ApplyResolution(uiScale);
            foreach (var child in _elementChilds)
                child.ApplyResolution(uiScale);

            _scrollBar.ApplyResolution(uiScale);
            _renderTarget?.Dispose();
            _renderTarget = null;
        }

        public void Render(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
        {
            _wasRenderUpdated = true;

            if (_renderTarget is null)
            {
                var canvas = Canvas.GetRelativeBounds();
                _renderTarget = new(graphicsDevice, canvas.Width, canvas.Height);
            }

            foreach (var layer in _elementChilds.OfType<UiLayer>())
                layer.Render(graphicsDevice, spriteBatch);

            graphicsDevice.SetRenderTarget(_renderTarget);
            graphicsDevice.Clear(Color.Transparent);
            spriteBatch.Begin(transformMatrix: _scrollMatrix, sortMode: SpriteSortMode.Immediate);

            foreach (var child in _elementChilds)
                child.Draw(spriteBatch);

            spriteBatch.End();

            spriteBatch.Begin();
            _scrollBar.Draw(spriteBatch);
            spriteBatch.End();
            graphicsDevice.SetRenderTarget(null);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!_wasRenderUpdated) throw new System.Exception("Render Layer before Drawing it");
            if (Alpha != 0 && InnerColor != Color.Transparent)
                LayerRectangle.Draw(spriteBatch, Canvas.GetRelativeBounds(), InnerColor * Alpha, BorderColor * Alpha, (int)(BorderSize * UiScale));
            spriteBatch.Draw(_renderTarget, Canvas.GetRelativeBounds(), Color.White);
            base.Draw(spriteBatch);
        }

        public Color InnerColor { private get; set; } = Color.White;
        public Color BorderColor { private get; set; } = Color.White;
        public float BorderSize { private get; set; } = 0;
        public float Alpha { private get; set; } = 1;
        public float TextureScale { private get; set; } = 1;

        public void Dispose()
        {
            foreach (var child in _elementChilds)
            {
                if (child is not UiLayer) continue;
                var layer = child as UiLayer;
                layer.Dispose();
            }
            _renderTarget?.Dispose();
        }
    }
}
