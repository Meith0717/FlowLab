// FlowLab - MeshPlacerAndRenderer.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System;
using System.Collections.Generic;
using System.Numerics;
using FlowLab.Core;
using FlowLab.Geometry.Meshes;
using Microsoft.Xna.Framework.Graphics;
using MonoKit.Game.Camera;
using Vector3 = System.Numerics.Vector3;

namespace FlowLab.Authoring;

public class MeshPlacerAndRenderer(GraphicsDevice graphicsDevice)
{
    private readonly List<WorldModel> _worldModels = [];
    private int _currentMeshIndex;

    public void AddMesh(Mesh mesh, ParticleType type)
    {
        _worldModels.Add(
            new WorldModel(graphicsDevice, mesh, type)
            {
                Translation = Vector3.Zero,
                Rotation = Vector3.Zero,
                Scale = Vector3.One,
            }
        );
        _currentMeshIndex = _worldModels.Count - 1;
    }

    public WorldModel GetCurrentMesh()
    {
        if (_currentMeshIndex < 0 || _currentMeshIndex >= _worldModels.Count)
            throw new ArgumentOutOfRangeException();

        return _worldModels[_currentMeshIndex];
    }

    public void RemoveCurrentMesh()
    {
        if (_currentMeshIndex < 0 || _currentMeshIndex >= _worldModels.Count)
            throw new ArgumentOutOfRangeException();

        _worldModels.RemoveAt(_currentMeshIndex);
    }

    public bool TrySelectMesh(int index)
    {
        if (_currentMeshIndex < 0 || _currentMeshIndex >= _worldModels.Count)
            return false;

        _currentMeshIndex = index;
        return true;
    }

    public void NextMesh()
    {
        _currentMeshIndex++;
        _currentMeshIndex %= _worldModels.Count;
    }

    public void Draw(Camera3D camera)
    {
        foreach (var mesh in _worldModels)
        {
            var translation = Matrix4x4.CreateTranslation(mesh.Translation);
            var rotation = Matrix4x4.CreateFromYawPitchRoll(
                mesh.Rotation.X,
                mesh.Rotation.Y,
                mesh.Rotation.Z
            );
            var scale = Matrix4x4.CreateScale(mesh.Scale);

            var model = translation * rotation * scale;
            mesh.DrawableMesh.Draw(camera, model);
        }
    }
}
