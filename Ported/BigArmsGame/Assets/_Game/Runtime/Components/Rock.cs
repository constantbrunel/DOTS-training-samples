using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Rock : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<HealthData>(entity);
        dstManager.AddComponent<LogicalPosition>(entity);
        dstManager.AddComponent<RockTag>(entity);
    }
}

public struct RockTag : IComponentData
{}
