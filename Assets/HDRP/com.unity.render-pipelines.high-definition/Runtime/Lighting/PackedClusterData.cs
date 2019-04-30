using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// ��HDRP�У�Cluster������logbase�Լ�layeredoffset�������ݵģ������һ���Խ�Լ�Ĵ���
    /// ����ֱ��ʹ��һ��float4����Ϊ���buffer����GPU ���ģ�GPU����ֱ����floatװint���������
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
