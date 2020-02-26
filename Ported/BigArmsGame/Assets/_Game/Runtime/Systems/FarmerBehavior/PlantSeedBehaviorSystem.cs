using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(BehaviorSelectorSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class PlantSeedBehaviorSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // TO DO
        return default;
    }
}
