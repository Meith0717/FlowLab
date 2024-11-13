// DirectoryExtension.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using System.IO;

namespace FlowLab.Core.Extensions
{
    internal static class DirectoryExtension
    {
        public static string[] GetFilesInDirectory(string directoryPath)
            => Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
    }
}
