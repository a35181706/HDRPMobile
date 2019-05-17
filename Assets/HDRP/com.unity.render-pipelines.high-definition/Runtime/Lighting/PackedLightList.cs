using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// ��HDRP�У�Cluster Lighting��FPTL Lighting�����ɱȽ϶�����ݣ������Ǵ����һ���Լ�Ĵ�������
    /// ����ֱ��ʹ��һ��float4����Ϊ���buffer����GPU ���ģ�GPU����ֱ����floatװint���������
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
