using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Experimental.PlayerLoop;


/// <summary>
/// Renders all Entities containing both MeshInstanceRenderer & TransformMatrix components.
/// 自己实现的基于DrawMeshInstanceIndirect渲染的ECS版本
/// </summary>
[UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
[UpdateAfter(typeof(MeshCullingBarrier))]
[UnityEngine.ExecuteInEditMode]
public class MeshInstanceIndirectRenderSystem : ComponentSystem
{

    List<MeshInstanceIndirectRenderer> m_CacheduniqueRendererTypes = new List<MeshInstanceIndirectRenderer>(10);
    ComponentGroup m_InstanceRendererGroup;

    private ComputeBuffer argsBuffer;
    private ComputeBuffer transformBuffer;
    //drawindrect是写死的5个，unity说的
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    Matrix4x4[] matrixBuffer;
    bool bInit = false;

    // This is the ugly bit, necessary until Graphics.DrawMeshInstanced supports NativeArrays pulling the data in from a job.
    public unsafe static void CopyMatrices(ComponentDataArray<TransformMatrix> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
    {
        // @TODO: This is using unsafe code because the Unity DrawInstances API takes a Matrix4x4[] instead of NativeArray.
        // We want to use the ComponentDataArray.CopyTo method
        // because internally it uses memcpy to copy the data,
        // if the nativeslice layout matches the layout of the component data. It's very fast...
        fixed (Matrix4x4* matricesPtr = outMatrices)
        {
            Assert.AreEqual(sizeof(Matrix4x4), sizeof(TransformMatrix));
            var matricesSlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<TransformMatrix>(matricesPtr, sizeof(Matrix4x4), length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref matricesSlice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
            transforms.CopyTo(matricesSlice, beginIndex);
        }
    }
    protected override void OnCreateManager(int capacity)
    {

        // We want to find all MeshInstanceIndirectRenderer & TransformMatrix combinations and render them
        m_InstanceRendererGroup = GetComponentGroup(typeof(MeshInstanceIndirectRenderer),
                                            typeof(TransformMatrix),
                                            ComponentType.Subtractive<MeshCulledComponent>());

    
    }

    void InitBuffers()
    {
        if (bInit || !InitDemo.m_instance)
        {
            return;
        }
        bInit = true;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4));
        matrixBuffer = new Matrix4x4[InitDemo.m_instance.swordCout];
        transformBuffer = new ComputeBuffer(InitDemo.m_instance.swordCout, size);
    }

    protected override void OnUpdate()
    {
        InitBuffers();

        // We want to iterate over all unique MeshInstanceRenderer shared component data,
        // that are attached to any entities in the world
        EntityManager.GetAllUniqueSharedComponentDatas(m_CacheduniqueRendererTypes);

        var forEachFilter = m_InstanceRendererGroup.CreateForEachFilter(m_CacheduniqueRendererTypes);

        MeshInstanceIndirectRenderer render = default(MeshInstanceIndirectRenderer);

        for (int i = 0; i != m_CacheduniqueRendererTypes.Count; i++)
        {
            var renderer = m_CacheduniqueRendererTypes[i];

            if (renderer.mesh)
            {
                render = renderer;
                break;
            }
        }

        var transforms = m_InstanceRendererGroup.GetComponentDataArray<TransformMatrix>(forEachFilter,1);
        CopyMatrices(transforms, 0, transforms.Length, matrixBuffer);


        if (render.mesh)
        {
            args[0] = (uint)render.mesh.GetIndexCount(render.subMesh);
            args[1] = (uint)transforms.Length;
            args[2] = (uint)render.mesh.GetIndexStart(render.subMesh);
            args[3] = (uint)render.mesh.GetBaseVertex(render.subMesh);
            
            transformBuffer.SetData(matrixBuffer);
            argsBuffer.SetData(args);
            render.material.SetBuffer("transformBuffer", transformBuffer);
            Graphics.DrawMeshInstancedIndirect(render.mesh, render.subMesh, render.material,
                new Bounds(Vector3.zero, Vector3.one * 999), argsBuffer);

            
        }
        else
        {
            args[0] = args[1] = args[2] = args[3] = 0;
        }

        

        m_CacheduniqueRendererTypes.Clear();
        forEachFilter.Dispose();
    }

    protected override void OnDestroyManager()
    {
        if (transformBuffer != null)
        {
            transformBuffer.Release();
            transformBuffer = null;
            argsBuffer.Release();
            argsBuffer = null;
        }
    }
}