// ParticleShaderComponent.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FlowLab.Ecs.Components;

[StructLayout(LayoutKind.Sequential)]
public struct ParticleShaderData
{
    public Vector3 Position; // 12 bytes
    public Color Color; // 4 bytes
    public float Size; // 4 bytes
    public float IgnorePlane; // 4 bytes (0 = clip normally, 1 = ignore cross-section plane)

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 1),
        new VertexElement(16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(20, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2)
    );
}
