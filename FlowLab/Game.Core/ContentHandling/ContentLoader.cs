// ContentLoader.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework.Content;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace FlowLab.Core.ContentHandling
{
    public class ContentLoader(ContentManager content)
    {
        private readonly ContentManager _content = content;
        public string ProcessMessage { get; private set; } = "";
        public double Process { get; private set; } = 0;

        private static string[] GetFilesInDirectory(string directoryPath)
            => Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);

        private static string[] GetDirectoriesFromPath(string contentPath)
            => Directory.GetDirectories(contentPath, "*.*", SearchOption.TopDirectoryOnly);

        public void LoadEssenzialContentSerial()
        {
            TextureManager.Instance.LoadBuildTextureContent(_content, "missingContent", "missingContent");
            LoadBuildContentIntoManager("shaders", ShaderManager.Instance.LoadBuildContent);
            LoadBuildContentIntoManager("fonts", TextureManager.Instance.LoadBuildFontContent);
            LoadBuildContentIntoManager("gui", TextureManager.Instance.LoadBuildTextureContent);
        }

        public void LoadContentAsync(ConfigsManager configsManager, Action onLoadComplete, Action<Exception> onError)
        {
            Thread thread = new(() =>
            {
                try
                {
                    LoadBuildContentIntoManager("music", MusicManager.Instance.LoadBuildContent);
                    LoadBuildContentIntoManager("sfx", SoundEffectManager.Instance.LoadBuildContent);
                    LoadBuildContentIntoManager("textures", TextureManager.Instance.LoadBuildTextureContent);
                    LoadCleanContent(configsManager);
                    ProcessMessage = "Ready";
                }
                catch (Exception ex)
                {
                    onError?.Invoke(ex);
                }
                onLoadComplete.Invoke();
            });

            thread.Start();
        }

        private void LoadBuildContentIntoManager(string contentDirectory, Action<ContentManager, string, string> managerLoader)
        {
            var rootDirectory = Path.Combine(_content.RootDirectory, contentDirectory);
            var files = GetFilesInDirectory(rootDirectory);
            Process = 0;

            for (int i = 0; i < files.Length; i++)
            {
                var filePath = files[i];
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var split = filePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToList();
                if (split.Count > 1) split.RemoveAt(0);
                var directory = Path.GetDirectoryName(Path.Combine(split.ToArray()));
                var pathWithoutExtension = Path.Combine(directory, fileName);
                ProcessMessage = $"Loading: {filePath}";
                Process += 1d / files.Length;
                managerLoader.Invoke(_content, fileName, pathWithoutExtension);
            }
        }

        private void LoadCleanContent(ConfigsManager configsManager)
        {
            var contentDirectory = "object_configs";
            var rootDirectory = Path.Combine(_content.RootDirectory, contentDirectory);
            var objectsTypesConfigs = GetDirectoriesFromPath(rootDirectory);
            var filesAmount = GetFilesInDirectory(rootDirectory).Length;
            Process = 0;

            // go through the directories with all the types
            foreach (var objectTypeConfigs in objectsTypesConfigs)
            {
                var objectType = Path.GetFileName(objectTypeConfigs);
                var objectConfigs = GetDirectoriesFromPath(objectTypeConfigs);

                // go through the directories with all objects of this types
                foreach (var objectConfig in objectConfigs)
                {
                    var objectID = Path.GetFileName(objectConfig);
                    var files = GetFilesInDirectory(objectConfig);

                    // go through the files of the object
                    foreach (var file in files)
                    {
                        Process += 1d / filesAmount;
                        ProcessMessage = $"Loading: {file}";
                        LoadFile(configsManager, objectType, objectID, file);
                    }
                }
            }
        }

        private void LoadFile(ConfigsManager configsManager, string objectType, string objectID, string objectFile)
        {
            var fileExtension = Path.GetExtension(objectFile).ToLower();
            switch (fileExtension)
            {
                case ".json":
                    var jsonDirectory = JsonHandler.LoadJsonInDictionary(objectFile);
                    configsManager.Add(objectType, objectID, jsonDirectory);
                    break;
                case ".png":
                    var fileName = Path.GetFileNameWithoutExtension(objectFile).ToLower();
                    TextureManager.Instance.LoadCleanTextureContent($"{objectID}_{fileName}", objectFile);
                    break;
            }
        }
    }
}
