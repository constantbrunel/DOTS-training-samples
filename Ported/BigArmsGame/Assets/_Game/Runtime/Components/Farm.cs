using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

[ConverterVersion("bouilla", 8)]
public class Farm : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public Vector2Int MapSize;
	public int StoreCount;
	public int RockSpawnAttempts;
    public int InitialFarmerCount;

    public GameObject TileGameObject;
    public GameObject StoreGameObject;
    public GameObject RockGameObject;
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
            TileEntity = conversionSystem.GetPrimaryEntity(TileGameObject),
            StoreEntity = conversionSystem.GetPrimaryEntity(StoreGameObject),
            RockEntity = conversionSystem.GetPrimaryEntity(RockGameObject),
            FarmerEntity = conversionSystem.GetPrimaryEntity(FarmerGameObject),
            DroneEntity = conversionSystem.GetPrimaryEntity(DroneGameObject)
        });
        dstManager.AddComponent<FarmNeedGenerationTag>(entity);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(TileGameObject);
        referencedPrefabs.Add(StoreGameObject);
        referencedPrefabs.Add(RockGameObject);
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

    public Entity TileEntity;
    public Entity StoreEntity;
    public Entity RockEntity;
    public Entity FarmerEntity;
    public Entity DroneEntity;
}

public struct FarmNeedGenerationTag : IComponentData{}