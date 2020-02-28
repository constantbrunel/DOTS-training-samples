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
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1,120000000000));

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
                // Try to assign target!
                if (targetEntityData.Value == Entity.Null)
                {
                    // First clear path
                    pathData.Clear();
                    UnityEngine.Debug.Log("Looking for a new zone to tile");

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
                        }
                        if (blocked)
                        {
                            break;
                        }
                    }
                    if (blocked == false)
                    {
                        // Get new path
                        Debug.Log("Looking for a path");
                        var outputPath = new NativeList<int>(Allocator.Temp);
                        Pathing.FindNearbyGroundInZone(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, 25, new RectInt(minX, minY, width, height), ref outputPath);
                        if (outputPath.Length == 0)
                        {
                            Debug.Log("No target aquired");
                            if (random.NextFloat(1) < .2f)
                            {
                                Debug.Log("Quiting");
                                behavior.Value = FarmerBehavior.None;
                            }
                        }
                        else
                        {
                            for (int i = 0; i < outputPath.Length; ++i)
                            {
                                Pathing.Unhash(map.MapSize.x, map.MapSize.y, outputPath[i], out int x, out int y);
                                pathData.Add(new PathData()
                                {
                                    Position = new int2(x, y)
                                });
                            }

                            // Create target!
                            var target = commandBuffer.CreateEntity(entityInQueryIndex);
                            commandBuffer.AddComponent(entityInQueryIndex, target, new TillTargetData() { PosX = minX, PosY = minY, SizeX = width, SizeY = height });
                            commandBuffer.SetComponent(entityInQueryIndex, entity, new TargetEntityData() { Value = target });
                            Debug.Log($"Target aquired - PosX = {minX}, PosY = {minY}, SizeX = {width}, SizeY = {height}");
                        }
                        outputPath.Dispose();
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
                        pathData.Clear();
                        var entModifier = commandBuffer.CreateEntity(entityInQueryIndex);
                        commandBuffer.AddComponent(entityInQueryIndex, entModifier, new TileModifierData() { PosX = logicalPosition.PositionX, PosY = logicalPosition.PositionY, NextType = TileTypes.Tilled });

                        // Till and then find new path to next zone
                        var outputPath = new NativeList<int>(Allocator.Temp);
                        Pathing.FindNearbyGroundInZone(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, 999, new RectInt(data.PosX, data.PosY, data.SizeX, data.SizeY), ref outputPath);

                        Debug.Log($"Tile ({logicalPosition.PositionX},{logicalPosition.PositionY})"); 


                        if (outputPath.Length == 0)
                        {
                            Debug.Log("Action done");
                            commandBuffer.DestroyEntity(entityInQueryIndex, targetEntityData.Value);
                            targetEntityData.Value = Entity.Null;
                            if (random.NextFloat(1) < .1f)
                            {
                                Debug.Log("Quiting action");
                                behavior.Value = FarmerBehavior.None;
                            }
                        }
                        else
                        {

                            Pathing.Unhash(map.MapSize.x, map.MapSize.y, outputPath[0], out var xa, out var ya);
                            Debug.Log($" Actual position ({logicalPosition.PositionX},{logicalPosition.PositionY}) -> New Tile to till ({xa},{ya})");

                            for (int i = 0; i < outputPath.Length; ++i)
                            {
                                Pathing.Unhash(map.MapSize.x, map.MapSize.y, outputPath[i], out int x, out int y);
                                pathData.Add(new PathData()
                                {
                                    Position = new int2(x, y)
                                });
                            }
                        }
                        outputPath.Dispose();
                    }
                    else if(pathData.Length == 0)
                    {
                        commandBuffer.DestroyEntity(entityInQueryIndex, targetEntityData.Value);
                        targetEntityData.Value = Entity.Null;
                        Debug.Log("Pathing fucked. Quiting");
                        behavior.Value = FarmerBehavior.None;
                    }
                }
            }
        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
