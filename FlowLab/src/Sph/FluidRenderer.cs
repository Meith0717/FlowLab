// FluidRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Ecs;
using MonoKit.Graphics.Camera;

namespace FlowLab.Sph;

public class FluidRenderer : IDisposable
{
    private static readonly Vector3 CrossSectionNormal = Vector3.UnitX;
    private const float CrossSectionDistance = -100;
    private readonly GraphicsDevice _graphics;
    private readonly VertexBuffer _quadBuffer;
    private readonly IndexBuffer _quadIndexBuffer;
    private readonly DynamicVertexBuffer _instanceBuffer;
    private readonly World _world;
    private readonly List<ParticleShaderData> _instanceData = [];
    private Effect _particleShader;

    public bool HideBoundary;

    public FluidRenderer(GraphicsDevice graphics, World world)
    {
        _graphics = graphics;
        _world = world;

        var quadVertices = new[]
        {
            new VertexPositionTexture(new Vector3(-1, -1, 0), new Vector2(-1, -1)),
            new VertexPositionTexture(new Vector3(1, -1, 0), new Vector2(1, -1)),
            new VertexPositionTexture(new Vector3(-1, 1, 0), new Vector2(-1, 1)),
            new VertexPositionTexture(new Vector3(1, 1, 0), new Vector2(1, 1)),
        };
        _quadBuffer = new VertexBuffer(
            graphics,
            typeof(VertexPositionTexture),
            4,
            BufferUsage.WriteOnly
        );
        _quadBuffer.SetData(quadVertices);
        _quadIndexBuffer = new IndexBuffer(
            graphics,
            IndexElementSize.SixteenBits,
            6,
            BufferUsage.WriteOnly
        );
        _quadIndexBuffer.SetData(new short[] { 0, 1, 2, 2, 1, 3 });
        _instanceBuffer = new DynamicVertexBuffer(
            graphics,
            ParticleShaderData.VertexDeclaration,
            Config.SimConfig.MaxParticles,
            BufferUsage.WriteOnly
        );
    }

    public void Initialize()
    {
        _particleShader = ContentProvider.Get<Effect>("ParticleShader");
    }

    public void Update()
    {
        var entities = HideBoundary
            ? _world.TypeTracker.GetEntitiesWith<FluidTag>()
            : _world.TypeTracker.GetEntitiesWith<ParticleTag>();

        var shaderDataPool = _world.Components.GetOrCreatePool<ParticleShaderData>();
        _instanceData.Clear();
        foreach (var entity in entities)
        {
            ref var shaderData = ref shaderDataPool.Get(entity.Id);
            _instanceData.Add(shaderData);
        }
    }

    public void Draw(Camera3D camera)
    {
        var activeParticleCount = _instanceData.Count;
        if (activeParticleCount == 0)
            return;

        _instanceBuffer.SetData(
            _instanceData.ToArray(),
            0,
            activeParticleCount,
            SetDataOptions.Discard
        );

        _particleShader.Parameters["View"].SetValue(camera.View);
        _particleShader.Parameters["Projection"].SetValue(camera.Projection);
        _particleShader.Parameters["CrossSectionNormal"].SetValue(CrossSectionNormal);
        _particleShader.Parameters["CrossSectionDistance"].SetValue(CrossSectionDistance);

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
