using Unity.Entities;

public struct Damage : IComponentData
{
    public float Value;
    public Entity Source;
    public Entity Target;
}
