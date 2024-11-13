// UiSprite.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Game.Engine.UserInterface.Components
{
    internal class UiSprite : UiElement
    {
        private readonly Texture2D _texture;

        public UiSprite(UiLayer root, string texture) : base(root)
        {
            _texture = TextureManager.Instance.GetTexture(texture);
        }

        public UiSprite(Canvas root, string texture) : base(root)
        {
            _texture = TextureManager.Instance.GetTexture(texture);
        }

        public override void Place(int? x = null, int? y = null, int? width = null, int? height = null, float relX = 0, float relY = 0, float relWidth = 0.1F, float relHeight = 0.1F, int? hSpace = null, int? vSpace = null, Anchor anchor = Anchor.None, FillScale fillScale = FillScale.None)
        {
            var textureDimension = new Vector2(_texture.Width, _texture.Height) * Scale;
            textureDimension.Ceiling();
            var textureSize = textureDimension.ToPoint();
            base.Place(x, y, textureSize.X, textureSize.Y, relX, relY, relWidth, relHeight, hSpace, vSpace, anchor, fillScale);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, Canvas.GetRelativeBounds(), Color * Alpha);
            base.Draw(spriteBatch);
        }

        public float Scale { private get; set; } = 1;
        public Color Color { private get; set; } = Color.White;
        public float Alpha { private get; set; } = 1;
    }

}
