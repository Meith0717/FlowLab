// TextureManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.IO;

namespace FlowLab.Core.ContentHandling
{
    public sealed class TextureManager
    {
        private static TextureManager _instance;

        public static TextureManager Instance
            => _instance is null ? _instance = new() : _instance;

        public readonly static int MaxLayerDepth = 10000;

        private readonly Dictionary<string, Texture2D> _textures = new();
        private readonly Dictionary<string, SpriteFont> _spriteFonts = new();
        public SpriteBatch SpriteBatch { get; private set; }
        public GraphicsDevice GraphicsDevice { get; private set; }

        public void SetSpriteBatch(SpriteBatch spriteBatch)
        {
            if (spriteBatch is null) throw new Exception();
            SpriteBatch = spriteBatch;
        }

        public void SetGraphicsDevice(GraphicsDevice graphicsDevice)
        {
            if (graphicsDevice is null) throw new Exception();
            GraphicsDevice = graphicsDevice;
        }

        public void LoadBuildTextureContent(ContentManager content, string id, string filePath)
        {
            Texture2D texture = content.Load<Texture2D>(filePath);
            _textures.Add(id, texture);
        }

        public void LoadBuildFontContent(ContentManager content, string id, string filePath)
        {
            SpriteFont spriteFont = content.Load<SpriteFont>(filePath);
            _spriteFonts.Add(id, spriteFont);
        }

        public void LoadCleanTextureContent(string id, string filePath)
        {
            using FileStream f = new(filePath, FileMode.Open);
            var texture = Texture2D.FromStream(GraphicsDevice, f);
            _textures.Add(id, texture);
        }

        public Texture2D GetTexture(string id)
        {
            if (_textures.TryGetValue(id, out var texture))
                return texture;
            if (_textures.TryGetValue("missingContent", out texture))
                return texture;
            throw new Exception($"Texture {id} wasn't found.");
        }

        public SpriteFont GetFont(string id)
        {
            SpriteFont spriteFont = _spriteFonts[id];
            if (spriteFont == null)
                throw new Exception($"Error, Font {id} was not found in TextureManager");
            return spriteFont;
        }

        public float GetDepth(int textureDepth)
        {
            var depth = textureDepth / (float)MaxLayerDepth;
            if (depth > 1) throw new Exception();
            return depth;
        }

        #region Draw
        // render Textures ___________________________________________________________________________

        public void Draw(string id, Vector2 position, float width, float height, Vector2 offset, float rotation)
        {
            SpriteBatch.Draw(GetTexture(id), new RectangleF(position.X + offset.X, position.Y + offset.Y, width, height).ToRectangle(), null, Color.White, rotation, new(GetTexture(id).Width / 2, GetTexture(id).Height / 2), SpriteEffects.None, 1);
        }

        public void Draw(string id, Vector2 position, float width, float height, Color color)
        {
            SpriteBatch.Draw(GetTexture(id), new RectangleF(position.X, position.Y, width, height).ToRectangle(), null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        public void Draw(string id, Vector2 position, Vector2 offset, float sclae, float rotation, int depth, Color color)
        {
            SpriteBatch.Draw(GetTexture(id), position, null, color, rotation, offset, sclae, SpriteEffects.None, GetDepth(depth));
        }
        public void Draw(string id, Vector2 position, float sclae, float rotation, int depth, Color color)
        {
            Texture2D texture = GetTexture(id);
            Vector2 offset = new(texture.Width / 2, texture.Height / 2);
            SpriteBatch.Draw(GetTexture(id), position, null, color, rotation, offset, sclae, SpriteEffects.None, GetDepth(depth));
        }

        // render Game Objects ___________________________________________________________________________

        // render String
        public void DrawString(string id, Vector2 position, string text, float scale, Color color)
        {
            SpriteBatch.DrawString(GetFont(id), text, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, 1);
        }

        public void DrawAdaptiveCircle(Vector2 center, float radius, Color color, float thickness, int depth, float zoom)
        {
            SpriteBatch.DrawCircle(center, radius, 100, color, thickness / zoom, GetDepth(depth));
        }

        // render Rectangle ___________________________________________________________________________
        public void DrawRectangleF(RectangleF rectangle, Color color, float thickness, int depth)
        {
            SpriteBatch.DrawRectangle(rectangle, color, thickness, GetDepth(depth));
        }

        public void DrawAdaptiveRectangleF(RectangleF rectangle, Color color, float thickness, int depth, float zoom)
        {
            SpriteBatch.DrawRectangle(rectangle, color, thickness / zoom, GetDepth(depth));
        }

        // render Line ___________________________________________________________________________

        public void DrawAdaptiveLine(Vector2 start, Vector2 end, Color color, float thickness, int depth, float zoom)
        {
            SpriteBatch.DrawLine(start, end, color, thickness / zoom, GetDepth(depth));
        }
        #endregion
    }
}