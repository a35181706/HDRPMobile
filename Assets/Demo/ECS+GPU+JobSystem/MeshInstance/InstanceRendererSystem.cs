using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Experimental.PlayerLoop;

/// <summary>
/// Renders all Entities containing both MeshInstanceRenderer & TransformMatrix components.
/// </summary>
[UpdateAfter(typeof(PreLateUpdate.ParticleSystemBeginUpdateAll))]
[UpdateAfter(typeof(FrustumCullingBarrier))]
[UnityEngine.ExecuteInEditMode]
public class InstanceRendererSystem : ComponentSystem
{
    // MeshInstanced最多1023个一次
    Matrix4x4[]                 m_MatricesArray = new Matrix4x4[1023];
	List<InstanceRenderer>  m_CacheduniqueRendererTypes = new List<InstanceRenderer>(10);
	ComponentGroup              m_InstanceRendererGroup;

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

        m_InstanceRendererGroup = GetComponentGroup(typeof(InstanceRenderer), typeof(TransformMatrix), ComponentType.Subtractive<FurstumCulledComponent>());
	}

	protected override void OnUpdate()
	{
        
        EntityManager.GetAllUniqueSharedComponentDatas(m_CacheduniqueRendererTypes);

		var forEachFilter = m_InstanceRendererGroup.CreateForEachFilter(m_CacheduniqueRendererTypes);

        for (int i = 0;i != m_CacheduniqueRendererTypes.Count;i++)
        {
            var renderer = m_CacheduniqueRendererTypes[i];

            if (renderer.mesh && renderer.material)
            {
                var transforms = m_InstanceRendererGroup.GetComponentDataArray<TransformMatrix>(forEachFilter, i);

                int beginIndex = 0;
                while (beginIndex < transforms.Length)
                {
                    int length = math.min(m_MatricesArray.Length, transforms.Length - beginIndex);
                    CopyMatrices(transforms, beginIndex, length, m_MatricesArray);
                    Graphics.DrawMeshInstanced(renderer.mesh, renderer.subMesh,
                        renderer.material, m_MatricesArray, length, null,
                        renderer.castShadows, renderer.receiveShadows);
                    beginIndex += length;
                }
             
            }
          
        }

		m_CacheduniqueRendererTypes.Clear();
		forEachFilter.Dispose();
	}
}
