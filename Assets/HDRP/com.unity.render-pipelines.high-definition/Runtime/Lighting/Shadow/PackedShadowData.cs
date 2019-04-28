using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// ��HDRP�У�Ŀǰ��������Ӱ���ݣ�
    /// HDDirectionalShadowData��HDShadowData
    /// �������ݴ�����Խ�Լ�Ĵ�����GPU��alignΪfloat4,����packer��float4Ϊ��λ
    /// </summary>
    [GenerateHLSL( PackingRules.Exact,false)]
    public struct PackedShadowData
    {
        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:sphereCascades 0
        ///     HDShadowData��:
        ///                 xyz:rot0
        ///                 w:edgeTolerance
        /// </summary>
        public Vector4 packedData1;

        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:sphereCascades 1
        ///     HDShadowData��:
        ///                 xyz:rot1
        ///                 w:flags
        /// </summary>
        public Vector4 packedData2;

        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:sphereCascades 2
        ///     HDShadowData��:
        ///                  xyz:rot2
        ///                  w:shadowFilterParams0.x
        /// </summary>
        public Vector4 packedData3;

        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:sphereCascades 3
        ///     HDShadowData��:
        ///                   xyz:pos
        ///                   w: shadowFilterParams0.y
        /// </summary>
        public Vector4 packedData4;

        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:cascadeDirection
        ///     HDShadowData��:
        ///                  xyzw:proj
        /// </summary>
        public Vector4 packedData5;

        /// <summary>
        ///     DirectionalLightData��:
        ///                  xyzw:cascadeBorders
        ///     HDShadowData��:
        ///                  xy:atlasOffset
        ///                  z:shadowFilterParams0.z
        ///                  w:shadowFilterParams0.w
        /// </summary>
        public Vector4 packedData6;

        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:unused
        ///     HDShadowData��:
        ///                  xyzw: zBufferParam
        /// </summary>
        public Vector4 packedData7;

        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:unused
        ///     HDShadowData��:
        ///                  xyzw: shadowMapSize
        /// </summary>
        public Vector4 packedData8;

        /// <summary>
        ///     HDDirectionalShadowData��:
        ///                 xyzw:unused
        ///     HDShadowData��:
        ///                   xyzw:viewBias
        /// </summary>
        public Vector4 packedData9;

        /// <summary>
        ///    HDDirectionalShadowData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData��:
        ///                   xyz:normalBias
        ///                   w:_padding
        /// </summary>
        public Vector4 packedData10;

        /// <summary>
        ///    HDDirectionalShadowData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData��:
        ///                   xyzw:shadowToWorld row0
        /// </summary>
        public Vector4 packedData11;

        /// <summary>
        ///    HDDirectionalShadowData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData��:
        ///                   xyzw:shadowToWorld row1
        /// </summary>
        public Vector4 packedData12;

        /// <summary>
        ///    HDDirectionalShadowData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData��:
        ///                   xyzw:shadowToWorld row2
        /// </summary>
        public Vector4 packedData13;

        /// <summary>
        ///    HDDirectionalShadowData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData��:
        ///                   xyzw:shadowToWorld row3
        /// </summary>
        public Vector4 packedData14;
    }

}
