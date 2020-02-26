using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class Rock : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Health;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new HealthData()
        { 
            Value = Health
        });
    }
}
