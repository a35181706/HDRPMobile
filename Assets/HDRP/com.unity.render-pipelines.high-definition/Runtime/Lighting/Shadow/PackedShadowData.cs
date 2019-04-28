using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// 在HDRP中，目前有两种阴影数据：
    /// HDDirectionalShadowData，HDShadowData
    /// 两种数据打包，以节约寄存器，GPU的align为float4,所以packer以float4为单位
    /// </summary>
    [GenerateHLSL( PackingRules.Exact,false)]
    public struct PackedShadowData
    {
        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:sphereCascades 0
        ///     HDShadowData中:
        ///                 xyz:rot0
        ///                 w:edgeTolerance
        /// </summary>
        public Vector4 packedData1;

        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:sphereCascades 1
        ///     HDShadowData中:
        ///                 xyz:rot1
        ///                 w:flags
        /// </summary>
        public Vector4 packedData2;

        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:sphereCascades 2
        ///     HDShadowData中:
        ///                  xyz:rot2
        ///                  w:shadowFilterParams0.x
        /// </summary>
        public Vector4 packedData3;

        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:sphereCascades 3
        ///     HDShadowData中:
        ///                   xyz:pos
        ///                   w: shadowFilterParams0.y
        /// </summary>
        public Vector4 packedData4;

        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:cascadeDirection
        ///     HDShadowData中:
        ///                  xyzw:proj
        /// </summary>
        public Vector4 packedData5;

        /// <summary>
        ///     DirectionalLightData中:
        ///                  xyzw:cascadeBorders
        ///     HDShadowData中:
        ///                  xy:atlasOffset
        ///                  z:shadowFilterParams0.z
        ///                  w:shadowFilterParams0.w
        /// </summary>
        public Vector4 packedData6;

        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:unused
        ///     HDShadowData中:
        ///                  xyzw: zBufferParam
        /// </summary>
        public Vector4 packedData7;

        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:unused
        ///     HDShadowData中:
        ///                  xyzw: shadowMapSize
        /// </summary>
        public Vector4 packedData8;

        /// <summary>
        ///     HDDirectionalShadowData中:
        ///                 xyzw:unused
        ///     HDShadowData中:
        ///                   xyzw:viewBias
        /// </summary>
        public Vector4 packedData9;

        /// <summary>
        ///    HDDirectionalShadowData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData中:
        ///                   xyz:normalBias
        ///                   w:_padding
        /// </summary>
        public Vector4 packedData10;

        /// <summary>
        ///    HDDirectionalShadowData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData中:
        ///                   xyzw:shadowToWorld row0
        /// </summary>
        public Vector4 packedData11;

        /// <summary>
        ///    HDDirectionalShadowData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData中:
        ///                   xyzw:shadowToWorld row1
        /// </summary>
        public Vector4 packedData12;

        /// <summary>
        ///    HDDirectionalShadowData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData中:
        ///                   xyzw:shadowToWorld row2
        /// </summary>
        public Vector4 packedData13;

        /// <summary>
        ///    HDDirectionalShadowData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     HDShadowData中:
        ///                   xyzw:shadowToWorld row3
        /// </summary>
        public Vector4 packedData14;
    }

}
