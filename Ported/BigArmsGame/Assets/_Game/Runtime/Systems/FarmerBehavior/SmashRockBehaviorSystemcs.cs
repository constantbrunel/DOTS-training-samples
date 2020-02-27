using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(BehaviorSelectorSystem))]
[UpdateInGroup(typeof(SimulationSystemGroup))]
public class SmashRockBehaviorSystem : JobComponentSystem
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

        var rockTags = GetComponentDataFromEntity<RockTag>(true);

        var jobDeps = Entities.WithAll<FarmerTag>().WithReadOnly(rockTags).ForEach((Entity entity, int entityInQueryIndex, ref FarmerBehaviorData behavior, ref TargetEntityData targetEntityData, in DynamicBuffer<PathData> pathData, in LogicalPosition logicalPosition) =>
        {
            if (behavior.Value == FarmerBehavior.SmashRock)
            {
                if (targetEntityData.Value == Entity.Null || !rockTags.Exists(targetEntityData.Value))
                {
                    var outputPath = new NativeList<int>(Allocator.Temp);
                    Entity newTarget = Pathing.FindNearbyRock(tiles, map.MapSize.x, map.MapSize.y, logicalPosition.PositionX, logicalPosition.PositionY, 20, ref outputPath);
                    UnityEngine.Debug.Log($"old target {targetEntityData.Value.Index}-{targetEntityData.Value.Version} - new target {newTarget.Index}-{newTarget.Version}");
                    targetEntityData.Value = newTarget;
                    if (targetEntityData.Value == null)
                    {
                        behavior.Value = FarmerBehavior.None;
                    }
                    else
                    {
                        var buffer = commandBuffer.SetBuffer<PathData>(entityInQueryIndex, entity);
                        
                        for(int i = 0; i < outputPath.Length; ++i)
                        {
                            Pathing.Unhash(map.MapSize.x, map.MapSize.y, outputPath[i], out int x, out int y);
                            buffer.Add(new PathData()
                            {
                                Position = new int2(x, y)
                            }); ;
                        }
                    }
                    outputPath.Dispose();
                }
                else if (pathData.Length == 1)
                {
                    var damageEntity = commandBuffer.CreateEntity(entityInQueryIndex);
                    commandBuffer.AddComponent(entityInQueryIndex, damageEntity, new Damage()
                    {
                        Value = 1,
                        Source = entity,
                        Target = targetEntityData.Value
                    });
                }
            }

        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
