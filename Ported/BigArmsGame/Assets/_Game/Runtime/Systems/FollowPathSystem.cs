using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class FollowPathSystem : JobComponentSystem
{
	private const float m_Walkspeed = 4f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
		var tiles = World.GetExistingSystem<FarmGeneratorSystem>().tiles;
		var map = GetSingleton<FarmData>();

		var deltaTime = Time.DeltaTime;

		return Entities
			.ForEach((ref Translation trans, ref DynamicBuffer<PathData> path, in FarmerBehaviorData behavior) =>
        {
			if (path.Length > 0)
			{
				int2 nextTile = path[path.Length - 1].Position;
				if (behavior.PositionX == nextTile.x && behavior.PositionY == nextTile.y)
				{
					path.RemoveAt(path.Length - 1);
				}
				else
				{
					if (Pathing.IsBlocked(tiles, map.MapSize.x, map.MapSize.y, nextTile.x, nextTile.y) == false)
					{
						float offset = .5f;
						if (Pathing.IsPlant(tiles, map.MapSize.x, nextTile.x, nextTile.y))
						{
							offset = .01f;
						}
						Vector2 targetPos = new Vector2(nextTile.x + offset, nextTile.y + offset);
						Vector2 position = new Vector2(trans.Value.x, trans.Value.z);
						position = Vector2.MoveTowards(position, targetPos, m_Walkspeed * deltaTime);
						trans.Value.x = position.x;
						trans.Value.z = position.y;
					}
				}
			}
		}).Schedule(inputDeps);
    }
}