using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;

using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;
using UnityEngine.Experimental.PlayerLoop;


/// <summary>
/// Renders all Entities containing both MeshInstanceRenderer & TransformMatrix components.
/// 自己实现的基于DrawMeshInstanceIndirect渲染的ECS版本
/// </summary>

[UpdateAfter(typeof(FrustumCullingBarrier))]
public class InstanceIndirectRenderSystem : ComponentSystem
{
    List<InstanceIndirectRenderer> m_CacheduniqueRendererTypes = new List<InstanceIndirectRenderer>(10);
    ComponentGroup m_InstanceRendererGroup;

    private ComputeBuffer argsBuffer;
    private ComputeBuffer []transformBuffers = null;
    //drawindrect是写死的5个，unity说的
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    Matrix4x4[] matrixBuffer;
    bool bInit = false;

    // This is the ugly bit, necessary until Graphics.DrawMeshInstanced supports NativeArrays pulling the data in from a job.
    public unsafe static void CopyMatrices(ComponentDataArray<LocalToWorld> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
    {
        // @TODO: This is using unsafe code because the Unity DrawInstances API takes a Matrix4x4[] instead of NativeArray.
        // We want to use the ComponentDataArray.CopyTo method
        // because internally it uses memcpy to copy the data,
        // if the nativeslice layout matches the layout of the component data. It's very fast...
        fixed (Matrix4x4* matricesPtr = outMatrices)
        {
            Assert.AreEqual(sizeof(Matrix4x4), sizeof(LocalToWorld));
            var matricesSlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<LocalToWorld>(matricesPtr, sizeof(Matrix4x4), length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref matricesSlice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
            transforms.CopyTo(matricesSlice, beginIndex);
        }

        
    }

    protected override void OnCreateManager()
    {
        m_InstanceRendererGroup = GetComponentGroup(ComponentType.ReadOnly<InstanceIndirectRenderer>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.Exclude<FurstumCulledComponent>());    
    }
    UnityEngine.Rendering.CommandBuffer cmdBuffer;
    void InitBuffers()
    {
        if (bInit || !InitDemo.m_instance)
        {
            return;
        }
        bInit = true;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4));
        matrixBuffer = new Matrix4x4[InitDemo.m_instance.earchSwordCout];
        transformBuffers = new ComputeBuffer[InitDemo.m_instance.swordPrefabs.Length];

        for (int i = 0;i < transformBuffers.Length;i++)
        {
            transformBuffers[i] = new ComputeBuffer(InitDemo.m_instance.earchSwordCout, size);
        }

        cmdBuffer = new UnityEngine.Rendering.CommandBuffer();

        if (Camera.main.actualRenderingPath == RenderingPath.Forward)
        {
            Camera.main.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterForwardOpaque, cmdBuffer);
        }
        else if (Camera.main.actualRenderingPath == RenderingPath.DeferredShading)
        {
            Camera.main.AddCommandBuffer(UnityEngine.Rendering.CameraEvent.AfterGBuffer, cmdBuffer);
        }

    }



    protected override void OnUpdate()
    {
        //InitBuffers();

        //EntityManager.GetAllUniqueSharedComponentData(m_CacheduniqueRendererTypes);

        //var forEachFilter = m_InstanceRendererGroup.CreateForEachFilter(m_CacheduniqueRendererTypes);
        //cmdBuffer.Clear();
        //for (int i = 0; i != m_CacheduniqueRendererTypes.Count; i++)
        //{
        //    var renderer = m_CacheduniqueRendererTypes[i];

        //    if (renderer.mesh && renderer.material)
        //    {
        //        var transforms = m_InstanceRendererGroup.GetComponentDataArray<LocalToWorld>(forEachFilter, i);

        //        CopyMatrices(transforms, 0, transforms.Length, matrixBuffer);

        //        args[0] = (uint)renderer.mesh.GetIndexCount(renderer.subMesh);
        //        args[1] = (uint)transforms.Length;
        //        args[2] = (uint)renderer.mesh.GetIndexStart(renderer.subMesh);
        //        args[3] = (uint)renderer.mesh.GetBaseVertex(renderer.subMesh);

        //        transformBuffers[i-1].SetData(matrixBuffer);
        //        argsBuffer.SetData(args);
        //        renderer.material.SetBuffer("transformBuffer", transformBuffers[i - 1]);

        //        //cmdBuffer.DrawMeshInstancedIndirect(renderer.mesh, renderer.subMesh, renderer.material,
        //        //   -1, argsBuffer, 0, null);

        //        Graphics.DrawMeshInstancedIndirect(renderer.mesh, renderer.subMesh, renderer.material,
        //            new Bounds(Vector3.zero, Vector3.one * 999), argsBuffer, 0, null, renderer.castShadows, renderer.receiveShadows);
        //    }
        //    else
        //    {
        //        args[0] = args[1] = args[2] = args[3] = 0;
        //    }
        //}

        //m_CacheduniqueRendererTypes.Clear();
        //forEachFilter.Dispose();
    }

    protected override void OnDestroyManager()
    {
        if (transformBuffers != null)
        {
            for (int i = 0; i < transformBuffers.Length; i++)
            {
                transformBuffers[i].Release();
            }
            transformBuffers = null;
            argsBuffer.Release();
            argsBuffer = null;
        }
    }
}