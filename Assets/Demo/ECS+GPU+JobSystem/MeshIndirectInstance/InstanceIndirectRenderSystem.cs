using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.PlayerLoop;
using Unity.Collections;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

/// <summary>
/// 自己实现的基于DrawMeshInstanceIndirect渲染的ECS版本
/// </summary>
[UpdateAfter(typeof(FrustumCullingSystem))]
public class InstanceIndirectRenderSystem : ComponentSystem
{
    private List<InstanceIndirectRenderer> m_CacheduniqueRendererTypes = new List<InstanceIndirectRenderer>(10);
    private EntityQuery m_InstanceRendererGroup;

    private ComputeBuffer m_IndirecttArgsBuffer;
    private ComputeBuffer []m_TransformBuffers = null;
    private CommandBuffer m_RenderCommandBuffer;

    //drawindrect是写死的5个，unity说的
    private uint[] m_IndirecttArgs = new uint[5] { 0, 0, 0, 0, 0 };
    private Matrix4x4[] m_MatrixBuffer;
    private bool bInit = false;

    public unsafe static void CopyMatrices(NativeArray<LocalToWorld> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
    {
        fixed (Matrix4x4* matricesPtr = outMatrices)
        {
            Assert.AreEqual(sizeof(Matrix4x4), sizeof(LocalToWorld));

            var matricesSlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<LocalToWorld>(matricesPtr, sizeof(Matrix4x4), length);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref matricesSlice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif

            matricesSlice.CopyFrom(transforms.Slice(beginIndex, length));
        }

    }

    protected override void OnCreateManager()
    {
        m_InstanceRendererGroup = GetEntityQuery(ComponentType.ReadOnly<InstanceIndirectRenderer>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.Exclude<FurstumCulledComponent>());    
    }

    void InitRender()
    {
        if (bInit || Camera.allCamerasCount <= 0)
        {
            return;
        }
        bInit = true;
        m_IndirecttArgsBuffer = new ComputeBuffer(1, m_IndirecttArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix4x4));
        m_MatrixBuffer = new Matrix4x4[InitDemo.m_instance.earchSwordCout];
        m_TransformBuffers = new ComputeBuffer[InitDemo.m_instance.swordPrefabs.Length];

        for (int i = 0;i < m_TransformBuffers.Length;i++)
        {
            m_TransformBuffers[i] = new ComputeBuffer(InitDemo.m_instance.earchSwordCout, size);
        }

        m_RenderCommandBuffer = new UnityEngine.Rendering.CommandBuffer();

        foreach (Camera cam in Camera.allCameras)
        {
            cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_RenderCommandBuffer);
        }
    }



    protected override void OnUpdate()
    {
        InitRender();

        if (!bInit)
        {
            return;
        }

        EntityManager.GetAllUniqueSharedComponentData(m_CacheduniqueRendererTypes);
        m_RenderCommandBuffer.Clear();

        for (int i = 0; i != m_CacheduniqueRendererTypes.Count; i++)
        {
            var renderer = m_CacheduniqueRendererTypes[i];

            if (renderer.mesh && renderer.material)
            {
                m_InstanceRendererGroup.ResetFilter();
                m_InstanceRendererGroup.SetFilter(renderer);
                var transforms = m_InstanceRendererGroup.ToComponentDataArray<LocalToWorld>(Unity.Collections.Allocator.TempJob);


                CopyMatrices(transforms, 0, transforms.Length, m_MatrixBuffer);

                m_IndirecttArgs[0] = (uint)renderer.mesh.GetIndexCount(renderer.subMesh);
                m_IndirecttArgs[1] = (uint)transforms.Length;
                m_IndirecttArgs[2] = (uint)renderer.mesh.GetIndexStart(renderer.subMesh);
                m_IndirecttArgs[3] = (uint)renderer.mesh.GetBaseVertex(renderer.subMesh);

                m_TransformBuffers[i - 1].SetData(m_MatrixBuffer);
                m_IndirecttArgsBuffer.SetData(m_IndirecttArgs);
                renderer.material.SetBuffer("transformBuffer", m_TransformBuffers[i - 1]);


                //Graphics.DrawMeshInstancedIndirect(renderer.mesh, renderer.subMesh, renderer.material, new Bounds(Vector3.zero, Vector3.one), m_IndirecttArgsBuffer, 0, null);

                m_RenderCommandBuffer.DrawMeshInstancedIndirect(renderer.mesh, renderer.subMesh, renderer.material,
                   -1, m_IndirecttArgsBuffer, 0, null);

                transforms.Dispose();
            }
            else
            {
                m_IndirecttArgs[0] = m_IndirecttArgs[1] = m_IndirecttArgs[2] = m_IndirecttArgs[3] = 0;
            }
        }

        m_CacheduniqueRendererTypes.Clear();
    }

    protected override void OnDestroyManager()
    {
        if (m_TransformBuffers != null)
        {
            for (int i = 0; i < m_TransformBuffers.Length; i++)
            {
                m_TransformBuffers[i].Release();
            }
            m_TransformBuffers = null;
            m_IndirecttArgsBuffer.Release();
            m_IndirecttArgsBuffer = null;
        }
    }
}