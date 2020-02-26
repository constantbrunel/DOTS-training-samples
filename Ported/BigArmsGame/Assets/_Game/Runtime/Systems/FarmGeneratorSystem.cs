using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

public class FarmGeneratorSystem : JobComponentSystem
{
    EntityQuery queryForFarmNeedingGeneration;

    public NativeArray<TileTypes> tiles;

    protected override void OnCreate()
    {
        base.OnCreate();
        queryForFarmNeedingGeneration = GetEntityQuery(typeof(FarmData), typeof(FarmNeedGenerationTag));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        // Build farm bitches

        return default;
    }

    public static TileTypes GetTileType(int x, int y)
    {
        return TileTypes.None;
    }
}
