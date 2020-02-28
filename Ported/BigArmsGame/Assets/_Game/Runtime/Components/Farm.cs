using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

[ConverterVersion("bouilla", 1)]
public class Farm : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public Vector2Int MapSize;
	public int StoreCount;
	public int RockSpawnAttempts;
    public int InitialFarmerCount;
    public int InitialTilledTileCount;

    public GameObject DefaultTileGameObject;
    public GameObject TiledTileGameObject;
    public GameObject StoreGameObject;
    public GameObject RockGameObject;
    public GameObject PlantGameObject1;
    public GameObject FarmerGameObject;
    public GameObject DroneGameObject;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new FarmData()
        {
            MapSize = new int2(MapSize.x, MapSize.y),
            StoreCount = StoreCount,
            RockSpawnAttempts = RockSpawnAttempts,
            InitialFarmerCount = InitialFarmerCount,
            InitialTilledTileCount = InitialTilledTileCount,
            DefaultTileEntity = conversionSystem.GetPrimaryEntity(DefaultTileGameObject),
            TiledTileEntity = conversionSystem.GetPrimaryEntity(TiledTileGameObject),
            StoreEntity = conversionSystem.GetPrimaryEntity(StoreGameObject),
            RockEntity = conversionSystem.GetPrimaryEntity(RockGameObject),
            PlantEntity1 = conversionSystem.GetPrimaryEntity(PlantGameObject1),
            FarmerEntity = conversionSystem.GetPrimaryEntity(FarmerGameObject),
            DroneEntity = conversionSystem.GetPrimaryEntity(DroneGameObject)
        });
        dstManager.AddComponent<FarmNeedGenerationTag>(entity);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(DefaultTileGameObject);
        referencedPrefabs.Add(TiledTileGameObject);
        referencedPrefabs.Add(StoreGameObject);
        referencedPrefabs.Add(RockGameObject);
        referencedPrefabs.Add(PlantGameObject1);
        referencedPrefabs.Add(FarmerGameObject);
        referencedPrefabs.Add(DroneGameObject);
    }
}

public struct FarmData : IComponentData
{
    public int2 MapSize;
    public int StoreCount;
    public int RockSpawnAttempts;
    public int InitialFarmerCount;
    public int InitialTilledTileCount;

    public Entity DefaultTileEntity;
    public Entity TiledTileEntity;
    public Entity StoreEntity;
    public Entity RockEntity;
    public Entity PlantEntity1;
    public Entity FarmerEntity;
    public Entity DroneEntity;
}

public struct FarmNeedGenerationTag : IComponentData{}