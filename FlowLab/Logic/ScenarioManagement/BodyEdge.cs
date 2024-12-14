// BodyEdge.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using Microsoft.Xna.Framework;

namespace FlowLab.Logic.ScenarioManagement
{
    internal class BodyEdge(Vector2 star, Vector2 end)
    {
        public readonly Vector2 Start = star;
        public readonly Vector2 End  = end;

        public float Length => Vector2.Distance(Start, End);

        public Vector2 Direction => Vector2.Normalize(End - Start);

        /// <summary>
        /// Returns true if the edge has the length of n times particle size
        /// </summary>
        public bool IsValid(float particleSize) => Length % particleSize == 0;

        /// <summary>
        /// Returns the position of the i'th particle on the edge
        /// </summary>
        public Vector2 GetParticlePosition(int i,  float particleSize) => Start + (Direction * (i * particleSize));

        /// <summary>
        /// Gets the amount of spaces on the edge if it is valid
        /// </summary>
        public bool TryGetParticleSpaceCount(float particleSize, out int count) 
        {
            count = 0;
            if (!IsValid(particleSize)) return false;
            count = (int)(Length / particleSize);  
            return true;
        }
    }
}
