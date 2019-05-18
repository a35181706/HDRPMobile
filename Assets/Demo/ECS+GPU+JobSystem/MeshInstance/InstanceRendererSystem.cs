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
/// 渲染有InstanceRenderer组件与LocalToWorld组件
/// </summary>
[UpdateAfter(typeof(FrustumCullingSystem))]
public class InstanceRendererSystem : ComponentSystem
{
    // MeshInstanced最多1023个一次
    private Matrix4x4[]                 m_MatricesArray = new Matrix4x4[1023];
    private List<InstanceRenderer>  m_CacheduniqueRendererTypes = new List<InstanceRenderer>(10);
    private EntityQuery m_InstanceRendererGroup;
    private CommandBuffer m_RenderCommandBuffer = null;
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
        m_InstanceRendererGroup = GetEntityQuery(ComponentType.ReadOnly<InstanceRenderer>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.Exclude<FurstumCulledComponent>());


    }

    protected void InitRender()
    {
        if (bInit || Camera.allCamerasCount <= 0)
        {
            return;
        }
        bInit = true;
        m_RenderCommandBuffer = new CommandBuffer();
        m_RenderCommandBuffer.name = "InstanceRendererSystem";

        foreach (Camera cam in Camera.allCameras)
        {
            cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, m_RenderCommandBuffer);
        }
    }

    protected override void OnDestroyManager()
    {
        if (null != m_RenderCommandBuffer)
        {
            m_RenderCommandBuffer.Release();
            m_RenderCommandBuffer = null;
        }

        bInit = true;
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

                int beginIndex = 0;
                while (beginIndex < transforms.Length)
                {
                    int length = math.min(m_MatricesArray.Length, transforms.Length - beginIndex);
                    CopyMatrices(transforms, beginIndex, length, m_MatricesArray);
                    //Graphics.DrawMeshInstanced(renderer.mesh, renderer.subMesh, renderer.material, m_MatricesArray, length, null);
                    m_RenderCommandBuffer.DrawMeshInstanced(renderer.mesh, renderer.subMesh, renderer.material, -1, m_MatricesArray, length, null);
                    beginIndex += length;
                }
                transforms.Dispose();
            }
        }
        m_CacheduniqueRendererTypes.Clear();

    }
}
