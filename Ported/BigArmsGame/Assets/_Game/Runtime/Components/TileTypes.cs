using Unity.Entities;

public enum TileTypes
{
    None,
    Tilled,
    Rock,
    Store,
    Planted
}

public struct TileDescriptor
{
    public TileTypes TileType;
    public Entity Entity;
}