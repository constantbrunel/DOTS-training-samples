using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class BehaviorSelectorSystem : JobComponentSystem
{
    private EntityCommandBufferSystem m_EndSimulationSystemGroupCommandBuffer;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_EndSimulationSystemGroupCommandBuffer = World.GetExistingSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = m_EndSimulationSystemGroupCommandBuffer.CreateCommandBuffer().ToConcurrent();

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 9999));

        var jobHandle = Entities
            .WithAll<FarmerTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref FarmerBehaviorData behavior, ref DynamicBuffer<PathData> pathBuffer, ref TargetEntityData target) =>
            {
                if(behavior.Value == FarmerBehavior.None)
                {
                    // Clear path before selecting a behavior
                    pathBuffer.Clear();

                    if(behavior.BehaviourType == BehaviourType.Farmer)
                    {
                        var rand = random.NextInt(0, 5);
                        switch(rand)
                        {
                            case 0:
                                behavior.Value = FarmerBehavior.TillGround;
                                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
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

                            default:
                                break;
                        }
                    }
                    else
                    {
                        behavior.Value = FarmerBehavior.SellPlant;
                    }

                    behavior.HeldPlant = Entity.Null;
                    target.Value = Entity.Null;
                }
            }).Schedule(inputDeps);

        m_EndSimulationSystemGroupCommandBuffer.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
