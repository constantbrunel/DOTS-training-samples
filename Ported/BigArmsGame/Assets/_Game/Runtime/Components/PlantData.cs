using Unity.Entities;
using UnityEngine;

public class PlantData : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PlantDataComp { GrowDuration = Random.Range(2, 10) });
        dstManager.AddComponent<IsGrowingTag>(entity);
        dstManager.AddComponent<LogicalPosition>(entity);
    }
}

public struct PlantDataComp : IComponentData
{
    public float Growth;
    public int GrowDuration;
}