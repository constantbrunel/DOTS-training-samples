using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class CameraFollow : MonoBehaviour
{
	public Vector2 ViewAngles;
	public float ViewDist;
	public float MouseSensitivity;

    private EntityQuery m_FarmerQuery;

	void Start()
	{
		transform.rotation = Quaternion.Euler(ViewAngles.y, ViewAngles.x, 0f);

		m_FarmerQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(FarmerTag), typeof(Translation));
	}

	void LateUpdate()
	{
        if(!m_FarmerQuery.IsEmptyIgnoreFilter)
        {
            var farmers = m_FarmerQuery.ToComponentDataArray<Translation>(Unity.Collections.Allocator.TempJob);
			Vector3 pos = farmers[0].Value;
		    ViewAngles.x += Input.GetAxis("Mouse X") * MouseSensitivity / Screen.height;
		    ViewAngles.y -= Input.GetAxis("Mouse Y") * MouseSensitivity / Screen.height;
		    ViewAngles.y = Mathf.Clamp(ViewAngles.y, 7f, 80f);
		    ViewAngles.x -= Mathf.Floor(ViewAngles.x / 360f) * 360f;
		    transform.rotation = Quaternion.Euler(ViewAngles.y, ViewAngles.x, 0f);
		    transform.position = pos - transform.forward * ViewDist;
			farmers.Dispose();
		}
	}
}
    