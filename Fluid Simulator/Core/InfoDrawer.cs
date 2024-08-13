using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StellarLiberation.Game.Core.CoreProceses.InputManagement;
using System.Collections.Generic;
using System.Linq;

namespace Fluid_Simulator.Core
{
    internal class InfoDrawer
    {
        public static float TextScale = .15f;
        private const float TextureScale = .8f;

        private readonly Dictionary<Texture2D, string> _keyBinds = new();
        private bool _showHelp;
        private Texture2D _helpTexture;

        private double _messageTime;
        private string _message = "";
        private Color _messageColor = Color.White;

        public void LoadContent(ContentManager content)
        {
            _helpTexture = content.Load<Texture2D>("h");
            _keyBinds.Add(content.Load<Texture2D>("scroll"), "Zoom in/out");
            _keyBinds.Add(content.Load<Texture2D>("escape"), "Exit");
            _keyBinds.Add(content.Load<Texture2D>("F1"), "Save data");
            _keyBinds.Add(content.Load<Texture2D>("F2"), "Collect Data");
            _keyBinds.Add(content.Load<Texture2D>("F12"), "Take screenshot");
            _keyBinds.Add(content.Load<Texture2D>("v"), "Change scene");
            _keyBinds.Add(content.Load<Texture2D>("c"), "Change color");
            _keyBinds.Add(content.Load<Texture2D>("q"), "Change particle shape");
            _keyBinds.Add(content.Load<Texture2D>("w"), "Increase shape height");
            _keyBinds.Add(content.Load<Texture2D>("s"), "Decrease shape height");
            _keyBinds.Add(content.Load<Texture2D>("a"), "Derease shape width");
            _keyBinds.Add(content.Load<Texture2D>("d"), "Incease shape width");
            _keyBinds.Add(content.Load<Texture2D>("delete"), "Clear particles");
            _keyBinds.Add(content.Load<Texture2D>("space"), "Pause/Resume");
        }

        public void AddMessage(string message, Color color)
        {
            _message = message;
            _messageColor = color;
            _messageTime = 0;
        }

        public void Update(GameTime gameTime, InputState inputState)
        {
            _messageTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_messageTime > 1500)
            {
                _message = "";
                _messageColor = Color.White;
                _messageTime = 0;
            }
            inputState.DoAction(ActionType.Help, () => _showHelp = !_showHelp);
        }

        public void DrawKeyBinds(SpriteBatch spriteBatch, SpriteFont spriteFont, Color color, int screenHeight)
        {
            var position = new Vector2(5, screenHeight - 40);
            if (!_showHelp)
            {
                spriteBatch.Draw(_helpTexture, position, null, Color.White, 0, Vector2.Zero, TextureScale, SpriteEffects.None, 1);
                spriteBatch.DrawString(spriteFont, " - Help", position + new Vector2((_helpTexture.Width * TextureScale) + 10, 5), color, 0, Vector2.Zero, TextScale, SpriteEffects.None, 1);
                return;
            }

            foreach (var keyValuePair in _keyBinds.Reverse())
            {
                var text = keyValuePair.Value;
                var texture = keyValuePair.Key;

                spriteBatch.Draw(texture, position, null, Color.White, 0, Vector2.Zero, TextureScale, SpriteEffects.None, 1);
                spriteBatch.DrawString(spriteFont, $" - {text}", position + new Vector2((texture.Width * TextureScale) + 10, 5), color, 0, Vector2.Zero, TextScale, SpriteEffects.None, 1);
                position.Y -= 40;
            }
        }

        public void DrawMesage(SpriteFont spriteFont, SpriteBatch spriteBatch, Rectangle viewBound)
        {
            var center = viewBound.Center.ToVector2();
            var stringDimension = spriteFont.MeasureString(_message);
            var position = center - (stringDimension / 2f);
            spriteBatch.DrawString(spriteFont, _message, position, _messageColor, 0, Vector2.Zero, 1f, SpriteEffects.None, 1);
        }

        public void DrawProperties(SpriteBatch spriteBatch, SpriteFont spriteFont, Color color, int screenWidth, float timeStep, float fluidStiffness, float fluidViscosity)
        {
            var data = new List<string>()
            {
                $"Time Step: {timeStep}",
                $"Stiffness: {fluidStiffness}",
                $"Viscosity: {fluidViscosity}"
            };

            var y = 5;
            foreach (var text in data)
            {
                var textDimension = spriteFont.MeasureString(text) * TextScale;
                var position = new Vector2(screenWidth - textDimension.X - 10, y);

                spriteBatch.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, TextScale, SpriteEffects.None, 1);
                y += 25;
            }
        }
    }
}
