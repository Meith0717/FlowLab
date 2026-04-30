// FrameCounter.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace FlowLab.Engine.Debugging
{
    public class FrameCounter
    {
        private int mSamples;
        private float mSummedFps;
        private GameTime mGameTime;
        private readonly int mMaxCoolDown;
        private int mCoolDown;

        public long TotalFrames { get; private set; }
        public float TotalSeconds { get; private set; }
        public float AverageFramesPerSecond { get; private set; }
        public float MinFramesPerSecond { get; private set; } = float.MaxValue;
        public float MaxFramesPerSecond { get; private set; }
        public float CurrentFramesPerSecond { get; private set; }
        public float FrameDuration { get; private set; }

        public FrameCounter(int cooldown = 0) => mMaxCoolDown = cooldown;

        public void Update(GameTime gameTime)
        {
            mGameTime = gameTime;
            if (mCoolDown <= 0) mCoolDown = mMaxCoolDown;
            mCoolDown -= gameTime.ElapsedGameTime.Milliseconds;
        }

        public void UpdateFrameCounting()
        {
            TotalFrames++;
            if (mGameTime == null) { return; }
            if (mCoolDown > 0) { return; }

            mSamples++;
            float deltaTime = (float)(mGameTime.ElapsedGameTime.TotalMilliseconds / 1000);
            FrameDuration = mGameTime.ElapsedGameTime.Milliseconds;
            CurrentFramesPerSecond = 1.0f / deltaTime;
            mSummedFps += CurrentFramesPerSecond;
            MinFramesPerSecond = MathHelper.Min(CurrentFramesPerSecond, MinFramesPerSecond);
            MaxFramesPerSecond = MathHelper.Max(CurrentFramesPerSecond, MaxFramesPerSecond);
            AverageFramesPerSecond = mSummedFps / mSamples;
            TotalSeconds += (float)mGameTime.ElapsedGameTime.TotalSeconds;
        }
    }
}
