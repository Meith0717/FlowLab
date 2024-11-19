// ListExtension.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowLab.Core.Extensions
{
    internal static class ListExtension
    {
        public static string ToString<T>(this List<T> value) => value.Count().ToString();
    }
}
