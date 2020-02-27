using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RockDamageSystem : JobComponentSystem
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
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        var damages = query.ToComponentDataArray<Damage>(Unity.Collections.Allocator.TempJob);
        var jobDeps = Entities.ForEach((Entity entity, int entityInQueryIndex, ref HealthData healthData, ref NonUniformScale scale) =>
        {
            foreach(var damage in damages)
            {
                if (damage.Target == entity)
                {
                    healthData.Value -= damage.Value;
                }
            }

            scale.Value = new float3(scale.Value.x, healthData.Value, scale.Value.z);

            if(healthData.Value <= 0)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
            
        }).WithDeallocateOnJobCompletion(damages).Schedule(inputDeps);

        EntityManager.DestroyEntity(query);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
