using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// 在HDRP中，Cluster Lighting、FPTL Lighting会生成比较多的数据，把他们打包在一起节约寄存器数量
    /// 不能直接使用一个float4，因为这个buffer是在GPU 填充的，GPU填充的直接用float装int会出现问题
    /// </summary>
    [GenerateHLSL(PackingRules.Exact, false)]
    public struct PackedLightList
    {
        public int FptlLightList;//FPTL 
        public int layeredOffset;//Cluster
        public int PerVoxelLightList;//Cluster 
        public float logBase;//Cluster 
    }

}
