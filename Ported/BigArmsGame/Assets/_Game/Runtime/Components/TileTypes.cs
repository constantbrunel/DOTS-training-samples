using Unity.Entities;

public enum TileTypes
{
    None,
    Tilled,
    Rock,
    Store,
    Planted,
    Harvestable,
    Reserved,
}

public struct TileDescriptor
{
    public TileTypes TileType;
    public Entity Entity;
}