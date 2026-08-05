// MessageDisplayer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Screens.Ui;

public class MessageDisplayer(GraphicsDevice graphicsDevice, int defaultDisplayTimeMs)
{
    private class Message(double displayTimeMs, string Text)
    {
        public double DisplayTimeMs = displayTimeMs;
        public readonly string Text = Text;
    }

    private readonly List<Message> _messages = [new Message(5000, "Messages active")];
    private readonly List<Message> _removedMessages = [];
    private SpriteFont _spriteFont;
    private float _uiScale;

    public void LoadContent(SpriteFont spriteFont) => _spriteFont = spriteFont;

    public void AddMessage(string text) => _messages.Add(new Message(defaultDisplayTimeMs, text));

    public void Update(double elapsedMilliseconds, float uiScale)
    {
        _uiScale = uiScale;

        foreach (var message in _messages)
        {
            message.DisplayTimeMs -= elapsedMilliseconds;
            message.DisplayTimeMs = double.Max(0, message.DisplayTimeMs);
            if (message.DisplayTimeMs > 0)
                continue;
            _removedMessages.Add(message);
        }

        foreach (var removedMessage in _removedMessages)
            _messages.Remove(removedMessage);
        _removedMessages.Clear();
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        const float scale = .2f;
        var topMid = new Vector2(graphicsDevice.Viewport.Width / 2f, 20);
        spriteBatch.Begin();
        for (int i = 0; i < _messages.Count; i++)
        {
            var message = _messages[i];
            var textSize = _spriteFont.MeasureString(message.Text) * scale * _uiScale;
            topMid.Y += textSize.Y * 1.2f;
            var position = topMid + new Vector2(-textSize.X / 2f, textSize.Y);
            spriteBatch.DrawString(
                _spriteFont,
                message.Text,
                position,
                Color.Yellow,
                0,
                Vector2.Zero,
                scale * _uiScale,
                SpriteEffects.None,
                0f
            );
        }
        spriteBatch.End();
    }
}
