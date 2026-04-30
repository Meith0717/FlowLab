// JsonElementExtension.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Text.Json;

namespace FlowLab.Core.Extensions
{
    internal static class JsonElementExtension
    {
        public static int[] GetInt32Array(this JsonElement jsonElement)
        {
            var arrayLength = jsonElement.GetArrayLength();
            int[] array = new int[arrayLength];
            for (int i = 0; i < arrayLength; i++)
                array[i] = jsonElement[i].GetInt32();
            return array;
        }

        public static Dictionary<string, JsonElement> GetDictionary(this JsonElement jsonElement)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonElement.GetRawText(), options);
        }

        public static Vector2 GetVector(this JsonElement jsonElement)
            => new Vector2(jsonElement.GetProperty("X").GetInt32(), jsonElement.GetProperty("Y").GetInt32());

        public static Color GetColor(this JsonElement jsonElement)
            => new Color(jsonElement.GetProperty("R").GetInt32(), jsonElement.GetProperty("G").GetInt32(), jsonElement.GetProperty("B").GetInt32(), jsonElement.GetProperty("A").GetInt32());

    }
}
