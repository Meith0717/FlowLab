// ParticelDebugger.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.ContentHandling;
using FlowLab.Core.InputManagement;
using FlowLab.Engine.Rendering;
using FlowLab.Engine.SpatialManagement;
using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FlowLab.Logic
{
    internal class ParticelDebugger(SpatialHashing spatialHashing)
    {
        private readonly SpatialHashing _spatialHashing = spatialHashing;
        private Particle _selectedParticle;
        private List<Particle> _particles = new();
        private bool _active = false;

        public Particle SelectedParticle => _selectedParticle;
        public bool IsSelected => _selectedParticle != null;

        public void Update(InputState inputState, Vector2 mousePosition, float particleSize, Camera2D camera)
        {
            inputState.DoAction(ActionType.Debugg, () => { _active = !_active; Clear(); });
            _particles.Clear();
            if (IsSelected) 
                camera.Position = SelectedParticle.Position;
            if (!_active) return; 
            _spatialHashing.InRadius(mousePosition, particleSize * 2, ref _particles);
            if (!inputState.ContainAction(ActionType.LeftClicked)) return;
            foreach (var particle in _particles)
            {
                if (!particle.BoundBox.Contains(mousePosition)) continue;
                _selectedParticle = (particle == _selectedParticle) ? null : particle;
                return;
            }
            _selectedParticle = null;
        }

        public void Clear() => _selectedParticle = null;

        public void DrawParticleInfo(SpriteBatch spriteBatch, Vector2 position)
        {
            if (!IsSelected) return;
            spriteBatch.Begin();
            var font = TextureManager.Instance.GetFont("consola");
            var props = typeof(Particle).GetProperties().Reverse();
            foreach (var prop in props)
            {
                object value = prop.GetValue(_selectedParticle, null);
                if (value is List<Particle> lst)
                    value = lst.Count();
                position.Y -= 20;
                spriteBatch.DrawString(font, $"{prop.Name}: {value}", position, Color.White, 0, Vector2.Zero, .15f, SpriteEffects.None, 1);
            }
            spriteBatch.End();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (!_active) return;
            _spatialHashing.Draw(spriteBatch);
            if (!IsSelected) return;
            spriteBatch.DrawCircle(SelectedParticle.Position, SelectedParticle.Diameter * 2, 30, Color.Red);
        }
    }
}
