using UnityEngine;
using Unity.Entities;

[ConverterVersion("ZBLAH", 2)]
public class Farmer : MonoBehaviour, IConvertGameObjectToEntity
{
    public BehaviourType BehaviourType;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<PathData>(entity);
        dstManager.AddComponent<FarmerTag>(entity);
        dstManager.AddComponentData(entity, new FarmerBehaviorData()
        {
            BehaviourType = BehaviourType
        });
        dstManager.AddComponent<TargetEntityData>(entity);
        dstManager.AddComponent<LogicalPosition>(entity);
    }
}
