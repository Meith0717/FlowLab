// Scenario.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    /// <summary>
    /// A scenario is construct out of Bodys containing border Particles
    /// </summary>
    internal class Scenario(List<Body> bodys)
    {
        public readonly List<Body> Bodys = bodys;

        public bool IsEmpty => Bodys.Count == 0;

        public void AddBody(Body body)
            => Bodys.Add(body);

        public void RemoveBody(Body body)
            => Bodys.Remove(body);

        public void Load(ParticleManager particleManager)
        {
            foreach (Body body in Bodys)
                body.Load(particleManager);
        }

        public void Update()
        {
            foreach (var body in Bodys)
                body.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var body in Bodys)
                body.Draw(spriteBatch, Color.White);
        }
    }
}
