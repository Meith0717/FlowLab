// FluidRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using System.Reflection;
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
    private readonly ISpatialGrid3D _spatialHash;
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
        ISpatialGrid3D spatialHash,
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

    public void Initialize()
    {
        _particleShader = ContentProvider.Get<Effect>("ParticleShader");
    }

    public void Update(bool hideBoundary)
    {
        var entities = hideBoundary
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

        var activeColor = new Color(100, 150, 255, 80);
        var inactiveColor = new Color(50, 75, 100, 40);

        // Get active cell hashes via reflection
        var activeCellHashes = new HashSet<long>();
        var activeCellsField = _spatialHash
            .GetType()
            .GetField("_activeCells", BindingFlags.NonPublic | BindingFlags.Instance);
        var gridsField = _spatialHash
            .GetType()
            .GetField("_grids", BindingFlags.NonPublic | BindingFlags.Instance);

        var activeCells = activeCellsField?.GetValue(_spatialHash) as System.Collections.IList;
        var grids = gridsField?.GetValue(_spatialHash) as System.Collections.IDictionary;

        if (activeCells != null && grids != null)
        {
            foreach (var kvp in (System.Collections.IEnumerable)grids)
            {
                var entry = (System.Collections.DictionaryEntry)kvp;
                if (activeCells.Contains(entry.Value))
                {
                    activeCellHashes.Add((long)entry.Key);
                }
            }
        }

        // Calculate visible cell range based on camera position
        var cameraPosition = camera.Position;

        // Calculate a reasonable range around the camera
        var range = 50; // cells in each direction
        var cellMinX = (int)Math.Floor(cameraPosition.X / _cellSize) - range;
        var cellMaxX = (int)Math.Floor(cameraPosition.X / _cellSize) + range;
        var cellMinY = (int)Math.Floor(cameraPosition.Y / _cellSize) - range;
        var cellMaxY = (int)Math.Floor(cameraPosition.Y / _cellSize) + range;
        var cellMinZ = (int)Math.Floor(cameraPosition.Z / _cellSize) - range;
        var cellMaxZ = (int)Math.Floor(cameraPosition.Z / _cellSize) + range;

        // Clamp to prevent excessive iteration
        cellMinX = Math.Max(cellMinX, -200);
        cellMaxX = Math.Min(cellMaxX, 200);
        cellMinY = Math.Max(cellMinY, -200);
        cellMaxY = Math.Min(cellMaxY, 200);
        cellMinZ = Math.Max(cellMinZ, -200);
        cellMaxZ = Math.Min(cellMaxZ, 200);

        var halfSize = _cellSize / 2f;

        // Draw grid lines (more efficient than individual cells)
        // Draw Y-aligned lines (vertical)
        for (var x = cellMinX; x <= cellMaxX; x++)
        for (var z = cellMinZ; z <= cellMaxZ; z++)
        {
            var xPos = x * _cellSize;
            var zPos = z * _cellSize;
            DrawGridLine(
                new Vector3(xPos, cellMinY * _cellSize, zPos),
                new Vector3(xPos, cellMaxY * _cellSize, zPos),
                GetCellColor(x, cellMinY, z, activeCellHashes),
                GetCellColor(x, cellMaxY, z, activeCellHashes)
            );
        }

        // Draw Z-aligned lines (depth)
        for (var x = cellMinX; x <= cellMaxX; x++)
        for (var y = cellMinY; y <= cellMaxY; y++)
        {
            var xPos = x * _cellSize;
            var yPos = y * _cellSize;
            DrawGridLine(
                new Vector3(xPos, yPos, cellMinZ * _cellSize),
                new Vector3(xPos, yPos, cellMaxZ * _cellSize),
                GetCellColor(x, y, cellMinZ, activeCellHashes),
                GetCellColor(x, y, cellMaxZ, activeCellHashes)
            );
        }

        // Draw X-aligned lines (horizontal)
        for (var y = cellMinY; y <= cellMaxY; y++)
        for (var z = cellMinZ; z <= cellMaxZ; z++)
        {
            var yPos = y * _cellSize;
            var zPos = z * _cellSize;
            DrawGridLine(
                new Vector3(cellMinX * _cellSize, yPos, zPos),
                new Vector3(cellMaxX * _cellSize, yPos, zPos),
                GetCellColor(cellMinX, y, z, activeCellHashes),
                GetCellColor(cellMaxX, y, z, activeCellHashes)
            );
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
