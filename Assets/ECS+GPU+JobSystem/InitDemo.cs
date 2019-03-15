using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
public class InitDemo : MonoBehaviour
{
    public static InitDemo m_instance;
    public GameObject swordPrefab;
    public int swordCout = 1000 * 10;


    public static int sphereSize = 40;
    public Mesh mesh;

    public static float G = -20f;
    public static float topY = 20f;
    public static float bottomY = -100f;


    // Use this for initialization
    void Start()
    {
        m_instance = this;
        EntityManager entityMgr = World.Active.GetOrCreateManager<EntityManager>();
        var entities = new NativeArray<Entity>(swordCout, Allocator.Temp);

        entityMgr.Instantiate(swordPrefab, entities);

        for (int i = 0; i < entities.Length; ++i)
        {
            Vector3 pos = UnityEngine.Random.insideUnitSphere * sphereSize;
            pos.y = topY;
            var entity = entities[i];
            entityMgr.SetComponentData(entity, new Translation { Value = pos });
            entityMgr.SetComponentData(entity, new Rotation { Value = quaternion.euler(new float3(1.67f, 0, 0), math.RotationOrder.XYZ) });
            entityMgr.SetComponentData(entity, new GravityComponentData { mass = Random.Range(0.5f, 3f), delay = Random.Range(0, 10f) });

            MeshCullingComponent cullComponent = new MeshCullingComponent();
            cullComponent.BoundingSphereCenter = mesh.bounds.center;
            cullComponent.BoundingSphereRadius = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z) * 0.5f;
            entityMgr.AddComponentData(entity, cullComponent);

            //MeshLODInactive inv = new MeshLODInactive();
            //entityMgr.AddComponentData(entity, inv);
        }

        entities.Dispose();
    }

}