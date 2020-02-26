using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using System.Collections.Generic;

[ConverterVersion("bouilla", 6)]
public class Farm : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
	public Vector2Int MapSize;
	public int StoreCount;
	public int RockSpawnAttempts;

    public GameObject TileGameObject;
    public GameObject StoreGameObject;
    public GameObject RockGameObject;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new FarmData()
        {
            MapSize = new int2(MapSize.x, MapSize.y),
            StoreCount = StoreCount,
            RockSpawnAttempts = RockSpawnAttempts,
            TileEntity = conversionSystem.GetPrimaryEntity(TileGameObject),
            StoreEntity = conversionSystem.GetPrimaryEntity(StoreGameObject),
            RockEntity = conversionSystem.GetPrimaryEntity(RockGameObject)
        });
        dstManager.AddComponent<FarmNeedGenerationTag>(entity);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(TileGameObject);
        referencedPrefabs.Add(StoreGameObject);
        referencedPrefabs.Add(RockGameObject);
    }
}

public struct FarmData : IComponentData
{
    public int2 MapSize;
    public int StoreCount;
    public int RockSpawnAttempts;

    public Entity TileEntity;
    public Entity StoreEntity;
    public Entity RockEntity;
}

public struct FarmNeedGenerationTag : IComponentData{}