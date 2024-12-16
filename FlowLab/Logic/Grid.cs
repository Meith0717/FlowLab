// SceneGrid.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace FlowLab.Logic
{
    internal class Grid(float particleSize)
    {
        private readonly float _particleSize = particleSize;

        public Point GetCell(Vector2 position) 
            => Vector2.Floor(position / _particleSize).ToPoint();

        public Vector2 GetCellPosition(Vector2 position)
            => GetCell(position).ToVector2() * _particleSize;

        public Vector2 GetCellCenter(Vector2 position)
            => GetCellPosition(position) + new Vector2(_particleSize) / 2f;

        public void DrawCell(SpriteBatch spriteBatch, Vector2 position, Color color) 
            => spriteBatch.FillRectangle(GetCellPosition(position), new(_particleSize, _particleSize), color);

        public void Draw(SpriteBatch spriteBatch, RectangleF cameraBounds, Vector2? debugPosition)
        {
            spriteBatch.DrawLine(Vector2.Zero, new(0, 20), Color.Green);
            spriteBatch.DrawLine(Vector2.Zero, new(20, 0), Color.Red);

            var color = Color.White * .05f;
            for (var x = 0f; x < cameraBounds.Right; x += _particleSize)
                spriteBatch.DrawLine(new(x, cameraBounds.Top), new(x, cameraBounds.Bottom), color, 1);

            for (var y = 0f; y < cameraBounds.Bottom; y += _particleSize)
                spriteBatch.DrawLine(new(cameraBounds.Left, y), new(cameraBounds.Right, y), color, 1);

            for (var x = -_particleSize; x > cameraBounds.Left; x -= _particleSize)
                spriteBatch.DrawLine(new(x, cameraBounds.Bottom), new(x, cameraBounds.Top), color, 1);

            for (var y = -_particleSize; y > cameraBounds.Top; y -= _particleSize)
                spriteBatch.DrawLine(new(cameraBounds.Right, y), new(cameraBounds.Left, y), color, 1);

            if (debugPosition is null) return;
            DrawCell(spriteBatch, debugPosition.Value, Color.Red);
        }
    }
}
