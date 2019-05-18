using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Rendering;


/// <summary>
/// Render Mesh with Material (must be instanced material) by object to world matrix.
/// Specified by TransformMatrix associated with Entity.
/// </summary>
[Serializable]
public struct InstanceRenderer : ISharedComponentData,IEquatable<InstanceRenderer>
{
    public Mesh                 mesh;
    public Material             material;
	public int                  subMesh;

    public ShadowCastingMode    castShadows;
    public bool                 receiveShadows;

    public bool Equals(InstanceRenderer other)
    {
        if(mesh == other.mesh && material == other.material && castShadows == other.castShadows && receiveShadows == other.receiveShadows && subMesh == other.subMesh)
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

public class InstanceRendererComponent : SharedComponentDataProxy<InstanceRenderer> { }

