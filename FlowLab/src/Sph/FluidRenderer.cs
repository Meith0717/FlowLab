// FluidRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
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
    private readonly DynamicVertexBuffer _instanceBufferA;
    private readonly DynamicVertexBuffer _instanceBufferB;
    private readonly World _world;
    private readonly ParticleShaderData[] _instanceData;
    private DynamicVertexBuffer _currentWriteBuffer;
    private DynamicVertexBuffer _currentReadBuffer;
    private Effect _particleShader;
    private int _particleCount;

    public bool HideBoundary;

    public FluidRenderer(GraphicsDevice graphics, World world)
    {
        _graphics = graphics;
        _world = world;
        _instanceData = new ParticleShaderData[Config.SimConfig.MaxParticles];

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

        // Double-buffered instance buffers
        _instanceBufferA = new DynamicVertexBuffer(
            graphics,
            ParticleShaderData.VertexDeclaration,
            Config.SimConfig.MaxParticles,
            BufferUsage.WriteOnly
        );
        _instanceBufferB = new DynamicVertexBuffer(
            graphics,
            ParticleShaderData.VertexDeclaration,
            Config.SimConfig.MaxParticles,
            BufferUsage.WriteOnly
        );
        _currentWriteBuffer = _instanceBufferA;
        _currentReadBuffer = _instanceBufferB;
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
        _particleCount = 0;
        foreach (var entity in entities)
        {
            ref var shaderData = ref shaderDataPool.Get(entity.Id);
            _instanceData[_particleCount++] = shaderData;
        }

        if (_particleCount > 0)
        {
            _currentWriteBuffer.SetData(
                _instanceData,
                0,
                _particleCount,
                SetDataOptions.NoOverwrite
            );
        }
        SwapBuffers();
    }

    private void SwapBuffers()
    {
        (_currentWriteBuffer, _currentReadBuffer) = (_currentReadBuffer, _currentWriteBuffer);
    }

    public void Draw(Camera3D camera)
    {
        if (_particleCount == 0)
            return;

        _particleShader.Parameters["View"].SetValue(camera.View);
        _particleShader.Parameters["Projection"].SetValue(camera.Projection);
        _particleShader.Parameters["CrossSectionNormal"].SetValue(CrossSectionNormal);
        _particleShader.Parameters["CrossSectionDistance"].SetValue(CrossSectionDistance);

        _graphics.SetVertexBuffers(
            new VertexBufferBinding(_quadBuffer, 0, 0),
            new VertexBufferBinding(_currentReadBuffer, 0, 1)
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
                instanceCount: _particleCount
            );
        }
    }

    public void Dispose()
    {
        _quadBuffer?.Dispose();
        _quadIndexBuffer?.Dispose();
        _instanceBufferA?.Dispose();
        _instanceBufferB?.Dispose();
    }
}
