using Unity.Entities;

public struct ReservationData : IComponentData
{
    public Entity FarmerEntity;
    public Entity PlantEntity;
}