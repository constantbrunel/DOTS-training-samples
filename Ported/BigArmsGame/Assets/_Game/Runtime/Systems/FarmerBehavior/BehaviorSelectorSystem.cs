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

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 9999));

        var jobHandle = Entities
            .WithAll<FarmerTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref FarmerBehaviorData behavior, ref DynamicBuffer<PathData> pathBuffer) =>
            {
                if(behavior.Value == FarmerBehavior.None)
                {
                    // Clear path before selecting a behavior
                    pathBuffer.Clear();

                    // Select a behavior
                    int rand = random.NextInt(0, 5);
                    UnityEngine.Debug.Log($"{rand}");

                    if (rand == 0)
                    {
                        UnityEngine.Debug.Log("Is now Smashing");
                        ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.SmashRock });
                    }
                    else if (rand == 1)
                    {
                        UnityEngine.Debug.Log("Is now Tilling");
                        //ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.TillGround });
                    }
                    else if (rand == 2)
                    {
                        UnityEngine.Debug.Log("Is now Planting");
                        ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.PlantSeed });
                    }
                    else if (rand == 3)
                    {
                        UnityEngine.Debug.Log("Is now Selling");
                        ecb.SetComponent(entityInQueryIndex, entity, new FarmerBehaviorData() { Value = FarmerBehavior.SellPlant });
                    }
                }
            }).Schedule(inputDeps);

        m_EndSimulationSystemGroupCommandBuffer.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
