using UnityEngine;
using Unity.Entities;

public class Farmer : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<PathData>(entity);
        dstManager.AddComponent<FarmerTag>(entity);
        dstManager.AddComponent<FarmerBehaviorData>(entity);
    }
}
