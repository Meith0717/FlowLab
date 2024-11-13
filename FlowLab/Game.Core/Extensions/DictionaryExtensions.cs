// DictionaryExtensions.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;

namespace FlowLab.Core.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;

            value = valueFactory.Invoke();
            dictionary[key] = value;
            return value;
        }
    }
}
