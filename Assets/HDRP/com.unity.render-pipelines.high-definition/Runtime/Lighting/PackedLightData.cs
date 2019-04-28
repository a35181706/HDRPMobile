using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// ��HDRP�У�Cluster������logbase�Լ�layeredoffset�������ݵģ������һ���Խ�Լ�Ĵ���
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
