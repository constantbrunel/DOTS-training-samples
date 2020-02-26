using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class FarmGeneratorSystem : JobComponentSystem
{
    private EntityQuery m_QueryForFarmNeedingGeneration;

    public NativeArray<TileTypes> tiles;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_QueryForFarmNeedingGeneration = GetEntityQuery(typeof(FarmData), typeof(FarmNeedGenerationTag));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Build farm bitches
        NativeArray<FarmData> farmDatas = m_QueryForFarmNeedingGeneration.ToComponentDataArray<FarmData>(Allocator.TempJob);

        FarmData farm = farmDatas[0];

        // Create tiles
        for(int x = 0; x < farm.MapSize.x; ++x)
        {
            for (int y = 0; y < farm.MapSize.y; ++y)
            {
                Entity tileEntity = EntityManager.Instantiate(farm.TileEntity);
                EntityManager.SetComponentData(tileEntity, new Translation
                {
                    Value = new float3(x, 0, y)
                });
            }
        }

        farmDatas.Dispose();
        EntityManager.RemoveComponent<FarmNeedGenerationTag>(m_QueryForFarmNeedingGeneration);
        return default;
    }

    public static TileTypes GetTileType(int x, int y)
    {
        return TileTypes.None;
    }
}
