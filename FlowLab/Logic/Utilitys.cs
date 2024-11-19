// Utilitys.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace FlowLab.Logic
{
    internal static class Utilitys
    {
        public static float Sum<T>(IEnumerable<T> scource, Func<T, float> body)
        {
            var sum = 0f;
            foreach (var item in scource)
                sum += body(item);
            return sum;
        }

        public static Vector2 Sum<T>(IEnumerable<T> scource, Func<T, Vector2> body)
        {
            var sum = Vector2.Zero;
            foreach (var item in scource)
                sum += body(item);
            return sum;
        }

        public static Dictionary<string, JsonElement> LoadJsonDictionary(string filePath)
        {
            using StreamReader reader = new(filePath);
            string jsonString = reader.ReadToEnd();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString, options);
            return dict;
        }

        public static void ForEach<T>(bool parallel, IEnumerable<T> scource, Action<T> body)
        {
            if (parallel)
                Parallel.ForEach(scource, body);
            else
                foreach (var element in scource)
                    body.Invoke(element);
        }
    }
}
