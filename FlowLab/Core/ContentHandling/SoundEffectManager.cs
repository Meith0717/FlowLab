// SoundEffectManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlowLab.Core.ContentHandling
{
    public sealed class SoundEffectManager
    {
        private static SoundEffectManager _instance;
        public static SoundEffectManager Instance { get { return _instance ??= new(); } }
        private readonly Dictionary<string, List<SoundEffectInstance>> SoundEffectInstances = new();
        private readonly int MaxSoundEffectInstances = new();
        public float OverallVolume;

        public SoundEffectManager(int maxSoundEffectInstances = 20) => MaxSoundEffectInstances = maxSoundEffectInstances;

        public void LoadBuildContent(ContentManager content, string iD, string path)
        {
            var soundEffect = content.Load<SoundEffect>(path);
            var sfxInstances = new List<SoundEffectInstance>();
            for (var i = 0; i < MaxSoundEffectInstances; i++) sfxInstances.Add(soundEffect.CreateInstance());
            SoundEffectInstances[iD] = sfxInstances;
        }

        private void LoadContent(string iD, string path)
        {
            using FileStream f = new(path, FileMode.Open);
            var soundEffect = SoundEffect.FromStream(f);
            var sfxInstances = new List<SoundEffectInstance>();
            for (var i = 0; i < MaxSoundEffectInstances; i++) sfxInstances.Add(soundEffect.CreateInstance());
            SoundEffectInstances[iD] = sfxInstances;
        }

        public void AddTextureContent(SoundEffect soundEffect, string iD)
        {
            var sfxInstances = new List<SoundEffectInstance>();
            for (var i = 0; i < MaxSoundEffectInstances; i++) sfxInstances.Add(soundEffect.CreateInstance());
            SoundEffectInstances[iD] = sfxInstances;
        }

        public void SetVolume(float master, float volume) => OverallVolume = MathHelper.Clamp(volume, 0f, 1f) * MathHelper.Clamp(master, 0f, 1f);

        public void PlaySound(string soundId, bool allowInterrupt = false, float volume = 1f, float pan = 0f)
        {
            if (!SoundEffectInstances.ContainsKey(soundId)) throw new Exception("SoundId not found!");

            var soundEffectInstances = SoundEffectInstances[soundId];

            var stopedSoundEffectInstances = soundEffectInstances.Where(s => s.State == SoundState.Stopped).ToList();

            SoundEffectInstance instance;
            switch (stopedSoundEffectInstances.Count)
            {
                case 0 when allowInterrupt:
                    instance = soundEffectInstances.First();
                    instance.Volume = OverallVolume * volume;
                    instance.Pan = MathHelper.Clamp(pan, -1f, 1f); ;
                    instance.Stop();
                    instance.Play();
                    break;
                case > 0:
                    instance = stopedSoundEffectInstances.First();
                    instance.Volume = OverallVolume * volume;
                    instance.Pan = MathHelper.Clamp(pan, -1f, 1f); ;
                    instance.Play();
                    break;
            }
        }

        private void IterateThroughInstances(Action<SoundEffectInstance> action)
        {
            foreach (var instances in SoundEffectInstances.Values)
            {
                foreach (var instance in instances) action.Invoke(instance);
            }
        }

        public void PauseAllSounds()
        {
            foreach (var instances in SoundEffectInstances.Values)
            {
                foreach (var instance in instances.Where((instance) => instance.State == SoundState.Playing))
                    instance.Pause();
            }
        }
        public void ResumeAllSounds()
        {
            foreach (var instances in SoundEffectInstances.Values)
            {
                foreach (var instance in instances.Where((instance) => instance.State == SoundState.Paused))
                    instance.Resume();
            }
        }
        public void StopAllSounds()
        {
            IterateThroughInstances((instance) =>
            {
                instance.IsLooped = false;
                instance.Stop();
            });
        }

        internal void ChangeOverallVolume(float master, float volume)
        {
            SetVolume(master, volume);
            IterateThroughInstances((instance) =>
            {
                instance.Volume = OverallVolume;
            });
        }
    }
}

