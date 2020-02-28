using Unity.Entities;
using Unity.Jobs;

public class ReservationSystem : JobComponentSystem
{
    private EntityCommandBufferSystem m_EndSimulationECBSystem;
    private EntityQuery m_Query;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_EndSimulationECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_Query = GetEntityQuery(typeof(ReservationData));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ecb = m_EndSimulationECBSystem.CreateCommandBuffer().ToConcurrent();

        var reservations = m_Query.ToComponentDataArray<ReservationData>(Unity.Collections.Allocator.TempJob);
        var targetData = GetComponentDataFromEntity<TargetEntityData>();

        var jh = Entities.WithReadOnly(reservations).WithReadOnly(targetData).WithAll<IsHarvestableTag>().ForEach((Entity entity, int entityInQueryIndex, in LogicalPosition data) =>
        {
            for (int i = 0; i < reservations.Length; ++i)
            {
                if (reservations[i].PlantEntity == entity)
                {
                    var farmerTarget = targetData[reservations[i].FarmerEntity];

                    farmerTarget.Value = reservations[i].PlantEntity;
                    ecb.SetComponent(entityInQueryIndex, reservations[i].FarmerEntity, farmerTarget);

                    var tileModifier = ecb.CreateEntity(entityInQueryIndex);
                    ecb.AddComponent(entityInQueryIndex, tileModifier, new TileModifierData { NextType = TileTypes.Reserved, PosX = data.PositionX, PosY = data.PositionY });

                    ecb.RemoveComponent<IsHarvestableTag>(entityInQueryIndex, entity);
                    ecb.AddComponent<IsReservedTag>(entityInQueryIndex, entity);
                    break;
                }
            }
        }).Schedule(inputDeps);

        reservations.Dispose(jh);

        m_EndSimulationECBSystem.AddJobHandleForProducer(jh);

        return jh;
    }
}