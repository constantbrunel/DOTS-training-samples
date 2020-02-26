using Unity.Entities;
using Unity.Jobs;

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

        var rockDamages = query.ToComponentDataArray<Damage>(Unity.Collections.Allocator.TempJob);
        var jobDeps = Entities.ForEach((Entity entity, int entityInQueryIndex, ref HealthData healthData) =>
        {
            foreach(var rockDamage in rockDamages)
            {
                if (rockDamage.Target == entity)
                {
                    healthData.Value -= rockDamage.Value;
                }
            }

            if(healthData.Value <= 0)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }
            
        }).Schedule(inputDeps);

        EntityManager.DestroyEntity(query);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
