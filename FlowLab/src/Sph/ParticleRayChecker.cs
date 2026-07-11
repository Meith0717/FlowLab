// ParticlePicker.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Collections.Generic;
using FlowLab.Ecs.Components;
using FlowLab.Extensions;
using Microsoft.Xna.Framework;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Entities;
using MonoKit.Spatial;

namespace FlowLab.Sph;

public class ParticleRayChecker(World world, EcsSpatialHash3D spatialHash)
{
    private const int MaxCellsToCheck = 50;

    private readonly ComponentPool<Transform3D> _transformPool =
        world.Components.GetOrCreatePool<Transform3D>();
    private readonly ComponentPool<ParticleProperties> _materialPool =
        world.Components.GetOrCreatePool<ParticleProperties>();
    private readonly List<Entity> _lst = [];

    public Entity? HitEntity { get; private set; }
    public Vector3 Position { get; private set; }

    public void CheckAlongRay(Ray ray)
    {
        HitEntity = null;
        Position = ray.Position;

        var rayT = ray.Position;
        var step = ray.Direction * spatialHash.CellSize;

        var cellCount = 0;
        while (cellCount < MaxCellsToCheck)
        {
            _lst.Clear();
            if (spatialHash.TryGetInRadius(rayT, spatialHash.CellSize, _lst))
                if (CheckForHit(ray, _lst))
                    break;
            cellCount++;
            rayT += step;
        }

        if (HitEntity == null)
            return;

        Position = _transformPool.Get(HitEntity.Value.Id).Position;
    }

    private bool CheckForHit(Ray ray, List<Entity> entities)
    {
        var hit = false;
        var closestDistanceToRay = float.MaxValue;

        foreach (var entity in entities)
        {
            ref var transform = ref _transformPool.Get(entity.Id);
            ref var material = ref _materialPool.Get(entity.Id);

            var entityPosition = transform.Position;
            var halfEntitySize = material.Size / 2f;

            var closestPointOnRay = ray.ClosestPoint(entityPosition);
            var distanceToRay = Vector3.Distance(entityPosition, closestPointOnRay);

            if (distanceToRay > halfEntitySize || closestDistanceToRay < distanceToRay)
                continue;

            closestDistanceToRay = distanceToRay;
            HitEntity = entity;
            hit = true;
        }

        return hit;
    }
}
