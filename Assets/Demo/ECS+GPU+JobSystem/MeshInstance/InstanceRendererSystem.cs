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
/// Renders all Entities containing both MeshInstanceRenderer & TransformMatrix components.
/// </summary>

[UpdateAfter(typeof(FrustumCullingBarrier))]
public class InstanceRendererSystem : ComponentSystem
{
    // MeshInstanced最多1023个一次
    Matrix4x4[]                 m_MatricesArray = new Matrix4x4[1023];
	List<InstanceRenderer>  m_CacheduniqueRendererTypes = new List<InstanceRenderer>(10);
	ComponentGroup              m_InstanceRendererGroup;
    List<HDAdditionalCameraData> allAdditionCamera = new List<HDAdditionalCameraData>(10);
    CommandBuffer renderCommandBuffer = null;
    private bool bInit = false;
    // This is the ugly bit, necessary until Graphics.DrawMeshInstanced supports NativeArrays pulling the data in from a job.
    public unsafe static void CopyMatrices(NativeArray<LocalToWorld> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
    {
        //	    // @TODO: This is using unsafe code because the Unity DrawInstances API takes a Matrix4x4[] instead of NativeArray.
        //	    // We want to use the ComponentDataArray.CopyTo method
        //	    // because internally it uses memcpy to copy the data,
        //	    // if the nativeslice layout matches the layout of the component data. It's very fast...
        //        fixed (Matrix4x4* matricesPtr = outMatrices)
        //        {
        //            Assert.AreEqual(sizeof(Matrix4x4), sizeof(LocalToWorld));
        //	        var matricesSlice = NativeSliceUnsafeUtility.ConvertExistingDataToNativeSlice<LocalToWorld>(matricesPtr, sizeof(Matrix4x4), length);
        //	        #if ENABLE_UNITY_COLLECTIONS_CHECKS
        //	        NativeSliceUnsafeUtility.SetAtomicSafetyHandle(ref matricesSlice, AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
        //#endif
        //            System.IntPtr srcPtr = new System.IntPtr(transforms.GetUnsafePtr());
        //            System.IntPtr DstPtr = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement<Matrix4x4>(outMatrices, 0);

        //            System.Runtime.InteropServices.Marshal.Copy(srcPtr,new System.IntPtr[] { DstPtr },beginIndex,length);


        //        }

        for (int i = beginIndex;i < length;i++)
        {
            LocalToWorld tran = transforms[i];
            outMatrices[i].m00 = tran.Value.c0.x;
            outMatrices[i].m01 = tran.Value.c0.y;
            outMatrices[i].m02 = tran.Value.c0.z;
            outMatrices[i].m03 = tran.Value.c0.w;

            outMatrices[i].m10 = tran.Value.c1.x;
            outMatrices[i].m11 = tran.Value.c1.y;
            outMatrices[i].m12 = tran.Value.c1.z;
            outMatrices[i].m13 = tran.Value.c1.w;


            outMatrices[i].m20 = tran.Value.c2.x;
            outMatrices[i].m21 = tran.Value.c2.y;
            outMatrices[i].m22 = tran.Value.c2.z;
            outMatrices[i].m23 = tran.Value.c2.w;

            outMatrices[i].m30 = tran.Value.c3.x;
            outMatrices[i].m31 = tran.Value.c3.y;
            outMatrices[i].m32 = tran.Value.c3.z;
            outMatrices[i].m33 = tran.Value.c3.w;
        }

        
    }

	protected override void OnCreateManager()
	{
        m_InstanceRendererGroup = GetComponentGroup(ComponentType.ReadOnly<InstanceRenderer>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.Exclude<FurstumCulledComponent>());

        //HDCamera hdCamera;

    }

    void InitRender()
    {
        if (bInit || Camera.allCamerasCount <= 0)
        {
            return;
        }
        bInit = true;
        renderCommandBuffer = new CommandBuffer();
        renderCommandBuffer.name = "InstanceRendererSystem";

        foreach (Camera cam in Camera.allCameras)
        {
            //HDAdditionalCameraData additionCamera = cam.GetComponent<HDAdditionalCameraData>();

            //if (additionCamera)
            //{
            //    additionCamera.customRender += AdditionCamera_customRender;
            //    allAdditionCamera.Add(additionCamera);
            //}

            cam.AddCommandBuffer(CameraEvent.AfterGBuffer, renderCommandBuffer);
        }


    }


    protected override void OnDestroyManager()
    {
        //foreach (HDAdditionalCameraData cam in allAdditionCamera)
        //{
        //    cam.customRender -= AdditionCamera_customRender;
        //}

        allAdditionCamera.Clear();

        if (null != renderCommandBuffer)
        {
            renderCommandBuffer.Release();
            renderCommandBuffer = null;
        }


        bInit = true;
    }

    private void AdditionCamera_customRender(UnityEngine.Rendering.ScriptableRenderContext context, HDCamera camera)
    {
        //context.ExecuteCommandBufferAsync(new UnityEngine.Rendering.CommandBuffer(), UnityEngine.Rendering.ComputeQueueType.)
        context.ExecuteCommandBuffer(renderCommandBuffer);
    }

	protected override void OnUpdate()
	{
        InitRender();

        if (!bInit)
        {
            return;
        }

        EntityManager.GetAllUniqueSharedComponentData(m_CacheduniqueRendererTypes);
        renderCommandBuffer.Clear();
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

                    //Graphics.DrawMeshInstanced(renderer.mesh, renderer.subMesh,
                    //    renderer.material, m_MatricesArray, length, null,
                    //    renderer.castShadows, renderer.receiveShadows);

                    renderCommandBuffer.DrawMeshInstanced(renderer.mesh, renderer.subMesh, renderer.material, -1, m_MatricesArray, length, null);
                    beginIndex += length;
                }
                transforms.Dispose();
            }

        }

        m_CacheduniqueRendererTypes.Clear();

    }
}
