// ParticleSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Ecs;
using MonoKit.Graphics.Camera;

namespace FlowLab;

public class ParticleSystem : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly World _world;
    private Effect _particleShader;
    private VertexBuffer _quadBuffer;
    private IndexBuffer _quadIndexBuffer;
    private DynamicVertexBuffer _instanceBuffer;

    private const int MaxParticles = 1_000_000;

    public ParticleSystem(GraphicsDevice graphics)
    {
        _graphics = graphics;
        _world = new World();

        for (var i = 0; i < 50; i++)
        for (var j = 0; j < 50; j++)
        {
            _world.Components.AddComponent(
                _world.CreateEntity(),
                new ParticleComponent
                {
                    Color = Color.DimGray,
                    Position = new Vector3(i, 0, j),
                    Size = 1,
                }
            );
            _world.Components.AddComponent(
                _world.CreateEntity(),
                new ParticleComponent
                {
                    Color = Color.DimGray,
                    Position = new Vector3(i, 49, j),
                    Size = 1,
                }
            );
        }

        for (var i = 0; i < 50; i++)
        for (var j = 0; j < 50; j++)
        {
            _world.Components.AddComponent(
                _world.CreateEntity(),
                new ParticleComponent
                {
                    Color = Color.DimGray,
                    Position = new Vector3(i, j, 0),
                    Size = 1,
                }
            );
            _world.Components.AddComponent(
                _world.CreateEntity(),
                new ParticleComponent
                {
                    Color = Color.DimGray,
                    Position = new Vector3(i, j, 49),
                    Size = 1,
                }
            );
        }

        for (var i = 0; i < 50; i++)
        for (var j = 0; j < 50; j++)
        {
            _world.Components.AddComponent(
                _world.CreateEntity(),
                new ParticleComponent
                {
                    Color = Color.DimGray,
                    Position = new Vector3(0, i, j),
                    Size = 1,
                }
            );
            _world.Components.AddComponent(
                _world.CreateEntity(),
                new ParticleComponent
                {
                    Color = Color.DimGray,
                    Position = new Vector3(49, i, j),
                    Size = 1,
                }
            );
        }

        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        var quadVertices = new[]
        {
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(-1, -1)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, -1)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(-1, 1)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1)),
        };
        _quadBuffer = new VertexBuffer(
            _graphics,
            typeof(VertexPositionTexture),
            4,
            BufferUsage.WriteOnly
        );
        _quadBuffer.SetData(quadVertices);
        _quadIndexBuffer = new IndexBuffer(
            _graphics,
            IndexElementSize.SixteenBits,
            6,
            BufferUsage.WriteOnly
        );
        _quadIndexBuffer.SetData(new short[] { 0, 1, 2, 2, 1, 3 });
        _instanceBuffer = new DynamicVertexBuffer(
            _graphics,
            ParticleComponent.VertexDeclaration,
            MaxParticles,
            BufferUsage.WriteOnly
        );
    }

    public void LoadContent(ContentManager content)
    {
        ContentProvider.Container<Effect>().LoadContent(content, "Shaders");
        _particleShader = ContentProvider.Get<Effect>("ParticleShader");
    }

    public void Draw(Camera3D camera)
    {
        var instanceData = _world.Components.GetPool<ParticleComponent>().AsSpan().ToArray();
        var activeParticleCount = instanceData.Length;

        _instanceBuffer.SetData(instanceData, 0, activeParticleCount, SetDataOptions.Discard);

        _particleShader.Parameters["View"].SetValue(camera.View);
        _particleShader.Parameters["Projection"].SetValue(camera.Projection);

        _graphics.SetVertexBuffers(
            new VertexBufferBinding(_quadBuffer, 0, 0),
            new VertexBufferBinding(_instanceBuffer, 0, 1)
        );
        _graphics.Indices = _quadIndexBuffer;
        _graphics.BlendState = BlendState.AlphaBlend;
        _graphics.DepthStencilState = DepthStencilState.Default;

        foreach (var pass in _particleShader.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.DrawInstancedPrimitives(
                primitiveType: PrimitiveType.TriangleList,
                baseVertex: 0,
                startIndex: 0,
                primitiveCount: 2,
                instanceCount: activeParticleCount
            );
        }
    }

    public void Dispose()
    {
        _quadBuffer?.Dispose();
        _quadIndexBuffer?.Dispose();
        _instanceBuffer?.Dispose();
    }
}
