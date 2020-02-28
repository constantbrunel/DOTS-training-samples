using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class TileModifierSystem : JobComponentSystem
{
    protected override void OnCreate()
    {
        base.OnCreate();

        RequireForUpdate(GetEntityQuery(typeof(TileModifierData)));
        RequireSingletonForUpdate<FarmData>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var tiles = World.GetExistingSystem<FarmGeneratorSystem>().tiles;
        var mapSize = World.GetExistingSystem<FarmGeneratorSystem>().MapSize;

        FarmData farmData = GetSingleton<FarmData>();
        var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 9999));

        // Debug.Log("Print map before");
        // PrintMapUtils.PrintMap(tiles, mapSize.x, mapSize.y);

        Entities.ForEach((in TileModifierData modifierData) =>
        {
            // UnityEngine.Debug.Log($"TileModifier ({modifierData.PosX},{modifierData.PosY} - new type {modifierData.NextType})");
            var tile = tiles[Pathing.Hash(mapSize.x, modifierData.PosX, modifierData.PosY)];
            switch (tile.TileType)
            {
                case TileTypes.Rock:
                    {
                        var translation = GetComponentDataFromEntity<Translation>(true)[tile.Entity];
                        var scale = GetComponentDataFromEntity<NonUniformScale>(true)[tile.Entity];
                        int2 position = new int2((int)(translation.Value.x + 0.5f - (scale.Value.x) / 2f), (int)(translation.Value.z + 0.5f - (scale.Value.z) / 2f));
                        int sizeX = (int)scale.Value.x + 1;
                        int sizeY = (int)scale.Value.z + 1;
                        for (int i = 0; i < sizeX; ++i)
                        {
                            for (int j = 0; j < sizeY; ++j)
                            {
                                tiles[Pathing.Hash(mapSize.x, position.x + i, position.y + j)] = new TileDescriptor() { TileType = modifierData.NextType, Entity = Entity.Null };
                            }
                        }

                        EntityManager.DestroyEntity(tile.Entity);
                        break;
                    }
                case TileTypes.Planted:
                    {
                        tiles[Pathing.Hash(mapSize.x, modifierData.PosX, modifierData.PosY)] = new TileDescriptor() { TileType = modifierData.NextType, Entity = tile.Entity };
                        break;
                    }
                case TileTypes.None:
                    {
                        Entity tilledTile = EntityManager.Instantiate(farmData.TiledTileEntity);
                        EntityManager.SetComponentData(tilledTile, new Translation() { Value = new float3(modifierData.PosX, 0.001f, modifierData.PosY) });
                        tiles[Pathing.Hash(mapSize.x, modifierData.PosX, modifierData.PosY)] = new TileDescriptor() { TileType = modifierData.NextType, Entity = Entity.Null };
                        break;
                    }
                case TileTypes.Harvestable:
                    {
                        tiles[Pathing.Hash(mapSize.x, modifierData.PosX, modifierData.PosY)] = new TileDescriptor() { TileType = modifierData.NextType, Entity = Entity.Null };
                        break;
                    }
                case TileTypes.Tilled:
                    {
                        Entity plantEntity;

                        var rand = random.NextInt(0, 3);
                        if (rand == 0)
                        {
                            plantEntity = EntityManager.Instantiate(farmData.PlantEntity1);
                        }
                        else if(rand == 1)
                        {
                            plantEntity = EntityManager.Instantiate(farmData.PlantEntity2);
                        }
                        else
                        {
                            plantEntity = EntityManager.Instantiate(farmData.PlantEntity3);
                        }
                        EntityManager.SetComponentData(plantEntity, new Translation() { Value = new float3(modifierData.PosX, 0f, modifierData.PosY) });
                        tiles[Pathing.Hash(mapSize.x, modifierData.PosX, modifierData.PosY)] = new TileDescriptor() { TileType = modifierData.NextType, Entity = plantEntity };
                        break;
                    }
            }
        }).WithStructuralChanges().WithoutBurst().Run();

        // Debug.Log("Print map after");
        // PrintMapUtils.PrintMap(World.GetExistingSystem<FarmGeneratorSystem>().tiles, mapSize.x, mapSize.y);

        EntityManager.DestroyEntity(GetEntityQuery(typeof(TileModifierData)));
        return inputDeps;
    }
}
