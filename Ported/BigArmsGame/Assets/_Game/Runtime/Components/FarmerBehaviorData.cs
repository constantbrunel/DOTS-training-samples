using Unity.Entities;

public enum FarmerBehavior
{
    None,
    SmashRock,
    TillGround,
    PlantSeed,
    SellPlant
}

public enum BehaviourType
{
    Farmer,
    Drone
}

public struct FarmerBehaviorData : IComponentData
{
    public BehaviourType BehaviourType;
    public FarmerBehavior Value;
    public Entity HeldPlant;
    public bool HasBoughtSeeds;
}
