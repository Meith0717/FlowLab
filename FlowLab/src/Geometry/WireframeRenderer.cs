// WireframeRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Graphics.Camera;

namespace FlowLab.Geometry;

public class WireframeRenderer : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly ObjModel _model;
    private VertexBuffer _vertexBuffer;
    private BasicEffect _effect;
    private Color _color = Color.White;
    private float _lineWidth = 1f;
    private Matrix _world = Matrix.Identity;

    public Color Color
    {
        get => _color;
        set => _color = value;
    }

    public float LineWidth
    {
        get => _lineWidth;
        set => _lineWidth = value;
    }

    public Matrix World
    {
        get => _world;
        set => _world = value;
    }

    public WireframeRenderer(GraphicsDevice graphics, ObjModel model)
    {
        _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        _model = model ?? throw new ArgumentNullException(nameof(model));

        _effect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            FogEnabled = false,
        };

        BuildEdgeBuffers();
    }

    private void BuildEdgeBuffers()
    {
        // Extract unique edges from triangles
        // Each triangle has 3 edges: (0,1), (1,2), (2,0)
        // Use a hash set to avoid duplicates (shared edges)
        var edgeSet = new HashSet<Edge>();
        
        foreach (var tri in _model.Triangles)
        {
            // Create edges sorted so (a,b) and (b,a) are treated as same
            edgeSet.Add(new Edge(tri.V0, tri.V1));
            edgeSet.Add(new Edge(tri.V1, tri.V2));
            edgeSet.Add(new Edge(tri.V2, tri.V0));
        }

        // Convert to vertex list for LineList
        var vertices = new VertexPositionColor[edgeSet.Count * 2];
        var i = 0;
        foreach (var edge in edgeSet)
        {
            vertices[i++] = new VertexPositionColor(edge.A, _color);
            vertices[i++] = new VertexPositionColor(edge.B, _color);
        }

        _vertexBuffer = new VertexBuffer(
            _graphics,
            typeof(VertexPositionColor),
            vertices.Length,
            BufferUsage.WriteOnly
        );
        _vertexBuffer.SetData(vertices);
    }

    public void Draw(Camera3D camera)
    {
        if (_graphics == null || _vertexBuffer == null)
            return;

        _effect.World = _world;
        _effect.View = camera.View;
        _effect.Projection = camera.Projection;

        _graphics.SetVertexBuffer(_vertexBuffer);
        _graphics.Indices = null;
        _graphics.RasterizerState = RasterizerState.CullNone;
        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.BlendState = BlendState.Opaque;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.DrawPrimitives(
                PrimitiveType.LineList,
                0,
                _vertexBuffer.VertexCount / 2
            );
        }
    }

    public void Dispose()
    {
        _vertexBuffer?.Dispose();
        _effect?.Dispose();
        GC.SuppressFinalize(this);
    }

    // Helper struct for edge deduplication
    private struct Edge : IEquatable<Edge>
    {
        public Vector3 A { get; }
        public Vector3 B { get; }

        public Edge(Vector3 a, Vector3 b)
        {
            // Store in consistent order so edge (a,b) equals edge (b,a)
            // Compare X first, then Y, then Z
            var cmp = a.X.CompareTo(b.X);
            if (cmp < 0 || (cmp == 0 && (a.Y.CompareTo(b.Y) < 0 || (a.Y == b.Y && a.Z <= b.Z))))
            {
                A = a;
                B = b;
            }
            else
            {
                A = b;
                B = a;
            }
        }

        public bool Equals(Edge other)
        {
            return A == other.A && B == other.B;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (A.GetHashCode() * 397) ^ B.GetHashCode();
            }
        }
    }
}
