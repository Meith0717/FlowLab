// ParticleSystem.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Diagnostics;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.System;
using FlowLab.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Content;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Systems;
using MonoKit.Graphics.Camera;
using MonoKit.Input;

namespace FlowLab;

public class ParticleSystem : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly World _world;
    private readonly SpatialHashSystem _spatialHashSystem;
    private readonly Random _random = new Random();
    private Effect _particleShader;
    private VertexBuffer _quadBuffer;
    private IndexBuffer _quadIndexBuffer;
    private DynamicVertexBuffer _instanceBuffer;

    // Bounds of the particle cube
    private const float MinBound = 2f;
    private const float MaxBound = 48f;

    private const int MaxParticles = 1_000_000;

    public ParticleSystem(GraphicsDevice graphics)
    {
        _graphics = graphics;
        _world = new World();
        _world.Systems.Add(_spatialHashSystem = new SpatialHashSystem(2));
        _world.Systems.Add(new LifetimeSystem());
        _world.Systems.Add(new Movement3DSystem());
        _world.Systems.Add(new ParticleTransformSyncSystem());

        Vector3 position;
        for (var i = 0.5f; i < 50.5f; i++)
        for (var j = 0.5f; j < 50.5f; j++)
        {
            position = new Vector3(i, 0.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);

            position = new Vector3(i, 49.5f, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 50.5f; i++)
        for (var j = 0.5f; j < 50.5f; j++)
        {
            position = new Vector3(i, j, 0.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);

            position = new Vector3(i, j, 49.5f);
            ParticleFactory.CreateBoundaryParticle(_world, position);
        }

        for (var i = 0.5f; i < 50.5f; i++)
        for (var j = 0.5f; j < 50.5f; j++)
        {
            position = new Vector3(0.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);

            position = new Vector3(49.5f, i, j);
            ParticleFactory.CreateBoundaryParticle(_world, position);
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
            AddRandomBlueParticle();

        _world
            .GetQuery()
            .With<Velocity3D>()
            .ForEach(e =>
            {
                _world.TryGetComponent(e, out Velocity3D velocity);
                velocity.LinearVelocity += new Vector3(0, -0.01f, 0);
                _world.AddComponent(e, velocity);
            });

        _world.Update(gameTime);
    }

    public void Draw(Camera3D camera, BasicEffect effect)
    {
        var instanceData = _world.Components.GetAllComponentData<ParticleShaderData>().ToArray();
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

        _spatialHashSystem.Grid.DrawDebug(
            _graphics,
            camera.View,
            camera.Projection,
            effect,
            Color.Red
        );
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
            (float)_random.NextDouble() * (MaxBound - MinBound) + MinBound,
            (float)_random.NextDouble() * (MaxBound - MinBound) + MinBound,
            (float)_random.NextDouble() * (MaxBound - MinBound) + MinBound
        );
        ParticleFactory.CreateFluidParticle(_world, position);
        Debug.WriteLine(_world.ToString());
    }
}
