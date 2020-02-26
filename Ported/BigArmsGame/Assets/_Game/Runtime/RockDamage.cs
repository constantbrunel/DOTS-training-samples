using Unity.Entities;
using UnityEngine;


public struct RockDamage : IComponentData
{
    float Damage;
    public Entity Source;
    public Entity Target;
}

