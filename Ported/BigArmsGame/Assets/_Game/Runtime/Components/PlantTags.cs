using Unity.Entities;


public struct IsGrowingTag : IComponentData { }

public struct IsHarvestableTag : IComponentData { }

public struct IsReservedTag : IComponentData { }

public struct IsSoldData : IComponentData
{
    public int StoreX;
    public int StoreY;
}
