using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// 在HDRP中，Cluster会生成logbase以及layeredoffset两个数据的，打包到一块以节约寄存器
    /// 不能直接使用一个float4，因为这个buffer是在GPU 填充的，GPU填充的直接用float装int会出现问题
    /// </summary>
    [GenerateHLSL(PackingRules.Exact, false)]
    public struct PackedClusterData
    {
        public int layeredOffset;


        public float logBase;
        public float unused00;
        public float unused01;
    }

}
