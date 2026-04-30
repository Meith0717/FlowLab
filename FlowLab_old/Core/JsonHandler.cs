// JsonHandler.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FlowLab.Core
{
    internal static class JsonHandler
    {
        public static Dictionary<string, JsonElement> LoadJsonInDictionary(string filePath)
        {
            using StreamReader reader = new(filePath);
            string jsonString = reader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString, options);
            return dict;
        }
    }
}
