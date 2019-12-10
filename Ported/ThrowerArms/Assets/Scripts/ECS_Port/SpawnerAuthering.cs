﻿using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
public struct SpawnerComponent : IComponentData
{
    public float3 center;
    public float3 extend;
    public Entity spawnEntity;
    public float frequency;
}

public class SpawnerAuthering : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject spawnPrefab;
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var spawnEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(spawnPrefab, settings);
        dstManager.AddComponentData(entity, new SpawnerComponent() { center = transform.position, extend = transform.localScale, frequency = 0.5f, spawnEntity =spawnEntity });
    }
}