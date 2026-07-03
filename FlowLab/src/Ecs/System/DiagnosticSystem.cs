using System.Threading;
using FlowLab.Config;
using FlowLab.Ecs.Components;
using FlowLab.Ecs.Tags;
using FlowLab.Sph;
using Microsoft.Xna.Framework.Input;
using MonoKit.Ecs;
using MonoKit.Ecs.Components;
using MonoKit.Ecs.Systems;
using MonoKit.Gameplay;
using MonoKit.Input;

public class DiagnosticSystem(SimConfig config, SimulationController simController) : ISystem
{
    public int Priority => 4;
    private ComponentPool<DiagnosticComponent> _diagnosticPool;
    private ComponentPool<SolverState> _solverPool;
    private ComponentPool<KinematicState> _kinematicPool;
    private ComponentPool<MaterialComponent> _materialPool;

    public void Initialize(World world)
    {
        _diagnosticPool = world.Components.GetOrCreatePool<DiagnosticComponent>();
        _kinematicPool = world.Components.GetOrCreatePool<KinematicState>();
        _solverPool = world.Components.GetOrCreatePool<SolverState>();
        _materialPool = world.Components.GetOrCreatePool<MaterialComponent>();
    }

    public void Update(
        double elapsedMs,
        World world,
        RuntimeContainer runtimeServices,
        InputHandler inputHandler
    )
    {
        var entities = world.TypeTracker.GetEntitiesWith<ParticleTag>();
        var unstableCount = 0;

        foreach (var entity in entities)
        {
            ref var diagnostic = ref _diagnosticPool.Get(entity.Id);
            ref var kinematic = ref _kinematicPool.Get(entity.Id);
            ref var material = ref _materialPool.Get(entity.Id);

            // NaN check
            if (
                !float.IsFinite(kinematic.Velocity.X)
                || !float.IsFinite(kinematic.Velocity.Y)
                || !float.IsFinite(kinematic.Velocity.Z)
            )
            {
                simController.Pause();
                new Thread(() =>
                    MessageBox.Show(
                        "Simulation Instability Detected",
                        $"Invalid values were detected (NaN/Infinity)." + "\n\nSimulation Paused",
                        ["Ok"]
                    )
                ).Start();
            }

            diagnostic.PreviousCfl = diagnostic.Cfl;
            diagnostic.Cfl =
                config.TimeStep * (kinematic.Velocity.Length() / config.MaxParticleSize);

            if (diagnostic.PreviousCfl == 0)
                continue;

            var cflDiff = diagnostic.Cfl - diagnostic.PreviousCfl;
            diagnostic.IsStable = cflDiff < 2;

            if (!diagnostic.IsStable)
                unstableCount++;
        }

        if (unstableCount <= 1)
            return;

        simController.Pause();
        new Thread(() =>
            MessageBox.Show(
                "Simulation Instability Detected",
                $"A total of {unstableCount} particles have exceeded the stability limit."
                    + "\n\nSimulation Paused",
                ["Ok"]
            )
        ).Start();
    }
}
