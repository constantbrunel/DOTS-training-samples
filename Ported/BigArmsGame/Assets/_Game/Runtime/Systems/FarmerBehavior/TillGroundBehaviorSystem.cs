using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(BehaviorSelectorSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class TillGroundBehaviorSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var commandBuffer = commandBufferSystem.CreateCommandBuffer().ToConcurrent();

        var tiles = World.GetExistingSystem<FarmGeneratorSystem>().tiles;
        var map = GetSingleton<FarmData>();

        var tillData = GetComponentDataFromEntity<TillTargetData>(true);
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0,120000000000));

        var jobDeps = Entities
            .WithReadOnly(tillData)
            .WithAll<FarmerTag>()
            .ForEach(
            (Entity entity,
            int entityInQueryIndex,
            ref FarmerBehaviorData behavior,
            ref TargetEntityData targetEntityData,
            ref DynamicBuffer<PathData> pathData,
            in LogicalPosition logicalPosition) =>
        {
            if (behavior.Value == FarmerBehavior.TillGround)
            {
                if (targetEntityData.Value == Entity.Null || !tillData.Exists(targetEntityData.Value))
                {
                    int width = random.NextInt(1, 8);
                    int height = random.NextInt(1, 8);
                    int minX = logicalPosition.PositionX + random.NextInt(-10, 10 - width);
                    int minY = logicalPosition.PositionY + random.NextInt(-10, 10 - height);
                    if (minX < 0) minX = 0;
                    if (minY < 0) minY = 0;
                    if (minX + width >= map.MapSize.x) minX = map.MapSize.x - 1 - width;
                    if (minY + height >= map.MapSize.y) minY = map.MapSize.y - 1 - height;

                    bool blocked = false;
                    for (int x = minX; x <= minX + width; x++)
                    {
                        for (int y = minY; y <= minY + height; y++)
                        {
                            TileTypes groundState = tiles[Pathing.Hash(map.MapSize.x, x, y)].TileType;
                            if (groundState != TileTypes.None && groundState != TileTypes.Tilled)
                            {
                                blocked = true;
                                break;
                            }
                            if (Pathing.IsRock(tiles, map.MapSize.x, x, y) || Pathing.IsStore(tiles, map.MapSize.x, x, y))
                            {
                                blocked = true;
                                break;
                            }
                        }
                        if (blocked)
                        {
                            break;
                        }
                    }
                    if (blocked == false)
                    {
                        var target = commandBuffer.CreateEntity(entityInQueryIndex);
                        commandBuffer.AddComponent(entityInQueryIndex, target, new TillTargetData() { PosX = minX, PosY = minY, SizeX = width, SizeY = height });
                        commandBuffer.SetComponent(entityInQueryIndex, entity, new TargetEntityData() { Value = target });
                        Debug.Log($"Target aquired - PosX = {minX}, PosY = {minY}, SizeX = {width}, SizeY = {height}");
                    }
                    else
                    {
                        Debug.Log("No target aquired");
                        if (random.NextFloat(1) < .2f)
                        {
                            Debug.Log("Quiting");
                            behavior.Value = FarmerBehavior.None;
                        }
                    }
                }
                else
                {
                    var data = tillData[targetEntityData.Value];
                    if (Pathing.IsTillableInZone(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, new RectInt(data.PosX, data.PosY, data.SizeX, data.SizeY))) 
                    {
                        Debug.Log("Tile");
                        pathData.Clear();
                        var entModifier = commandBuffer.CreateEntity(entityInQueryIndex);
                        commandBuffer.AddComponent(entityInQueryIndex, entModifier, new TileModifierData() { PosX = logicalPosition.PositionX, PosY = logicalPosition.PositionY, NextType = TileTypes.Tilled });
                    }
                    else
                    {
                        if (pathData.Length == 0)
                        {
                            Debug.Log("Looking for a path");
                            var outputPath = new NativeList<int>(Allocator.Temp);
                            Pathing.FindNearbyGroundInZone(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, 25, new RectInt(data.PosX, data.PosY, data.SizeX, data.SizeY), ref outputPath);
                            if (outputPath.Length == 0)
                            {
                                Debug.Log("No path found quiting");
                                behavior.Value = FarmerBehavior.None;
                            }
                            else
                            {
                                var buffer = commandBuffer.SetBuffer<PathData>(entityInQueryIndex, entity);

                                for (int i = 0; i < outputPath.Length; ++i)
                                {
                                    Pathing.Unhash(map.MapSize.x, map.MapSize.y, outputPath[i], out int x, out int y);
                                    Debug.Log($"New path spot {x}/{y}");
                                    buffer.Add(new PathData()
                                    {
                                        Position = new int2(x, y)
                                    });
                                }

                                outputPath.Dispose();
                                return;
                            }
                            outputPath.Dispose();
                        }
                    }

                    if (pathData.Length == 0)
                    {
                        Debug.Log("Action done");
                        if (random.NextFloat(1) < .1f)
                        {
                            Debug.Log("Quiting action");
                            behavior.Value = FarmerBehavior.None;
                        }
                        else
                        {
                            Debug.Log("Finding new temporary path");
                            var outputPath = new NativeList<int>(Allocator.Temp);
                            Pathing.FindNearbyTilled(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, 25, ref outputPath);
                            if (outputPath.Length == 0)
                            {
                                behavior.Value = FarmerBehavior.None;
                            }
                            else
                            {
                                var buffer = commandBuffer.SetBuffer<PathData>(entityInQueryIndex, entity);

                                for (int i = 0; i < outputPath.Length; ++i)
                                {
                                    Pathing.Unhash(map.MapSize.x, map.MapSize.y, outputPath[i], out int x, out int y);
                                    buffer.Add(new PathData()
                                    {
                                        Position = new int2(x, y)
                                    });
                                }
                            }
                            outputPath.Dispose();
                        }
                    }
                }
            }
        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
