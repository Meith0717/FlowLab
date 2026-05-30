// SimulationControler.cs
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
    private readonly World _world = world;

    public bool IsPaused { get; private set; }
    public bool HideBoundary { get; private set; }

    public void Update(double elapsedMilliseconds, InputHandler inputHandler)
    {
        if (inputHandler.HasAction((byte)ActionType.PauseSimulation))
            TogglePause();

        if (inputHandler.HasAction((byte)ActionType.HideBoundary))
            ToggleHideBoundary();

        if (inputHandler.HasAction((byte)ActionType.ClearFluid) && IsPaused)
            ClearFluid();
    }

    private void TogglePause() => IsPaused = !IsPaused;

    private void ToggleHideBoundary() => HideBoundary = !HideBoundary;

    private void ClearFluid()
    {
        var fluidCollection = _world.TypeTracker.GetEntitiesWith<FluidTag>();
        var lifePool = _world.Components.GetOrCreatePool<Lifetime>();
        Parallel.ForEach(
            fluidCollection,
            fluidEntity =>
            {
                ref var lifeTime = ref lifePool.Get(fluidEntity.Id);
                lifeTime.DestroyNow = true;
            }
        );
    }
}
