using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateAfter(typeof(BehaviorSelectorSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SellPlantBehaviorSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_EndSimulationECBSystem;

    private FarmGeneratorSystem m_FarmGeneratorSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_EndSimulationECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_FarmGeneratorSystem = World.GetOrCreateSystem<FarmGeneratorSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ecb = m_EndSimulationECBSystem.CreateCommandBuffer().ToConcurrent();
        FarmData farm = GetSingleton<FarmData>();

        NativeArray<TileDescriptor> array = m_FarmGeneratorSystem.tiles;
        int2 mapSize = m_FarmGeneratorSystem.MapSize;

        var jh = Entities.WithReadOnly(array).ForEach((Entity entity, int entityInQueryIndex, ref TargetEntityData target, ref DynamicBuffer<PathData> pathData, ref FarmerBehaviorData behaviorData, in LogicalPosition pos) =>
        {
            if (behaviorData.Value != FarmerBehavior.SellPlant)
            {
                return;
            }

            if (GetEntityOnTile(array, mapSize.x, pos.PositionX, pos.PositionY) == target.Value)
            {
                if (behaviorData.HeldPlant == Entity.Null)
                {
                    behaviorData.HeldPlant = target.Value;

                    // Copy from Pathing, implement a utility method
                    NativeList<int> outputPath = new NativeList<int>(Allocator.TempJob);
                    var result = Pathing.FindNearbyStore(array, mapSize.x, mapSize.y, pos.PositionX, pos.PositionY, 20, ref outputPath);

                    if (result != Entity.Null)
                    {
                        DynamicBuffer<PathData> path = new DynamicBuffer<PathData>();
                        for (int i = 0; i < outputPath.Length; ++i)
                        {
                            Pathing.Unhash(mapSize.x, mapSize.y, outputPath[i], out int posX, out int posY);
                            path.Add(new PathData { Position = new int2(posX, posY) });
                        }
                        target.Value = result;
                        pathData = path;
                    }

                    outputPath.Dispose();
                }
                else
                {
                    ecb.RemoveComponent<IsReservedTag>(entityInQueryIndex, behaviorData.HeldPlant);
                    ecb.AddComponent(entityInQueryIndex, behaviorData.HeldPlant, new IsSoldData { StoreX = pos.PositionX, StoreY = pos.PositionY });

                    // TODO - Find another target to sell ?
                }
            }
        }).Schedule(inputDeps);

        m_EndSimulationECBSystem.AddJobHandleForProducer(jh);

        return jh;
    }

    private static Entity GetEntityOnTile(NativeArray<TileDescriptor> array, int mapSizeX, int x, int y)
    {
        return array[Pathing.Hash(mapSizeX, x, y)].Entity;
    }
}
