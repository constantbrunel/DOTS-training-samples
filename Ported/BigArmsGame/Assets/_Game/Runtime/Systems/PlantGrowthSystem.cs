using Unity.Entities;
using Unity.Jobs;

public class PlantGrowthSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_EndSimulationECBSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_EndSimulationECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float dt = Time.DeltaTime;
        var ecb = m_EndSimulationECBSystem.CreateCommandBuffer().ToConcurrent();

        var jh = Entities.WithAll<IsGrowingTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref PlantDataComp data, in LogicalPosition logicalPosition) =>
        {
            data.Growth += dt;

            // TODO - Base the scale of the plant based on the Growth/GrowDuration ratio
            //float scale = data.Growth / (float)data.GrowDuration;

            if(data.Growth >= data.GrowDuration)
            {
                data.Growth = data.GrowDuration;
                ecb.RemoveComponent<IsGrowingTag>(entityInQueryIndex, entity);
                ecb.AddComponent<IsHarvestableTag>(entityInQueryIndex, entity);

                var tileModifierEntity = ecb.CreateEntity(entityInQueryIndex);
                ecb.AddComponent(entityInQueryIndex, tileModifierEntity, new TileModifierData { NextType = TileTypes.Harvestable, PosX = logicalPosition.PositionX, PosY = logicalPosition.PositionY });
            }
        }).Schedule(inputDeps);

        m_EndSimulationECBSystem.AddJobHandleForProducer(jh);

        return jh;
    }
}