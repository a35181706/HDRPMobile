using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
public class LegacySystem : MonoBehaviour {

    public int swordCout = 1000 * 10;
    public static int sphereSize = 40;

    public Mesh mesh;
    public Material mat;
    // Use this for initialization
    void Start () {

        var rootGo = new GameObject("Balls");
        rootGo.transform.position = Vector3.zero;

        for (int i = 0; i < swordCout; ++i)
        {
            var go = new GameObject();
            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            var meshRd = go.AddComponent<MeshRenderer>();
            meshRd.sharedMaterial = mat;
            meshRd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRd.receiveShadows = false;
            var dropComponent = go.AddComponent<LagacyDrop>();
            dropComponent.delay = UnityEngine.Random.Range(0, 10f);
            dropComponent.mass = UnityEngine.Random.Range(0.5f, 3f);

            Vector3 pos = UnityEngine.Random.insideUnitSphere * 40;
            go.transform.parent = rootGo.transform;
            pos.y = InitDemo.topY;
            go.transform.position = pos;
            go.transform.eulerAngles = new Vector3(90, 0, 0);
        }
    }
	
}
