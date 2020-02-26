using Unity.Entities;

public struct MoneyData : IComponentData
{
    public int FarmerBank;
    public int DroneBank;
    public int FarmerCost;
    public int DroneCost;
}