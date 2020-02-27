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
            CurrentValue = Health,
            MaxValue = Health
        });
        dstManager.AddComponent<LogicalPosition>(entity);
    }

    public static float GetHeightFromHealth(float currentHealth, float maxHealth)
    {
        return currentHealth / maxHealth * maxHealth / 100;
    }
}
