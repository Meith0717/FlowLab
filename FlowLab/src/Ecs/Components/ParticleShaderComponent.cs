// ParticleComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Ecs.Components;

[StructLayout(LayoutKind.Sequential)]
public struct ParticleShaderComponent
{
    public Vector3 Position; // 12 bytes
    public Color Color; // 4 bytes
    public float Size; // 4 bytes

    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 1),
        new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1) // UsageIndex 1 indicates instance data
    );
}
