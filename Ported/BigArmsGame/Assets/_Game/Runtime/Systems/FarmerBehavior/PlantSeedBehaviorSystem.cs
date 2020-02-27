using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(BehaviorSelectorSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class PlantSeedBehaviorSystem : JobComponentSystem
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

        var random = new Unity.Mathematics.Random((uint)UnityEngine.Time.realtimeSinceStartup);

        var jobDeps = Entities.WithAll<FarmerTag>().ForEach((Entity entity, int entityInQueryIndex, ref FarmerBehaviorData behavior, ref TargetEntityData targetEntityData, in DynamicBuffer<PathData> pathData, in LogicalPosition logicalPosition) =>
        {
            if (behavior.Value == FarmerBehavior.PlantSeed)
            {
                if (behavior.HasBoughtSeeds == false)
                {
                    if (Pathing.IsStore(tiles, map.MapSize.x, logicalPosition.PositionX, logicalPosition.PositionY))
                    {
                        behavior.HasBoughtSeeds = true;
                    }
                    else if (pathData.Length == 0)
                    {
                        var outputPath = new NativeList<int>(Allocator.Temp);
                        Pathing.FindNearbyStore(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, 20, ref outputPath);
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
                else if (Pathing.IsReadyForPlant(tiles, map.MapSize.x, logicalPosition.PositionX, logicalPosition.PositionY))
                {
                    pathData.Clear();

                    var modifier = commandBuffer.CreateEntity(entityInQueryIndex);
                    commandBuffer.AddComponent(entityInQueryIndex, modifier, new TileModifierData() { PosX = logicalPosition.PositionX, PosY = logicalPosition.PositionY, NextType = TileTypes.None });
                }
                else if (pathData.Length == 0)
                {
                    if (random.NextFloat(0.0f, 1.0f) < .1f)
                    {
                        behavior.Value = FarmerBehavior.None;
                    }
                    else
                    {
                        var outputPath = new NativeList<int>(Allocator.Temp);
                        Pathing.FindNearbyTilled(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, 20, ref outputPath);
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

        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
