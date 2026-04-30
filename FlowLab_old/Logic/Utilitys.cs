// Utilitys.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlowLab.Logic
{
    internal static class Utilitys
    {
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
