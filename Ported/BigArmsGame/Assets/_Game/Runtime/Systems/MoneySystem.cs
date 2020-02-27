using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class MoneySystem : JobComponentSystem
{
    private EntityQuery m_IsSoldQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_IsSoldQuery = GetEntityQuery(typeof(IsSoldData));
        RequireSingletonForUpdate<MoneyData>();

        var bankEntity = EntityManager.CreateEntity();
        EntityManager.AddComponentData(bankEntity, new MoneyData { FarmerBank = 0, DroneBank = 0, FarmerCost = 5, DroneCost = 20 });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        MoneyData bank = GetSingleton<MoneyData>();
        var farm = GetSingleton<FarmData>();

        int soldCount = m_IsSoldQuery.CalculateEntityCount();

        Entities.ForEach((Entity entity, int entityInQueryIndex, IsSoldData data) =>
        {
            bank.FarmerBank++;
            bank.DroneBank++;

            if (bank.FarmerBank >= bank.FarmerCost)
            {
                Spawn(EntityManager, farm.FarmerEntity, data.StoreX, data.StoreY);
                UnityEngine.Debug.Log("Farmer Spawned");
                bank.FarmerBank -= bank.FarmerCost;
            }

            if (bank.DroneBank >= bank.DroneCost)
            {
                Spawn(EntityManager, farm.DroneEntity, data.StoreX, data.StoreY);
                UnityEngine.Debug.Log("Drone Spawned");
                bank.DroneBank -= bank.DroneCost;
            }
        }).WithStructuralChanges().Run();

        SetSingleton(bank);

        // TODO - Add a nice animation to fadeout plant sold
        EntityManager.DestroyEntity(m_IsSoldQuery);

        return default;
    }

    private void Spawn(EntityManager dstManager, Entity prefab, int posX = 0, int posY = 0)
    {
        Entity farmerEntity = dstManager.Instantiate(prefab);
        EntityManager.SetComponentData(farmerEntity, new Translation()
        {
            Value = new float3(posX, 0, posY)
        });
    }
}