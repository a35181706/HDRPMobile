using System;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Rendering;

/// <summary>
/// Render Mesh with Material (must be instanced material) by object to world matrix.
/// Specified by TransformMatrix associated with Entity.
/// </summary>
[Serializable]
public struct InstanceIndirectRenderer : ISharedComponentData
{
    public Mesh mesh;
    public Material material;
    public int subMesh;

    public ShadowCastingMode castShadows;
    public bool receiveShadows;
}

public class InstanceIndirectRendererComponent : SharedComponentDataProxy<InstanceIndirectRenderer> { }
