// ConfigsManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Core.Extensions;
using System.Collections.Generic;
using System.Text.Json;

namespace FlowLab.Core.ContentHandling
{
    public class ConfigsManager
    {
        private readonly Dictionary<string, Dictionary<string, Dictionary<string, JsonElement>>> _configs = new();

        public void Add(string type, string objectID, Dictionary<string, JsonElement> config)
            => _configs.GetOrAdd(type, () => new()).Add(objectID, config);

        public Dictionary<string, JsonElement> Get(string type, string objectId)
            => _configs[type][objectId];

        public IEnumerable<string> GetIDsOfType(string type) => _configs[type].Keys;
    }
}
