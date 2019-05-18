using System;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Rendering;

/// <summary>
/// 渲染Mesh，使用Indriect接口
/// </summary>
[Serializable]
public struct InstanceIndirectRenderer : ISharedComponentData, IEquatable<InstanceIndirectRenderer>
{
    public Mesh mesh;
    public Material material;
    public int subMesh;

    public ShadowCastingMode castShadows;
    public bool receiveShadows;

    public bool Equals(InstanceIndirectRenderer other)
    {
        if (mesh == other.mesh && material == other.material && castShadows == other.castShadows && receiveShadows == other.receiveShadows && subMesh == other.subMesh)
        {
            return true;
        }


        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

public class InstanceIndirectRendererComponent : SharedComponentDataProxy<InstanceIndirectRenderer> { }
