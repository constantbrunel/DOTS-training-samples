using Unity.Entities;
using Unity.Mathematics;

// Used for Farmer pathing
public struct PathData : IBufferElementData
{
    public int2 Position;
}