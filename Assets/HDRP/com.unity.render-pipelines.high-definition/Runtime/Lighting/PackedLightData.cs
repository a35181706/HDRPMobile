using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// 在HDRP中，Cluster会生成logbase以及layeredoffset两个数据的，打包到一块以节约寄存器
    /// </summary>
    [GenerateHLSL( PackingRules.Exact,false)]
    public struct PackedClusterData
    {
        /// <summary>
        /// x:layerdoffset uint
        /// y:logbase float
        /// z: noused
        /// w:noused
        /// </summary>
        public Vector4 packedData1;
    }

}
