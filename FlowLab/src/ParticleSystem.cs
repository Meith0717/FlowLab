// ParticleSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Ecs;
using MonoKit.Ecs.Systems;
using MonoKit.Graphics.Camera;
using MonoKit.Input;
using SpatialHashSystem = FlowLab.Ecs.System.SpatialHashSystem;

namespace FlowLab;

public class ParticleSystem : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly World _world;
    private readonly SpatialHashSystem _spatialHashSystem;
    private Effect _particleShader;
    private VertexBuffer _quadBuffer;
    private IndexBuffer _quadIndexBuffer;
    private DynamicVertexBuffer _instanceBuffer;

    // Bounds of the particle cube
    private const float MinBound = 2f;
    private const float MaxBound = 23f;

    private const int MaxParticles = 1_000_000;

    public ParticleSystem(GraphicsDevice graphics)
    {
        _graphics = graphics;
        _world = new World();
        _world.Systems.Add(_spatialHashSystem = new SpatialHashSystem(2));
        _world.Systems.Add(new LifetimeSystem());
        _world.Systems.Add(new ParticleTransformSyncSystem());
        _world.Systems.Add(new SimulationSystem(_spatialHashSystem.Grid, 1, 1, 100, 2, .05f));

        Vector3 position;
        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(i, 0.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, -0.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(i, j, 0.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, j, -0.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);

            position = new Vector3(i, j, 24.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(i, j, 25.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 25.5f; i++)
        for (var j = 0.5f; j < 25.5f; j++)
        {
            position = new Vector3(0.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(-0.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);

            position = new Vector3(24.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
            position = new Vector3(25.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        InitializeBuffers();
        AddRandomBlueParticle();
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
            ParticleShaderData.VertexDeclaration,
            MaxParticles,
            BufferUsage.WriteOnly
        );
    }

    public void LoadContent(ContentManager content)
    {
        ContentProvider.Container<Effect>().LoadContent(content, "Shaders");
        _particleShader = ContentProvider.Get<Effect>("ParticleShader");
    }

    public void Update(double gameTime, InputHandler inputHandler)
    {
        if (inputHandler.HasAction((byte)ActionType.Test))
        {
            AddRandomBlueParticle();
        }

        _world.Update(gameTime);
    }

    public void Draw(Camera3D camera, BasicEffect effect)
    {
        var instanceData = _world
            .Components.GetOrCreatePool<ParticleShaderData>()
            .AsSpan()
            .ToArray();
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

        // _spatialHashSystem.Grid.DrawDebug(
        //     _graphics,
        //     camera.View,
        //     camera.Projection,
        //     effect,
        //     Color.Red
        // );
    }

    public void Dispose()
    {
        _quadBuffer?.Dispose();
        _quadIndexBuffer?.Dispose();
        _instanceBuffer?.Dispose();
    }

    private void AddRandomBlueParticle()
    {
        var position = new Vector3(
            MinBound + MaxBound / 2,
            MinBound + MaxBound / 2,
            MinBound + MaxBound / 2
        );

        for (var x = -10; x <= 10; x++)
        for (var y = -10; y <= 10; y++)
        for (var z = -10; z <= 10; z++)
            ParticleFactory.CreateFluidParticle(_world, position + new Vector3(x, y, z));
    }
}
