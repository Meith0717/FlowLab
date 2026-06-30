// SimulationController.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.Threading.Tasks;
using FlowLab.Ecs.Tags;
using FlowLab.Input;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Input;

namespace FlowLab.Sph;

public class SimulationController(World world)
{
    private bool _debugEnabled;

    public bool IsPaused { get; private set; } = true;
    public bool HideBoundary { get; private set; }
    public bool ShowSpatialGrids { get; private set; }

    public void TogglePause() => IsPaused = !IsPaused;

    public void Pause() => IsPaused = true;

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        if (inputHandler.HasAction((byte)ActionType.PauseSimulation))
            IsPaused = !IsPaused;

        if (inputHandler.HasAction((byte)ActionType.ClearFluid))
            ClearFluid();

        if (inputHandler.HasAction((byte)ActionType.HideBoundary))
            HideBoundary = !HideBoundary;

        if (inputHandler.HasAction((byte)ActionType.ToggleDebug))
            ToggleDebug();

        if (inputHandler.HasAction((byte)ActionType.ToggleSpatialGrids) && _debugEnabled)
            ShowSpatialGrids = !ShowSpatialGrids;
    }

    public void ClearFluid()
    {
        var fluidCollection = world.TypeTracker.GetEntitiesWith<FluidTag>();
        var lifePool = world.Components.GetOrCreatePool<Lifetime>();

        Parallel.ForEach(
            fluidCollection,
            fluidEntity =>
            {
                ref var lifeTime = ref lifePool.Get(fluidEntity.Id);
                lifeTime.DestroyNow = true;
            }
        );
    }

    private void ToggleDebug()
    {
        _debugEnabled = !_debugEnabled;
        if (_debugEnabled)
            return;
        ShowSpatialGrids = false;
    }
}
