// Serializer.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Newtonsoft.Json;
using System;
using System.IO;

namespace FlowLab.Core
{
    public class Serializer
    {
        public readonly string RootPath;

        public Serializer(string gameName)
        {
            string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            RootPath = Path.Combine(AppDataPath, gameName);
            CreateFolder(RootPath);
        }

        public void CreateFolder(string relativePath)
        {
            DirectoryInfo createFDirectoryInfo = new(Path.Combine(RootPath, relativePath));
            if (createFDirectoryInfo.Exists) return;
            createFDirectoryInfo.Create();
        }

        public bool FileExist(string relativePath) => File.Exists(Path.Combine(RootPath, relativePath));

        public bool DeleteFile(string relativePath)
        {
            var filePath = Path.Combine(RootPath, relativePath);
            if (!File.Exists(filePath)) return false;
            File.Delete(filePath);
            return true;
        }

        public StreamWriter GetStreamWriter(string relativePath)
        {
            return new StreamWriter(Path.Combine(RootPath, relativePath));
        }

        public void SerializeObject(object obj, string relativePath)
        {
            JsonSerializerSettings jsonSerializerSettings = new()
            {
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            };

            var path = Path.Combine(RootPath, relativePath);
            var jsonObject = JsonConvert.SerializeObject(obj, jsonSerializerSettings);
            File.WriteAllText(path, jsonObject);
        }

        public object PopulateObject(object obj, string relativePath)
        {
            var filePath = Path.Combine(RootPath, relativePath);
            if (!File.Exists(filePath)) throw new FileNotFoundException();
            var jsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                NullValueHandling = NullValueHandling.Ignore,
            };
            using StreamReader streamReader = new(filePath);
            string json = streamReader.ReadToEnd();
            JsonConvert.PopulateObject(json, obj, jsonSerializerSettings);
            return obj;
        }
    }
}
