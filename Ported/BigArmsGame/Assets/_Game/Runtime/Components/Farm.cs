using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class Farm : MonoBehaviour, IConvertGameObjectToEntity
{
	public Vector2Int MapSize;
	public int StoreCount;
	public int RockSpawnAttempts;

	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new FarmData()
        {
            MapSize = new int2(MapSize.x, MapSize.y),
            StoreCount = StoreCount,
            RockSpawnAttempts = RockSpawnAttempts
        });
        dstManager.AddComponent<FarmNeedGenerationTag>(entity);
    }
}

public struct FarmData : IComponentData
{
    public int2 MapSize;
    public int StoreCount;
    public int RockSpawnAttempts;
}

public struct FarmNeedGenerationTag : IComponentData{}