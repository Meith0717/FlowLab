using System;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Graphics.Camera;

namespace FlowLab.Sph;

public class InstabilityRenderer : IDisposable
{
    private const int MaxUnstableParticles = 10000;

    private readonly GraphicsDevice _graphics;
    private readonly World _world;
    private readonly SimConfig _config;
    private readonly BasicEffect _lineEffect;
    private readonly VertexPositionColor[] _lineVertices;
    private readonly VertexBuffer _vertexBuffer;
    private readonly ComponentPool<Transform3D> _transformPool;
    private readonly ComponentPool<KinematicState> _kinematicPool;
    private readonly ComponentPool<DiagnosticComponent> _diagnosticPool;

    private Color VectorColor { get; } = Color.Red;

    public InstabilityRenderer(GraphicsDevice graphics, World world, SimConfig config)
    {
        _graphics = graphics;
        _world = world;
        _config = config;

        _lineEffect = new BasicEffect(graphics)
        {
            VertexColorEnabled = true,
            LightingEnabled = false,
            FogEnabled = false,
        };

        _lineVertices = new VertexPositionColor[MaxUnstableParticles * 2];
        _vertexBuffer = new VertexBuffer(
            _graphics,
            typeof(VertexPositionColor),
            MaxUnstableParticles * 2,
            BufferUsage.WriteOnly
        );

        var components = world.Components;
        _transformPool = components.GetOrCreatePool<Transform3D>();
        _kinematicPool = components.GetOrCreatePool<KinematicState>();
        _diagnosticPool = components.GetOrCreatePool<DiagnosticComponent>();
    }

    public void Draw(Camera3D camera)
    {
        _lineEffect.View = camera.View;
        _lineEffect.Projection = camera.Projection;
        _lineEffect.World = Matrix.Identity;

        var vertexCount = 0;
        var entities = _world.TypeTracker.GetEntitiesWith<DiagnosticComponent>();

        foreach (var entity in entities)
        {
            ref var diagnostic = ref _diagnosticPool.Get(entity.Id);
            if (!diagnostic.IsUnstable)
                continue;

            ref var transform = ref _transformPool.Get(entity.Id);
            ref var kinematic = ref _kinematicPool.Get(entity.Id);

            var velocity = kinematic.Velocity * _config.TimeStep;

            var start = transform.Position;
            var end = start - velocity;

            if (vertexCount + 1 >= _lineVertices.Length)
                break;

            _lineVertices[vertexCount++] = new VertexPositionColor(start, VectorColor);
            _lineVertices[vertexCount++] = new VertexPositionColor(end, VectorColor);
        }

        if (vertexCount < 2)
            return;

        if (vertexCount % 2 != 0)
            vertexCount--;

        _vertexBuffer.SetData(_lineVertices, 0, vertexCount);

        _graphics.BlendState = BlendState.Opaque;
        _graphics.DepthStencilState = DepthStencilState.None;
        _graphics.RasterizerState = RasterizerState.CullNone;

        foreach (var pass in _lineEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _graphics.SetVertexBuffer(_vertexBuffer);
            _graphics.Indices = null;
            _graphics.DrawPrimitives(PrimitiveType.LineList, 0, vertexCount / 2);
        }
    }

    public void Dispose()
    {
        _lineEffect?.Dispose();
        _vertexBuffer?.Dispose();
        GC.SuppressFinalize(this);
    }
}
