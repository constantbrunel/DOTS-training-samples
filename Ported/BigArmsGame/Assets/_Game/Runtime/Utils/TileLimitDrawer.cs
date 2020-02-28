using UnityEngine;
using Unity.Entities;

public class TileLimitDrawer : MonoBehaviour
{
    public void OnDrawGizmos()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        var query = entityManager.CreateEntityQuery(typeof(FarmData));
        if(query.IsEmptyIgnoreFilter)
        {
            return;
        }

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

        // Draw tilling zone
        var queryTilling = entityManager.CreateEntityQuery(typeof(TillTargetData));
        if (!queryTilling.IsEmptyIgnoreFilter)
        {
            var tillZones = queryTilling.ToComponentDataArray<TillTargetData>(Unity.Collections.Allocator.TempJob);
            foreach (var tillZone in tillZones)
            {
                Vector3 botLeft = new Vector3(tillZone.PosX, 0.1f, tillZone.PosY);
                Vector3 botRight = new Vector3(tillZone.PosX + tillZone.SizeX, 0.1f, tillZone.PosY);
                Vector3 topLeft = new Vector3(tillZone.PosX, 0.1f, tillZone.PosY + tillZone.SizeY);
                Vector3 topRight = new Vector3(tillZone.PosX + tillZone.SizeX, 0.1f, tillZone.PosY + tillZone.SizeY);
                // Left
                Debug.DrawLine(botLeft, topLeft, new Color(0, 1, 0));
                // Up
                Debug.DrawLine(topLeft, topRight, new Color(0, 1, 0));
                // Right
                Debug.DrawLine(topRight, botRight, new Color(0, 1, 0));
                // Bottom
                Debug.DrawLine(botRight, botLeft, new Color(0, 1, 0));
            }
            tillZones.Dispose();
        }

        // Print path for farmers
        var queryFarmers = entityManager.CreateEntityQuery(typeof(PathData));
        if(!queryFarmers.IsEmptyIgnoreFilter)
        {
            var pathDatas = queryFarmers.ToEntityArray(Unity.Collections.Allocator.TempJob);
            foreach (var pathDataEntity in pathDatas)
            {
                var pathDataBuffer = entityManager.GetBuffer<PathData>(pathDataEntity);

                if(pathDataBuffer.Length == 0)
                {
                    continue;
                }

                var posX = pathDataBuffer[0].Position.x;
                var posY = pathDataBuffer[0].Position.y;

                for(int i = 1; i < pathDataBuffer.Length; ++i)
                {
                    var destX = pathDataBuffer[i].Position.x;
                    var destY = pathDataBuffer[i].Position.y;

                    Vector3 from = new Vector3(posX, 0.1f, posY);
                    Vector3 to = new Vector3(destX, 0.1f, destY);

                    Debug.DrawLine(from, to, new Color(1, 0, 0));

                    posX = destX;
                    posY = destY;
                }
            }
            pathDatas.Dispose();
        }

        queryFarmers.Dispose();
        queryTilling.Dispose();

        farms.Dispose();
        query.Dispose();
    }
}
