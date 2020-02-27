using Unity.Entities;

// Used to grow plants and destroy rocks
public struct HealthData : IComponentData
{
    public float MaxValue;
    public float CurrentValue;
}