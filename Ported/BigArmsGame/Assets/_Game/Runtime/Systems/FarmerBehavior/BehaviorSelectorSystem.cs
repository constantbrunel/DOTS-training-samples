using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class BehaviorSelectorSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var random = new Unity.Mathematics.Random((uint)Random.Range(0, 999999));

        var jobHandle = Entities
            .WithAll<FarmerTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref FarmerBehaviorData behavior, ref DynamicBuffer<PathData> pathBuffer) =>
            {
                if(behavior.Value == FarmerBehavior.None)
                {
                    // Clear path before selecting a behavior
                    pathBuffer.Clear();

                    if(behavior.BehaviourType == BehaviourType.Farmer)
                    {
                        var rand = random.NextInt(0, 4);
                        switch(rand)
                        {
                            case 0:
                                behavior.Value = FarmerBehavior.TillGround;
                                break;

                            case 1:
                                behavior.Value = FarmerBehavior.PlantSeed;
                                break;

                            case 2:
                                behavior.Value = FarmerBehavior.SellPlant;
                                break;

                            case 3:
                                behavior.Value = FarmerBehavior.SmashRock;
                                break;
                        }
                    }
                    else
                    {
                        behavior.Value = FarmerBehavior.SellPlant;
                    }
                }
            }).Schedule(inputDeps);

        return jobHandle;
    }
}
