// Scenario.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ParticleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FlowLab.Logic.ScenarioManagement
{
    [Serializable]
    internal class Scenario()
    {
        [JsonProperty] public string Name;
        [JsonProperty] public readonly List<Body> Bodies = [];

        [JsonIgnore] public bool Saved { get; set; } = true;

        [JsonIgnore]
        public bool IsEmpty
            => Bodies.Count == 0;

        public void AddBody(Body body)
        {
            Bodies.Add(body);
            Saved = false;
        }

        public void RemoveBody(Body body)
        {
            Bodies.Remove(body);
            Saved = false;
        }

        public void Load(ParticleManager particleManager)
            => Utilitys.ForEach(false, Bodies, body => body.Load(particleManager));

        public void Update(float timeStep)
            => Utilitys.ForEach(false, Bodies, body => body.Update(timeStep));

        public void Draw(SpriteBatch spriteBatch)
            => Utilitys.ForEach(false, Bodies, body => body.Draw(spriteBatch, Color.White));
    }
}
