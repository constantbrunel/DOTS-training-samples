using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

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

        var jobDeps = Entities.WithAll<FarmerTag>().ForEach((Entity entity, int entityInQueryIndex, ref FarmerBehaviorData behavior, ref TargetEntityData targetEntityData, in DynamicBuffer<PathData> pathData) =>
        {
        if (behavior.Value == FarmerBehavior.SmashRock)
        {
                if (targetEntityData.Value == null)
                {
                    var outputPath = new NativeList<int>();
                    targetEntityData.Value = Pathing.FindNearbyRock(tiles, map.MapSize.x, map.MapSize.y, behavior.PositionX, behavior.PositionY, 20, ref outputPath);
                    targetEntityData.Value = Entity.Null;
                    if (targetEntityData.Value == null)
                    {
                        behavior.Value = FarmerBehavior.None;
                    }
                    else
                    {
                        var buffer = commandBuffer.SetBuffer<PathData>(entityInQueryIndex, entity);
                        foreach (var path in outputPath)
                        {
                            buffer.Add(new PathData()
                            {
                                Position = path
                            });
                        }
                    }
                }
                else
                {
                    if (pathData.Length == 1)
                    {
                        var damageEntity = commandBuffer.CreateEntity(entityInQueryIndex);
                        commandBuffer.AddComponent(entityInQueryIndex, damageEntity, new Damage()
                        {
                            Value = 1,
                            Source = entity
                        });
                    }
                }
            }

        }).Schedule(inputDeps);

        commandBufferSystem.AddJobHandleForProducer(jobDeps);

        return jobDeps;
    }
}
