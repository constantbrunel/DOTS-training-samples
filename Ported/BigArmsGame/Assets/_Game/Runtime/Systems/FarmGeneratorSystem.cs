using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

public class FarmGeneratorSystem : JobComponentSystem
{
    private EntityQuery m_QueryForFarmNeedingGeneration;

    public NativeArray<TileDescriptor> tiles;
    public int2 MapSize;

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
        MapSize = new int2(mapX, mapY);

        // Create default ground
        for (int x = 0; x < mapX; ++x)
        {
            for (int y = 0; y < mapY; ++y)
            {
                tiles[mapX * y + x] = new TileDescriptor()
                {
                    TileType = TileTypes.None,
                    Entity = Entity.Null
                };
            }
        }
        Entity tileEntity = EntityManager.Instantiate(farm.DefaultTileEntity);
        EntityManager.SetComponentData(tileEntity, new Translation
        {
            Value = new float3(mapX/2 - 0.5f, 0, mapY/2 - 0.5f)
        });
        EntityManager.AddComponentData(tileEntity, new NonUniformScale()
        {
            Value = new float3(mapX * 0.1f, 1, mapY * 0.1f)
        });

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

        // Create Tilled tiles
        int tilledTiles = 0;
        while(tilledTiles <= farm.InitialTilledTileCount)
        {
            int maxXSize = 8;
            int maxYSize = 8;
            int sizeX = UnityEngine.Random.Range(1, maxXSize);
            int sizeY = UnityEngine.Random.Range(1, maxYSize);
            int posX = UnityEngine.Random.Range(0, mapX - sizeX);
            int posY = UnityEngine.Random.Range(0, mapX - sizeY);

            bool blocked = false;
            for (int x = posX; x <= posX + sizeX; x++)
            {
                for (int y = posY; y <= posY + sizeY; y++)
                {
                    if (tiles[x + (mapX * y)].TileType == TileTypes.Store)
                    {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) break;
            }
            if (blocked)
            {
                continue;
            }

            for (int x = posX; x < posX + sizeX; ++x)
            {
                for(int y = posY; y < posY + sizeY; ++y)
                {
                    Entity tilledTile = EntityManager.Instantiate(farm.TiledTileEntity);
                    EntityManager.SetComponentData(tilledTile, new Translation() { Value = new float3(x, 0.001f, y) });
                    tiles[x + y * mapX] = new TileDescriptor()
                    {
                        TileType = TileTypes.Tilled,
                        Entity = Entity.Null
                    };
                    tilledTiles++;
                }
            }
        }

        // Create rocks
        int rockSpawnAttempts = farm.RockSpawnAttempts;
        for (int i = 0; i < rockSpawnAttempts; i++)
        {
            int width = UnityEngine.Random.Range(0, 4);
            int height = UnityEngine.Random.Range(0, 4);
            int rockX = UnityEngine.Random.Range(0, mapX - width - 1);
            int rockY = UnityEngine.Random.Range(0, mapY - height - 1);

            bool blocked = false;
            for (int x = rockX; x <= rockX + width; x++)
            {
                for (int y = rockY; y <= rockY + height; y++)
                {
                    if (tiles[x + (mapX * y)].TileType == TileTypes.Rock
                        || tiles[x + (mapX * y)].TileType == TileTypes.Store
                        || tiles[x + (mapX * y)].TileType == TileTypes.Tilled)
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

                var health = UnityEngine.Random.Range(50, 250);
                EntityManager.SetComponentData(rockEntity, new HealthData()
                {
                    CurrentValue = health,
                    MaxValue = health
                });
                EntityManager.AddComponentData(rockEntity, new NonUniformScale()
                {
                    Value = new float3(scaleWidth - 0.5f, Rock.GetHeightFromHealth(health, health), scaleHeight - 0.5f)
                });
                EntityManager.SetComponentData(rockEntity, new Translation()
                {
                    Value = new float3((float)rockX - 0.5f + ((float)scaleWidth / 2.0f), 0, (float)rockY - 0.5f + ((float)scaleHeight / 2.0f))
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

        int spawnedFarmer = 0;
        while(spawnedFarmer < farm.InitialFarmerCount)
        {
            int x = UnityEngine.Random.Range(0, mapX);
            int y = UnityEngine.Random.Range(0, mapY);

            // Avoid spawning into rocks
            if(tiles[x + y * mapX].TileType == TileTypes.Rock)
            {
                continue;
            }

            Entity farmerEntity = EntityManager.Instantiate(farm.FarmerEntity);
            EntityManager.SetComponentData(farmerEntity, new Translation()
            {
                Value = new float3(x, 0, y)
            });
            EntityManager.SetComponentData(farmerEntity, new LogicalPosition { PositionX = x, PositionY = y });

            spawnedFarmer++;
        }

        EntityManager.RemoveComponent<FarmNeedGenerationTag>(m_QueryForFarmNeedingGeneration);
        return default;
    }

    public static TileTypes GetTileType(int x, int y)
    {
        return TileTypes.None;
    }
}
