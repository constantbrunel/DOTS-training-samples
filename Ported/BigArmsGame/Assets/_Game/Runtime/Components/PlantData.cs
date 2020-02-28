using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlantData : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PlantDataComp { GrowDuration = UnityEngine.Random.Range(8, 10) });
        dstManager.AddComponent<IsGrowingTag>(entity);
        dstManager.AddComponent<LogicalPosition>(entity);
        dstManager.AddComponentData(entity, new Scale()
        {
            Value = 0.1f
        }); ;
    }
}

public struct PlantDataComp : IComponentData
{
    public float Growth;
    public int GrowDuration;
}