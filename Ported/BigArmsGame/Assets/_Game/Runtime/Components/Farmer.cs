using UnityEngine;
using Unity.Entities;

[ConverterVersion("ZBLAH", 1)]
public class Farmer : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<PathData>(entity);
        dstManager.AddComponent<FarmerTag>(entity);
        dstManager.AddComponent<FarmerBehaviorData>(entity);
        dstManager.AddComponent<TargetEntityData>(entity);
    }
}
