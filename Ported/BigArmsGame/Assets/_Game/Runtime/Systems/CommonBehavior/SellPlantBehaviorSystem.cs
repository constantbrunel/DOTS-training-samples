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

        RequireSingletonForUpdate<FarmData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ecb = m_EndSimulationECBSystem.CreateCommandBuffer().ToConcurrent();
        FarmData farm = GetSingleton<FarmData>();
        var allReservedPlants = GetComponentDataFromEntity<IsReservedTag>();
        NativeArray<TileDescriptor> array = m_FarmGeneratorSystem.tiles;
        int2 mapSize = m_FarmGeneratorSystem.MapSize;

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Time.realtimeSinceStartup);

        var jh = Entities.WithReadOnly(array).WithReadOnly(allReservedPlants).ForEach((Entity entity, int entityInQueryIndex, ref TargetEntityData target, ref DynamicBuffer<PathData> pathData, ref FarmerBehaviorData behaviorData, in LogicalPosition pos) =>
        {
            if (behaviorData.Value != FarmerBehavior.SellPlant)
            {
                return;
            }

            if(target.Value == Entity.Null && behaviorData.HeldPlant == Entity.Null)
            {
                // Find near harvestable to continue
                NativeList<int> outputPath = new NativeList<int>(Allocator.Temp);
                var result = Pathing.FindNearbyHarvestable(array, mapSize.x, mapSize.y, pos.PositionX, pos.PositionY, 20, ref outputPath);

                if (result != Entity.Null && !allReservedPlants.Exists(result))
                {
                    for (int i = 0; i < outputPath.Length; ++i)
                    {
                        Pathing.Unhash(mapSize.x, mapSize.y, outputPath[i], out int posX, out int posY);
                        pathData.Add(new PathData { Position = new int2(posX, posY) });
                    }
                    target.Value = result;
                }
                else
                {
                    behaviorData.Value = FarmerBehavior.None;
                }
                outputPath.Dispose();
                return;
            }

            if (GetEntityOnTile(array, mapSize.x, pos.PositionX, pos.PositionY) == target.Value)
            {
                if (behaviorData.HeldPlant == Entity.Null)
                {
                    // Store the reference to the plant entity
                    behaviorData.HeldPlant = target.Value;

                    // Create the tile modifier to remove planted type and be able to plant see again on this tile.
                    var tileModifierEntity = ecb.CreateEntity(entityInQueryIndex);
                    ecb.AddComponent(entityInQueryIndex, tileModifierEntity, new TileModifierData { NextType = TileTypes.Tilled, PosX = pos.PositionX, PosY = pos.PositionY });

                    // Find path to the store
                    NativeList<int> outputPath = new NativeList<int>(Allocator.Temp);
                    var result = Pathing.FindNearbyStore(array, mapSize.x, mapSize.y, pos.PositionX, pos.PositionY, 20, ref outputPath);

                    if (result != Entity.Null)
                    {
                        for (int i = 0; i < outputPath.Length; ++i)
                        {
                            Pathing.Unhash(mapSize.x, mapSize.y, outputPath[i], out int posX, out int posY);
                            pathData.Add(new PathData { Position = new int2(posX, posY) });
                        }
                        target.Value = result;
                    }

                    outputPath.Dispose();
                }
                else
                {
                    ecb.RemoveComponent<IsReservedTag>(entityInQueryIndex, behaviorData.HeldPlant);
                    ecb.AddComponent(entityInQueryIndex, behaviorData.HeldPlant, new IsSoldData { StoreX = pos.PositionX, StoreY = pos.PositionY });

                    behaviorData.HeldPlant = Entity.Null;

                    // reset behavior
                    float rand = random.NextFloat(0f, 1f);
                    if(rand <= 0.2f)
                    {
                        behaviorData.Value = FarmerBehavior.None;
                    }
                    else
                    {
                        // Find near harvestable to continue
                        NativeList<int> outputPath = new NativeList<int>(Allocator.Temp);
                        var result = Pathing.FindNearbyHarvestable(array, mapSize.x, mapSize.y, pos.PositionX, pos.PositionY, 20, ref outputPath);

                        if (result != Entity.Null)
                        {
                            for (int i = 0; i < outputPath.Length; ++i)
                            {
                                Pathing.Unhash(mapSize.x, mapSize.y, outputPath[i], out int posX, out int posY);
                                pathData.Add(new PathData { Position = new int2(posX, posY) });
                            }
                            target.Value = result;
                        }
                        else
                        {
                            behaviorData.Value = FarmerBehavior.None;
                        }
                    }
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
