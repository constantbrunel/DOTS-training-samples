using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class TileModifierSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var tiles = World.GetExistingSystem<FarmGeneratorSystem>().tiles;
        var map = GetSingleton<FarmData>();

        Entities.ForEach((in TileModifierData modifierData) =>
        {
            var tile = tiles[Pathing.Hash(map.MapSize.x, modifierData.PosX, modifierData.PosY)];
            switch (tile.TileType)
            {
                case TileTypes.Rock:
                    {
                        var translation = GetComponentDataFromEntity<Translation>(true)[tile.Entity];
                        var scale = GetComponentDataFromEntity<NonUniformScale>(true)[tile.Entity];
                        int2 position = new int2((int)(translation.Value.x - (scale.Value.x + 1)/ 2f), (int)(translation.Value.z - (scale.Value.z + 1)/ 2f));
                        int sizeX = (int)scale.Value.x + 1;
                        int sizeY = (int)scale.Value.z + 1;
                        for(int i = 0; i < sizeX; ++i)
                        {
                            for(int j = 0; j < sizeY; ++j)
                            {
                                tiles[Pathing.Hash(map.MapSize.x, position.x + i, position.y + j)] = new TileDescriptor() { TileType = modifierData.NextType, Entity = Entity.Null };
                            }
                        }

                        EntityManager.DestroyEntity(tile.Entity);
                        break;
                    }
            }
        }).WithStructuralChanges().WithoutBurst().Run();

        EntityManager.DestroyEntity(GetEntityQuery(typeof(TileModifierData)));

        return default;
    }
}
