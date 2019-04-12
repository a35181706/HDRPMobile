using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
public class InitDemo : MonoBehaviour
{
    public static InitDemo m_instance;
    public GameObject []swordPrefabs;
    public int earchSwordCout = 1000 * 5;


    public static int sphereSize = 40;
    public Mesh mesh;

    public static float G = -20f;
    public static float topY = 20f;
    public static float bottomY = -100f;

    public Material LitMaterial;
    // Use this for initialization
    void Start()
    {
        m_instance = this;
        EntityManager entityMgr = World.Active.GetOrCreateManager<EntityManager>();

        foreach (GameObject obj in swordPrefabs)
        {
            var entities = new NativeArray<Entity>(earchSwordCout, Allocator.Temp);

            entityMgr.Instantiate(obj, entities);

            for (int i = 0; i < entities.Length; ++i)
            {
                Vector3 pos = UnityEngine.Random.insideUnitSphere * sphereSize;
                pos.y = topY;
                var entity = entities[i];
                entityMgr.SetComponentData(entity, new Translation { Value = pos });
                entityMgr.SetComponentData(entity, new Rotation { Value = quaternion.Euler(new float3(1.67f, 0, 0), math.RotationOrder.XYZ) });
                entityMgr.SetComponentData(entity, new GravityComponentData { mass = UnityEngine.Random.Range(0.5f, 3f), delay = UnityEngine.Random.Range(0, 10f) });

                FurstumCullingComponent cullComponent = new FurstumCullingComponent();
                cullComponent.BoundingSphereCenter = mesh.bounds.center;
                cullComponent.BoundingSphereRadius = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z) * 0.5f;
                entityMgr.AddComponentData(entity, cullComponent);

            }
            
            entities.Dispose();
        }
    }
}