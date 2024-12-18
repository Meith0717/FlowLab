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
            string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            RootPath = Path.Combine(documentPath, gameName);
            CreateFolder(RootPath);
        }

        public string GetFullPath(string relativePath)
            => Path.Combine(RootPath, relativePath);

        public void CreateFolder(string relativePath)
        {
            DirectoryInfo directoryInfo = new(GetFullPath(relativePath));
            if (directoryInfo.Exists) return;
            directoryInfo.Create();
        }

        public FileInfo[] GetFilesInFolder(string folderPath)
        {
            DirectoryInfo directoryInfo = new(GetFullPath(folderPath));
            return directoryInfo.GetFiles();
        }

        public void ClearFolder(string relativePath)
        {
            Directory.Delete(Path.Combine(RootPath, relativePath), true);
            CreateFolder(relativePath);
        }

        public bool FileExist(string relativePath) 
            => File.Exists(Path.Combine(RootPath, relativePath));

        public bool DeleteFolder(string relativePath)
        {
            var filePath = Path.Combine(RootPath, relativePath);
            if (!Directory.Exists(filePath)) return false;
            Directory.Delete(filePath);
            return true;
        }
        public bool DeleteFile(string relativePath)
        {
            var filePath = Path.Combine(RootPath, relativePath);
            if (!File.Exists(filePath)) return false;
            File.Delete(filePath);
            return true;
        }

        public StreamWriter GetStreamWriter(string relativePath) 
            => new StreamWriter(Path.Combine(RootPath, relativePath));

        public FileStream GetFileStream(string relativePath, FileMode mode)
            => new FileStream(Path.Combine(RootPath, relativePath), mode);

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
