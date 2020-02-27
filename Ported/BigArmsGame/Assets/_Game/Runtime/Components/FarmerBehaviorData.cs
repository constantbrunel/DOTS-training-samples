﻿using Unity.Entities;

public enum FarmerBehavior
{
    None,
    SmashRock,
    TillGround,
    PlantSeed,
    SellPlant,
}

public struct FarmerBehaviorData : IComponentData
{
    public FarmerBehavior Value;
    public int PositionX;
    public int PositionY;
}