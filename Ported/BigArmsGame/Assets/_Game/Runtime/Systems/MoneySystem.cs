using Unity.Entities;
using Unity.Jobs;

public class MoneySystem : JobComponentSystem
{
    private EntityQuery m_IsSoldQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_IsSoldQuery = GetEntityQuery(typeof(IsSoldTag));
        RequireSingletonForUpdate<MoneyData>();

        var bankEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(bankEntity, new MoneyData { FarmerBank = 0, DroneBank = 0, FarmerCost = 5, DroneCost = 20 });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MoneyData bank = GetSingleton<MoneyData>();

        int soldCount = m_IsSoldQuery.CalculateEntityCount();

        bank.FarmerBank += soldCount;
        bank.DroneBank += soldCount;

        while (bank.FarmerBank >= bank.FarmerCost)
        {
            SpawnFarmer();
            bank.FarmerBank -= bank.FarmerCost;
        }

        while (bank.DroneBank >= bank.DroneCost)
        {
            SpawnDrone();
            bank.DroneBank -= bank.DroneCost;
        }

        SetSingleton(bank);

        // TODO - Add a nice animation to fadeout plant sold
        EntityManager.DestroyEntity(m_IsSoldQuery);

        return default;
    }

    private void SpawnFarmer()
    {
        // TODO - Spawn farmer
        UnityEngine.Debug.Log("Farmer Spawned");
    }

    private void SpawnDrone()
    {
        // TODO - Spawn Drone
        UnityEngine.Debug.Log("Drone Spawned");
    }
}