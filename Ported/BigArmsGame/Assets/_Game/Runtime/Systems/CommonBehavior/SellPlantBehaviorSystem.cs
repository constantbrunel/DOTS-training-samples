using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(BehaviorSelectorSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SellPlantBehaviorSystem : JobComponentSystem
{
    private EndSimulationEntityCommandBufferSystem m_EndSimulationECBSystem;
    private EntityQuery m_AllHarvestablePlantsQuery;
    private FarmGeneratorSystem m_FarmGeneratorSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_EndSimulationECBSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_FarmGeneratorSystem = World.GetOrCreateSystem<FarmGeneratorSystem>();

        m_AllHarvestablePlantsQuery = EntityManager.CreateEntityQuery(typeof(IsHarvestableTag));

        RequireSingletonForUpdate<FarmData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var ecb = m_EndSimulationECBSystem.CreateCommandBuffer().ToConcurrent();
        FarmData farm = GetSingleton<FarmData>();
        var allPlants = GetComponentDataFromEntity<PlantDataComp>();
        var allLogicalPos = GetComponentDataFromEntity<LogicalPosition>();
        NativeArray<TileDescriptor> array = m_FarmGeneratorSystem.tiles;
        int2 mapSize = m_FarmGeneratorSystem.MapSize;

        bool IsHarvestableExist = !m_AllHarvestablePlantsQuery.IsEmptyIgnoreFilter;

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 9999));

        var jh = Entities.WithReadOnly(array).WithReadOnly(allPlants).WithReadOnly(allLogicalPos)
            .ForEach((Entity entity, int entityInQueryIndex, ref TargetEntityData target, ref DynamicBuffer<PathData> pathData, ref FarmerBehaviorData behaviorData, in LogicalPosition pos, in Translation farmerPos) =>
        {
            if (behaviorData.Value != FarmerBehavior.SellPlant)
            {
                return;
            }

            if (!IsHarvestableExist)
            {
                behaviorData.Value = FarmerBehavior.None;
                return;
            }

            if (target.Value == Entity.Null && behaviorData.HeldPlant == Entity.Null)
            {
                // Find near harvestable to continue
                NativeList<int> outputPath = new NativeList<int>(Allocator.Temp);
                var result = Pathing.FindNearbyHarvestable(array, mapSize.x, mapSize.y, pos.PositionX, pos.PositionY, 20, ref outputPath);

                if (result != Entity.Null)
                {
                    var reservationEntity = ecb.CreateEntity(entityInQueryIndex);
                    ecb.AddComponent(entityInQueryIndex, reservationEntity, new ReservationData { FarmerEntity = entity, PlantEntity = result });

                }
                else
                {
                    behaviorData.Value = FarmerBehavior.None;
                }
                outputPath.Dispose();
                return;
            }

            if (target.Value != Entity.Null && behaviorData.HeldPlant == Entity.Null)
            {
                if (!allPlants.Exists(target.Value))
                {
                    target.Value = Entity.Null;
                    // We got a problem
                    //UnityEngine.Debug.Log($"Reset behavior because the plant we aimed does not exist anymore");
                    behaviorData.Value = FarmerBehavior.None;
                    return;
                }

                var targetPos = allLogicalPos[target.Value];
                NativeList<int> outputPath = new NativeList<int>(Allocator.Temp);
                var result = Pathing.FindNearbyHarvestable(array, mapSize.x, mapSize.y, targetPos.PositionX, targetPos.PositionY, 0, ref outputPath);
                for (int i = 0; i < outputPath.Length; ++i)
                {
                    Pathing.Unhash(mapSize.x, mapSize.y, outputPath[i], out int posX, out int posY);
                    pathData.Add(new PathData { Position = new int2(posX, posY) });
                }
                outputPath.Dispose();
            }


            if (behaviorData.HeldPlant != Entity.Null)
            {
                if (!allPlants.Exists(behaviorData.HeldPlant))
                {
                    behaviorData.HeldPlant = Entity.Null;
                    target.Value = Entity.Null;
                    return;
                }
                ecb.SetComponent(entityInQueryIndex, behaviorData.HeldPlant, new Translation { Value = new float3(farmerPos.Value.x, 2f, farmerPos.Value.z) });
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
                    if (rand <= 0.1f)
                    {
                        //UnityEngine.Debug.Log($"Reset behavior with random");
                        behaviorData.Value = FarmerBehavior.None;
                    }
                    else
                    {
                        //UnityEngine.Debug.Log($"Try to find another plant");
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
                            //UnityEngine.Debug.Log($"New plant found at pos {pathData[0].Position}");
                            target.Value = result;

                            ecb.RemoveComponent<IsHarvestableTag>(entityInQueryIndex, result);
                            ecb.AddComponent<IsReservedTag>(entityInQueryIndex, result);
                        }
                        else
                        {
                            //UnityEngine.Debug.Log($"No plant found :( :sadparrot:");
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
