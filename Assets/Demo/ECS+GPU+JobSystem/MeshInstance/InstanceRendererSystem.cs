using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.PlayerLoop;
using Unity.Collections;

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

	// This is the ugly bit, necessary until Graphics.DrawMeshInstanced supports NativeArrays pulling the data in from a job.
    public unsafe static void CopyMatrices(NativeArray<LocalToWorld> transforms, int beginIndex, int length, Matrix4x4[] outMatrices)
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
            System.IntPtr srcPtr = new System.IntPtr(transforms.GetUnsafePtr());
            System.IntPtr DstPtr = new System.IntPtr((void*)matricesPtr);
            System.Runtime.InteropServices.Marshal.Copy(srcPtr,new System.IntPtr[] { DstPtr },beginIndex,length);


        }
    }

	protected override void OnCreateManager()
	{

        m_InstanceRendererGroup = GetComponentGroup(ComponentType.ReadOnly<InstanceRenderer>(),
            ComponentType.ReadOnly<LocalToWorld>(),
            ComponentType.Exclude<FurstumCulledComponent>());

        

    }

	protected override void OnUpdate()
	{
        
  //      EntityManager.GetAllUniqueSharedComponentData(m_CacheduniqueRendererTypes);

  //      for (int i = 0;i != m_CacheduniqueRendererTypes.Count;i++)
  //      {
  //          var renderer = m_CacheduniqueRendererTypes[i];

  //          m_InstanceRendererGroup.SetFilter(new InstanceRenderer() {
  //              mesh = renderer.mesh,
  //              subMesh = renderer.subMesh,
  //              material = renderer.material,
  //              castShadows = renderer.castShadows,
  //              receiveShadows = renderer.receiveShadows
  //              });
  //          if (renderer.mesh && renderer.material)
  //          {
                
  //              var transforms = m_InstanceRendererGroup.ToComponentDataArray<LocalToWorld>( Unity.Collections.Allocator.Temp );
              
  //              int beginIndex = 0;
  //              while (beginIndex < transforms.Length)
  //              {
  //                  int length = math.min(m_MatricesArray.Length, transforms.Length - beginIndex);
  //                  CopyMatrices(transforms, beginIndex, length, m_MatricesArray);
  //                  Graphics.DrawMeshInstanced(renderer.mesh, renderer.subMesh,
  //                      renderer.material, m_MatricesArray, length, null,
  //                      renderer.castShadows, renderer.receiveShadows);
  //                  beginIndex += length;
  //              }
  //              transforms.Dispose();
  //          }
          
  //      }

		//m_CacheduniqueRendererTypes.Clear();

	}
}
