using System;
using UnityEngine;
using Unity.Entities;
using UnityEngine.Rendering;

/// <summary>
/// 渲染Mesh，使用Indriect接口
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
