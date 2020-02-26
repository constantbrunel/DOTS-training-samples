using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class FarmGeneratorSystem : JobComponentSystem
{
    private EntityQuery m_QueryForFarmNeedingGeneration;

    public NativeArray<TileDescriptor> tiles;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_QueryForFarmNeedingGeneration = GetEntityQuery(typeof(FarmData), typeof(FarmNeedGenerationTag));
        RequireForUpdate(m_QueryForFarmNeedingGeneration);
    }

    protected override void OnDestroy()
    {
        tiles.Dispose();
        base.OnDestroy();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        FarmData farm = GetSingleton<FarmData>();

        int mapX = farm.MapSize.x;
        int mapY = farm.MapSize.y;

        tiles = new NativeArray<TileDescriptor>(mapX * mapY, Allocator.Persistent);

        // Create tiles
        for (int x = 0; x < mapX; ++x)
        {
            for (int y = 0; y < mapY; ++y)
            {
                Entity tileEntity = EntityManager.Instantiate(farm.TileEntity);
                EntityManager.SetComponentData(tileEntity, new Translation
                {
                    Value = new float3(x, 0, y)
                });
                tiles[mapX * y + x] = new TileDescriptor()
                {
                    TileType = TileTypes.None,
                    Entity = Entity.Null
                };
            }
        }

        // Create store
        int spawnedStores = 0;
        while (spawnedStores < farm.StoreCount)
        {
            int x = UnityEngine.Random.Range(0, mapX);
            int y = UnityEngine.Random.Range(0, mapY);
            if (tiles[x + y * mapX].TileType != TileTypes.Store)
            {
                Entity storeEntity = EntityManager.Instantiate(farm.StoreEntity);
                EntityManager.SetComponentData(storeEntity, new Translation()
                {
                    Value = new float3(x, 0, y)
                });

                tiles[x + y * mapX] = new TileDescriptor()
                {
                    TileType = TileTypes.Store,
                    Entity = storeEntity
                };

                spawnedStores++;
            }
        }

        // Create rocks
        int rockSpawnAttempts = farm.RockSpawnAttempts;
        for (int i = 0; i < rockSpawnAttempts; i++)
        {
            int width = UnityEngine.Random.Range(0, 4);
            int height = UnityEngine.Random.Range(0, 4);
            int rockX = UnityEngine.Random.Range(0, mapX - width);
            int rockY = UnityEngine.Random.Range(0, mapY - height);

            bool blocked = false;
            for (int x = rockX; x <= rockX + width; x++)
            {
                for (int y = rockY; y <= rockY + height; y++)
                {
                    if (tiles[x + mapX * y].TileType == TileTypes.Rock|| tiles[x + mapX * y].TileType == TileTypes.Store)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) break;
            }

            if (blocked == false)
            {
                int scaleWidth = width + 1;
                int scaleHeight = height + 1;
                Entity rockEntity = EntityManager.Instantiate(farm.RockEntity);
                EntityManager.AddComponentData(rockEntity, new NonUniformScale()
                {
                    Value = new float3(scaleWidth, 1, scaleHeight)
                });
                EntityManager.AddComponentData(rockEntity, new Translation()
                {
                    Value = new float3((float)rockX - 0.5f + (float)scaleWidth / 2.0f, 0, (float)rockY - 0.5f + (float)scaleWidth / 2.0f)
                });

                for (int x = rockX; x <= rockX + width; x++)
                {
                    for (int y = rockY; y <= rockY + height; y++)
                    {
                        tiles[x + mapX * y] = new TileDescriptor()
                        {
                            TileType = TileTypes.Rock,
                            Entity = rockEntity
                        };
                    }
                }
            }
        }

        EntityManager.RemoveComponent<FarmNeedGenerationTag>(m_QueryForFarmNeedingGeneration);
        return default;
    }

    public static TileTypes GetTileType(int x, int y)
    {
        return TileTypes.None;
    }
}
