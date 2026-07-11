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
using MonoKit.Spatial;

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
    private readonly EcsSpatialHash3D _spatialHash;
    private readonly float _cellSize;
    private DynamicVertexBuffer _currentWriteBuffer;
    private DynamicVertexBuffer _currentReadBuffer;
    private Effect _particleShader;
    private BasicEffect _gridEffect;
    private int _particleCount;

    public bool ShowSpatialGrids { get; set; }

    public FluidRenderer(
        GraphicsDevice graphics,
        World world,
        EcsSpatialHash3D spatialHash,
        float cellSize
    )
    {
        _graphics = graphics;
        _world = world;
        _spatialHash = spatialHash;
        _cellSize = cellSize;
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

        // Initialize grid rendering
        _gridEffect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
        };
    }

    public void LoadContent()
    {
        _particleShader = ContentProvider.Get<Effect>("ParticleShader");
    }

    public void Update(bool hideBoundary)
    {
        var entities = hideBoundary
            ? _world.TypeTracker.GetEntitiesWith<FluidTag>()
            : _world.TypeTracker.GetEntitiesWith<ParticleProperties>();

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

    private void DrawSpatialGrid(Camera3D camera)
    {
        var view = camera.View;
        var projection = camera.Projection;

        _gridEffect.View = view;
        _gridEffect.Projection = projection;
        _gridEffect.World = Matrix.Identity;

        _graphics.BlendState = BlendState.Opaque;
        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.RasterizerState = RasterizerState.CullNone;

        // Get active grid positions from spatial hash
        var activePositions = _spatialHash.ActiveGridPositions;

        if (activePositions.Count == 0)
            return;

        // Calculate camera frustum bounds for culling
        var cameraPosition = camera.Position;
        var range = 100; // cells in each direction from camera
        var cellMinX = (int)Math.Floor(cameraPosition.X / _cellSize) - range;
        var cellMaxX = (int)Math.Floor(cameraPosition.X / _cellSize) + range;
        var cellMinY = (int)Math.Floor(cameraPosition.Y / _cellSize) - range;
        var cellMaxY = (int)Math.Floor(cameraPosition.Y / _cellSize) + range;
        var cellMinZ = (int)Math.Floor(cameraPosition.Z / _cellSize) - range;
        var cellMaxZ = (int)Math.Floor(cameraPosition.Z / _cellSize) + range;

        var halfSize = _cellSize / 2f;
        var activeColor = Color.Red;

        // Draw each active cell as a cube/wireframe
        foreach (var gridPos in activePositions)
        {
            var x = (int)gridPos.X;
            var y = (int)gridPos.Y;
            var z = (int)gridPos.Z;

            // Cull cells outside view range
            if (
                x < cellMinX
                || x > cellMaxX
                || y < cellMinY
                || y > cellMaxY
                || z < cellMinZ
                || z > cellMaxZ
            )
                continue;

            var xPos = x * _cellSize;
            var yPos = y * _cellSize;
            var zPos = z * _cellSize;

            // Draw cell as a wireframe cube
            var minCorner = new Vector3(xPos, yPos, zPos);
            var maxCorner = new Vector3(xPos + _cellSize, yPos + _cellSize, zPos + _cellSize);

            // Bottom face
            DrawGridLine(
                minCorner,
                new Vector3(xPos + _cellSize, yPos, zPos),
                activeColor,
                activeColor
            );
            DrawGridLine(
                minCorner,
                new Vector3(xPos, yPos, zPos + _cellSize),
                activeColor,
                activeColor
            );
            DrawGridLine(
                new Vector3(xPos + _cellSize, yPos, zPos),
                maxCorner,
                activeColor,
                activeColor
            );
            DrawGridLine(
                new Vector3(xPos, yPos, zPos + _cellSize),
                maxCorner,
                activeColor,
                activeColor
            );

            // Top face
            var topMin = new Vector3(xPos, yPos + _cellSize, zPos);
            var topMax = maxCorner;
            DrawGridLine(
                topMin,
                new Vector3(xPos + _cellSize, yPos + _cellSize, zPos),
                activeColor,
                activeColor
            );
            DrawGridLine(
                topMin,
                new Vector3(xPos, yPos + _cellSize, zPos + _cellSize),
                activeColor,
                activeColor
            );
            DrawGridLine(
                new Vector3(xPos + _cellSize, yPos + _cellSize, zPos),
                topMax,
                activeColor,
                activeColor
            );
            DrawGridLine(
                new Vector3(xPos, yPos + _cellSize, zPos + _cellSize),
                topMax,
                activeColor,
                activeColor
            );

            // Vertical edges
            DrawGridLine(minCorner, topMin, activeColor, activeColor);
            DrawGridLine(
                new Vector3(xPos + _cellSize, yPos, zPos),
                new Vector3(xPos + _cellSize, yPos + _cellSize, zPos),
                activeColor,
                activeColor
            );
            DrawGridLine(
                new Vector3(xPos, yPos, zPos + _cellSize),
                new Vector3(xPos, yPos + _cellSize, zPos + _cellSize),
                activeColor,
                activeColor
            );
            DrawGridLine(maxCorner, topMax, activeColor, activeColor);
        }
    }

    private void DrawGridLine(Vector3 start, Vector3 end, Color startColor, Color endColor)
    {
        var vertices = new[]
        {
            new VertexPositionColor(start, startColor),
            new VertexPositionColor(end, endColor),
        };

        var vb = new VertexBuffer(_graphics, typeof(VertexPositionColor), 2, BufferUsage.WriteOnly);
        vb.SetData(vertices);

        _graphics.SetVertexBuffer(vb);
        _graphics.Indices = null;

        foreach (var pass in _gridEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.DrawPrimitives(PrimitiveType.LineList, 0, 1);
        }

        vb.Dispose();
    }

    private Color GetCellColor(int x, int y, int z, HashSet<long> activeHashes)
    {
        unchecked
        {
            var hash = ((long)x * 73856093L) ^ ((long)y * 19349663L) ^ ((long)z * 83492791L);
            return activeHashes.Contains(hash)
                ? new Color(100, 150, 255, 120)
                : new Color(50, 75, 100, 60);
        }
    }

    public void Draw(Camera3D camera)
    {
        if (_particleCount == 0 && !ShowSpatialGrids)
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

        // Draw spatial grid if enabled
        if (ShowSpatialGrids)
        {
            DrawSpatialGrid(camera);
        }
    }

    public void Dispose()
    {
        _quadBuffer?.Dispose();
        _quadIndexBuffer?.Dispose();
        _instanceBufferA?.Dispose();
        _instanceBufferB?.Dispose();
        _gridEffect?.Dispose();
    }
}
