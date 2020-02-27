using Unity.Entities;

public struct TileModifierData : IComponentData
{
    public int PosX;
    public int PosY;
    public TileTypes NextType;
}