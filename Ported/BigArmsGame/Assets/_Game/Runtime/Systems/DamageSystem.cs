using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class DamageSystem : JobComponentSystem
{
    private EntityQuery query;
    private EntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        query = GetEntityQuery(typeof(Damage));

        RequireForUpdate(query);
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBufferConcurrent = commandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var commandBuffer = commandBufferSystem.CreateCommandBuffer();

        var damages = query.ToComponentDataArray<Damage>(Unity.Collections.Allocator.TempJob);
        var jobDeps = Entities.ForEach((Entity entity, int entityInQueryIndex, ref HealthData healthData, ref NonUniformScale scale) =>
        {
            for(int i = 0; i < damages.Length; ++i)
            {
                if (damages[i].Target == entity)
                {
                    healthData.CurrentValue -= damages[i].Value;
                }
            }

            var y = healthData.CurrentValue / healthData.MaxValue * healthData.MaxValue / 100;
            scale.Value = new float3(scale.Value.x, y, scale.Value.z);

            if(healthData.CurrentValue <= 0)
            {
                commandBufferConcurrent.DestroyEntity(entityInQueryIndex, entity);
            }
            
        }).WithDeallocateOnJobCompletion(damages).Schedule(inputDeps);

        commandBuffer.DestroyEntity(query);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
