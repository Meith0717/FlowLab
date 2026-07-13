// InstabilityRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System;
using FlowLab.Ecs.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Graphics.Camera;

namespace FlowLab.Sph;

/// <summary>
/// Renders velocity vectors for unstable particles to visualize instability sources.
/// Only draws vectors where DiagnosticComponent.IsStable == false
/// </summary>
public class InstabilityRenderer : IDisposable
{
    private readonly GraphicsDevice _graphics;
    private readonly World _world;
    private readonly BasicEffect _lineEffect;
    private readonly VertexPositionColor[] _lineVertices;

    private const int MaxUnstableParticles = 10000;
    private const float VectorScale = 0.5f; // Scale factor for velocity vectors
    private const float VectorLengthCap = 5f; // Maximum vector length to draw

    /// <summary>
    /// Whether to show only negative velocity components (downward/leftward motion)
    /// </summary>
    public bool ShowOnlyNegativeVelocity { get; set; } = true;

    public bool Enabled { get; set; } = true;
    public Color VectorColor { get; set; } = Color.Red;
    public float LineWidth { get; set; } = 10f;

    private ComponentPool<Transform3D> _transformPool;
    private ComponentPool<KinematicState> _kinematicPool;
    private ComponentPool<DiagnosticComponent> _diagnosticPool;
    private ComponentPool<ParticleProperties> _particlePool;

    public InstabilityRenderer(GraphicsDevice graphics, World world)
    {
        _graphics = graphics;
        _world = world;

        _lineEffect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            FogEnabled = false,
        };

        _lineVertices = new VertexPositionColor[MaxUnstableParticles * 2];

        // Get component pools
        var components = world.Components;
        _transformPool = components.GetOrCreatePool<Transform3D>();
        _kinematicPool = components.GetOrCreatePool<KinematicState>();
        _diagnosticPool = components.GetOrCreatePool<DiagnosticComponent>();
        _particlePool = components.GetOrCreatePool<ParticleProperties>();
    }

    public void Draw(Camera3D camera)
    {
        if (!Enabled)
            return;

        _lineEffect.View = camera.View;
        _lineEffect.Projection = camera.Projection;
        _lineEffect.World = Matrix.Identity;

        // Collect unstable particles
        var vertexCount = 0;
        var entities = _world.TypeTracker.GetEntitiesWith<DiagnosticComponent>();

        foreach (var entity in entities)
        {
            ref var diagnostic = ref _diagnosticPool.Get(entity.Id);
            if (diagnostic.IsStable)
                continue;

            ref var transform = ref _transformPool.Get(entity.Id);
            ref var kinematic = ref _kinematicPool.Get(entity.Id);

            // Check if we should show only negative velocity
            if (ShowOnlyNegativeVelocity)
            {
                // Only draw if velocity has significant negative components
                if (
                    kinematic.Velocity.Y >= 0
                    && kinematic.Velocity.X >= 0
                    && kinematic.Velocity.Z >= 0
                )
                    continue;
            }

            // Calculate scaled velocity vector
            var velocity = kinematic.Velocity;
            var length = velocity.Length();

            if (length < 0.01f) // Skip very small velocities
                continue;

            // Cap the length for visualization
            var displayLength = Math.Min(length * VectorScale, VectorLengthCap);

            // Normalize and scale
            var direction =
                velocity.Length() > 0 ? velocity / velocity.Length() * displayLength : Vector3.Zero;

            var start = transform.Position;
            var end = start + direction;

            // Add line vertices (2 vertices per line)
            if (vertexCount + 1 < _lineVertices.Length)
            {
                _lineVertices[vertexCount++] = new VertexPositionColor(start, VectorColor);
                _lineVertices[vertexCount++] = new VertexPositionColor(end, VectorColor);
            }
        }

        if (vertexCount < 2)
            return;

        // Draw all lines
        _graphics.BlendState = BlendState.Opaque;
        _graphics.DepthStencilState = DepthStencilState.Default;
        _graphics.RasterizerState = RasterizerState.CullNone;

        // Set line width if supported
        // Note: In XNA/MonoGame, line width is fixed unless using custom effect
        // We use color intensity to simulate thickness

        foreach (var pass in _lineEffect.CurrentTechnique.Passes)
        {
            pass.Apply();

            // Create vertex buffer for this frame
            var vertexBuffer = new VertexBuffer(
                _graphics,
                typeof(VertexPositionColor),
                vertexCount,
                BufferUsage.WriteOnly
            );
            vertexBuffer.SetData(_lineVertices, 0, vertexCount);

            _graphics.SetVertexBuffer(vertexBuffer);
            _graphics.Indices = null;

            _graphics.DrawPrimitives(PrimitiveType.LineList, 0, vertexCount / 2);

            vertexBuffer.Dispose();
        }
    }

    public void Dispose()
    {
        _lineEffect?.Dispose();
    }
}
