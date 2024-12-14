// Scenario.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    /// <summary>
    /// A scenario is construct out of Bodys containing border Particles
    /// </summary>
    internal class Scenario(List<Body> bodys)
    {
        private readonly List<Body> _bodys = bodys;

        public void AddBody(Body body) 
            => _bodys.Add(body);

        public void RemoveBody(Body body)
            => _bodys.Remove(body);

        public void Load(ParticleManager particleManager)
        {
            var fluidDensity = particleManager.FluidDensity;
            var particleSize = particleManager.ParticleDiameter;
            foreach (Body body in _bodys)
            {
                body.Construct(particleSize, fluidDensity);
                body.Load(particleManager);
            }
        }

        public void Update()
        {
            foreach (var body in _bodys) ;
                // TODO
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (var body in _bodys)
                body.Draw(spriteBatch);
        }
    }
}
