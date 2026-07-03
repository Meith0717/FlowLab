// RigidBodyRenderer.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using FlowLab.Ecs.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Graphics.Camera;

namespace FlowLab.Rigid_Bodies;

public class RigidBodyRenderer(World world, GraphicsDevice graphics)
{
    private readonly Entity[] _buffer = new Entity[2048];
    private readonly ComponentPool<Transform3D> _transformPool =
        world.Components.GetOrCreatePool<Transform3D>();
    private readonly ComponentPool<RigidBodyComponent> _rigidBodyPool =
        world.Components.GetOrCreatePool<RigidBodyComponent>();

    public void Draw(Camera3D camera)
    {
        var rigidBodySpan = world.TypeTracker.GetEntitiesWith<Transform3D, RigidBodyComponent>(
            _buffer
        );

        foreach (var entity in rigidBodySpan)
        {
            ref var transform = ref _transformPool.Get(entity.Id);
            ref var rigidBody = ref _rigidBodyPool.Get(entity.Id);

            var transformMatrix =
                Matrix.CreateTranslation(transform.Position)
                * transform.Orientation
                * Matrix.CreateScale(transform.Scale);

            rigidBody.Model.Draw(camera, transformMatrix);
        }
    }
}
