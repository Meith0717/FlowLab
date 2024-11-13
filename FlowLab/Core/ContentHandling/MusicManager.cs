// MusicManager.cs 
// Copyright (c) 2023-2024 Thierry Meiers 
// All rights reserved.

using FlowLab.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using System.IO;

namespace FlowLab.Core.ContentHandling
{

    public class MusicManager
    {
        private static MusicManager _instance;
        public static MusicManager Instance { get { return _instance ??= new(); } }

        private readonly List<SoundEffect> _musics = new();
        private SoundEffectInstance _musicInstance;
        private bool _loadet;
        public float OverallVolume;

        public void LoadBuildContent(ContentManager content, string iD, string path)
        {
            var soundEffect = content.Load<SoundEffect>(path);
            soundEffect.Name = iD;
            _musics.Add(soundEffect);
            _loadet = true;
        }

        private void LoadContent(string iD, string path)
        {
            using FileStream f = new(path, FileMode.Open);
            var soundEffect = SoundEffect.FromStream(f);
            soundEffect.Name = iD;
            _musics.Add(soundEffect);
            _loadet = true;
        }

        public void AddContent(SoundEffect soundEffect) => _musics.Add(soundEffect);


        public void InitializeVolume(float master, float volume) => OverallVolume = MathHelper.Clamp(volume, 0f, 1f) * MathHelper.Clamp(master, 0f, 1f);

        public void Pause() => _musicInstance.Pause();
        public void Resume() => _musicInstance.Resume();

        public void Update()
        {
            if (!_loadet) return;
            if (_musicInstance is null)
            {
                var music = ExtendedRandom.GetRandomElement(_musics);
                _musicInstance = music.CreateInstance();
                _musicInstance.Volume = OverallVolume;
                _musicInstance.Play();
                return;
            }

            if (_musicInstance.State == SoundState.Playing) return;
            if (_musicInstance.State == SoundState.Paused) return;
            _musicInstance = null;
        }

        public void ChangeOverallVolume(float master, float volume)
        {
            InitializeVolume(master, volume);
            if (_musicInstance is null) return;
            _musicInstance.Volume = OverallVolume;
        }
    }
}

