// FlowLab - BlueprintPlacer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Numerics;
using FlowLab.Core;
using FlowLab.Geometry.Meshes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Game.Camera;

namespace FlowLab.Authoring;

public class BlueprintPlacer(GraphicsDevice graphicsDevice) : IDisposable
{
    private readonly BlueprintRenderer _blueprintRenderer = new(graphicsDevice);
    private readonly List<Blueprint> _blueprints = [];
    private int _currentMeshIndex;

    public Blueprint CreateBlueprint(Mesh mesh, ParticleType type)
    {
        var blueprint = new Blueprint(mesh) { ParticleType = type };
        _blueprints.Add(blueprint);
        _blueprintRenderer.AddMesh(mesh);
        _currentMeshIndex = _blueprints.Count - 1;
        return blueprint;
    }

    public bool TryGetCurrentMesh(out Blueprint blueprint)
    {
        blueprint = null;
        if (_currentMeshIndex < 0 || _currentMeshIndex >= _blueprints.Count)
            return false;

        blueprint = _blueprints[_currentMeshIndex];
        return true;
    }

    public void RemoveCurrentMesh()
    {
        if (_currentMeshIndex < 0 || _currentMeshIndex >= _blueprints.Count)
            throw new ArgumentOutOfRangeException();

        _blueprints.RemoveAt(_currentMeshIndex);
    }

    public void NextBlueprint()
    {
        _currentMeshIndex++;
        if (_currentMeshIndex >= _blueprints.Count)
            _currentMeshIndex = -1;
    }

    public void Draw(Camera3D camera)
    {
        for (var i = 0; i < _blueprints.Count; i++)
        {
            var blueprint = _blueprints[i];
            var translation = Matrix4x4.CreateTranslation(blueprint.Translation);
            var rotation = Matrix4x4.CreateFromYawPitchRoll(
                blueprint.Rotation.X,
                blueprint.Rotation.Y,
                blueprint.Rotation.Z
            );
            var scale = Matrix4x4.CreateScale(blueprint.Scale);
            var model = translation * rotation * scale;

            var color = blueprint.ParticleType switch
            {
                ParticleType.Fluid => Color.DodgerBlue,
                ParticleType.StaticBoundary => Color.Black,
                ParticleType.DynamicBoundary => Color.LightGreen,
                _ => throw new ArgumentOutOfRangeException(),
            };
            color = i == _currentMeshIndex ? Color.Red : color;

            _blueprintRenderer.DrawMesh(blueprint.Mesh, camera, model, color);
        }
    }

    public void Dispose()
    {
        _blueprintRenderer.Dispose();
        GC.SuppressFinalize(this);
    }
}
