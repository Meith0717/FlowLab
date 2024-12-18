// PersistenceManager.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using FlowLab.Logic.ScenarioManagement;
using System;
using System.IO;
using System.Threading;

namespace FlowLab.Core
{
    public class PersistenceManager
    {
        public readonly Serializer Serializer;
        private static readonly string _settingsDirectory = "settings";
        private static readonly string _screenShotDirectory = "screenshots";
        public static readonly string TempDirectory = "tmp";
        public static readonly string VideoDirectory = "videos";

        public static string SettingsFilePath => Path.Combine(_settingsDirectory, "settings.json");
        public static string VideoFilePath => Path.Combine(VideoDirectory, $"{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.mp4");
        public static string ScreenshotFilePath => Path.Combine(_screenShotDirectory, $"{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.png");

        public PersistenceManager(string rootFolder)
        {
            Serializer = new(rootFolder);
            Serializer.CreateFolder(_settingsDirectory);
            Serializer.CreateFolder(_screenShotDirectory);
            Serializer.CreateFolder(TempDirectory);
            Serializer.CreateFolder(VideoDirectory);
        }

        public void Load<Object>(string path, Action<Object> onLoadComplete, Action<Exception> onError) where Object : new()
        {
            try
            {
                Object @object = new();
                @object = (Object)Serializer.PopulateObject(@object, path);
                onLoadComplete?.Invoke(@object);
            }
            catch (Exception e) { onError?.Invoke(e); }
        }

        public void Save(string path, object @object, Action onSaveComplete, Action<Exception> onError)
        {
            try
            {
                if (@object is null) throw new Exception();
                Serializer.SerializeObject(@object, path);
                onSaveComplete?.Invoke();
            }
            catch (Exception e) { onError?.Invoke(e); }
        }

        public void LoadAsync<Object>(Object newObject, string path, Action<Object> onLoadComplete, Action<Exception> onError, Action onAllways = null)
        {
            Thread loadThread = new(() =>
            {
                try
                {
                    newObject = (Object)Serializer.PopulateObject(newObject, path);
                    onLoadComplete?.Invoke(newObject);
                }
                catch (Exception e) { onError?.Invoke(e); }
                onAllways?.Invoke();
            });

            loadThread.Start();
        }

        public void SaveAsync(string path, object @object, Action onSaveComplete, Action<Exception> onError)
        {
            Thread saveThread = new(() =>
            {
                try
                {
                    if (@object is null) throw new Exception();
                    Serializer.SerializeObject(@object, path);
                    onSaveComplete?.Invoke();
                }
                catch (Exception e) { onError?.Invoke(e); }
            });

            saveThread.Start();
        }
    }

}
