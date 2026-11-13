// FlowLab - ParticleRenderingSystem.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using FlowLab.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Game.ArchetypeEcs;
using MonoKit.Game.ArchetypeEcs.Entities;
using MonoKit.Game.ArchetypeEcs.Systems;
using MonoKit.Game.Camera;

namespace FlowLab.Systems;

public class ParticleRenderingSystem : IDrawSystem, IUpdateSystem
{
    private const int MaxParticles = 1_000_000;
    private readonly GraphicsDevice _graphics;
    private readonly VertexBuffer _quadBuffer;
    private readonly IndexBuffer _quadIndexBuffer;
    private readonly ParticleShaderData[] _instanceData;
    private DynamicVertexBuffer _currentWriteBuffer;
    private DynamicVertexBuffer _currentReadBuffer;
    private readonly Effect _particleShader;
    private readonly BasicEffect _gridEffect;
    private int _particleCount;
    private readonly Camera3D _camera3D;

    public ParticleRenderingSystem(
        Camera3D camera3D,
        GraphicsDevice graphics,
        Effect particleShader
    )
    {
        _camera3D = camera3D;
        _graphics = graphics;
        _particleShader = particleShader;
        _instanceData = new ParticleShaderData[MaxParticles]; // TODO

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

        var instanceBufferA = new DynamicVertexBuffer(
            graphics,
            ParticleShaderData.VertexDeclaration,
            MaxParticles,
            BufferUsage.WriteOnly
        );
        var instanceBufferB = new DynamicVertexBuffer(
            graphics,
            ParticleShaderData.VertexDeclaration,
            MaxParticles,
            BufferUsage.WriteOnly
        );
        _currentWriteBuffer = instanceBufferA;
        _currentReadBuffer = instanceBufferB;

        _gridEffect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
        };
    }

    public void Dispose()
    {
        _quadBuffer?.Dispose();
        _quadIndexBuffer?.Dispose();
        _gridEffect?.Dispose();
    }

    public void Initialize(World world) { }

    public void Update(double elapsedMs, World world)
    {
        _particleCount = 0;

        var boundaryColumn = world.GetComponentColumn<Core.Tags.BoundaryTag>();

        world.ForEach(
            (
                Entity e,
                ref Core.Components.Kinematics eKinematics,
                ref ParticleShaderData eShaderData
            ) =>
            {
                eShaderData.Position = eKinematics.Position;
                eShaderData.Color = boundaryColumn.Has(e) ? Color.Gray : Color.DodgerBlue;
                _instanceData[_particleCount] = eShaderData;
                _particleCount++;
            }
        );

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

    public void Draw(
        double elapsedMs,
        World world,
        SpriteBatch spriteBatch,
        GraphicsDevice graphicsDevice
    )
    {
        if (_particleCount == 0)
            return;

        var normalizedNormal = Vector3.Normalize(Vector3.UnitX);
        var planeDistance = -Vector3.Dot(normalizedNormal, Vector3.UnitX * 100);
        _particleShader.Parameters["CrossSectionNormal"].SetValue(normalizedNormal);
        _particleShader.Parameters["CrossSectionDistance"].SetValue(planeDistance);
        _particleShader.Parameters["View"].SetValue(_camera3D.View);
        _particleShader.Parameters["Projection"].SetValue(_camera3D.Projection);

        _graphics.SetVertexBuffers(
            new VertexBufferBinding(_quadBuffer, 0, 0),
            new VertexBufferBinding(_currentReadBuffer, 0, 1)
        );
        _graphics.Indices = _quadIndexBuffer;
        _graphics.BlendState = BlendState.AlphaBlend;
        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.RasterizerState = RasterizerState.CullNone;

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

    private void SwapBuffers()
    {
        (_currentWriteBuffer, _currentReadBuffer) = (_currentReadBuffer, _currentWriteBuffer);
    }
}
