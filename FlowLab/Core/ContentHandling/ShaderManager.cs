// ShaderManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FlowLab.Core.ContentHandling
{
    public sealed class ShaderManager
    {
        private static ShaderManager _instance;
        public static ShaderManager Instance
            => _instance is null ? _instance = new() : _instance;
        private readonly Dictionary<string, Effect> mShaders = new();

        public void LoadBuildContent(ContentManager content, string ID, string path)
        {
            var effect = content.Load<Effect>(path);
            mShaders.Add(ID, effect);
        }

        public Effect GetEffect(string ID) => mShaders[ID];
    }
}
