using UnityEngine;
using Unity.Entities;

public class TileLimitDrawer : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(FarmData));
        var farms = query.ToComponentDataArray<FarmData>(Unity.Collections.Allocator.TempJob);
        int x = farms[0].MapSize.x;
        int y = farms[0].MapSize.y;

        for(float i = -0.5f; i <= x; ++i)
        {
            Debug.DrawLine(new Vector3(i, 0.25f, -0.5f), new Vector3(i, 0.25f, y - 0.5f));
        }
        for (float j = -0.5f; j <= y; ++j)
        {
            Debug.DrawLine(new Vector3(-0.5f, 0.25f, j), new Vector3(x - 0.5f, 0.25f, j));
        }

        farms.Dispose();
        query.Dispose();
    }
}
