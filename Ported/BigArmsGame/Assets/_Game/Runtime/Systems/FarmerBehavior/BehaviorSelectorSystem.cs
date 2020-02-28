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
        var ecb = m_EndSimulationSystemGroupCommandBuffer.CreateCommandBuffer().ToConcurrent();

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Time.realtimeSinceStartup);

        var jobHandle = Entities
            .WithAll<FarmerTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref FarmerBehaviorData behavior, ref DynamicBuffer<PathData> pathBuffer, ref TargetEntityData target) =>
            {
                if(behavior.Value == FarmerBehavior.None)
                {
                    // Clear path before selecting a behavior
                    pathBuffer.Clear();

                    // Select a behavior
                    int rand = 1;// random.NextInt(0, 4);
                    if (rand == 0)
                    {
                        ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.SmashRock });
                    }
                    else if (rand == 1)
                    {
                        if(target.Value != Entity.Null)
                        {
                            ecb.DestroyEntity(entityInQueryIndex, target.Value);
                            target.Value = Entity.Null;
                        }
                        ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.TillGround });
                    }
                    else if (rand == 2)
                    {
                        ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.PlantSeed });
                    }
                    else if (rand == 3)
                    {
                        ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.SellPlant });
                    }
                }
            }).Schedule(inputDeps);

        m_EndSimulationSystemGroupCommandBuffer.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
