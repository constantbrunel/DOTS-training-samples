using Unity.Entities;
using Unity.Jobs;

[UpdateAfter(typeof(BehaviorSelectorSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class TillGroundBehaviorSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // TO DO
        return default;
    }
}
