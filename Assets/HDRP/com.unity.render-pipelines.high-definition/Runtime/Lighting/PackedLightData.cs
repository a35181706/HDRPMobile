using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// ��HDRP�У�Ŀǰ�����ֹ�Դ���ݣ�
    /// DirectionalLightData��EnvLightData��LightData
    /// ���ֹ�Դ���ݴ�����Խ�Լ�Ĵ�����GPU��alignΪfloat4,����packer��float4Ϊ��λ
    /// </summary>
    [GenerateHLSL(PackingRules.Exact, false)]
    public struct PackedLightData
    {
        /// <summary>
        ///     DirectionalLightData��:
        ///                 xyz:positionRWS
        ///                 w:lightLayers
        ///     LightData��:positionRWS
        ///                 xyz:positionRWS
        ///                 w:lightLayers
        ///     EnvLightData��:
        ///                 xyz:capturePositionRWS
        ///                 w:lightLayers
        /// </summary>
        public Vector4 packedData1;

        /// <summary>
        ///     DirectionalLightData��:
        ///                 xyz:forward
        ///                 w:lightDimmer
        ///     LightData��:
        ///                 xyz:forward
        ///                 w:lightDimmer
        ///     EnvLightData��:
        ///                 xyz:proxyForward
        ///                 w:influenceShapeType
        /// </summary>
        public Vector4 packedData2;

        /// <summary>
        ///     DirectionalLightData��:
        ///                  xyz:right -Rescaled by (2 / shapeWidth)
        ///                  w:volumetricLightDimmer -Replaces 'lightDimer'
        ///     LightData��:
        ///                  xyz:right -If spot: rescaled by cot(outerHalfAngle); if projector: rescaled by (2 / shapeWidth)
        ///                  w:volumetricLightDimmer -Replaces 'lightDimer'
        ///     
        ///     EnvLightData��:
        ///                  xyz:proxyRight
        ///                  w:minProjectionDistance
        /// </summary>
        public Vector4 packedData3;

        /// <summary>
        ///     DirectionalLightData��:
        ///                   xyz:up -Rescaled by (2 / shapeWidth)
        ///                   w:angleScale -Sun disk highlight
        ///     LightData��:
        ///                   xyz:up - If spot: rescaled by cot(outerHalfAngle); if projector: rescaled by (2 / shapeHeight)
        ///                   w:angleScale Spot light
        ///     EnvLightData��:
        ///                   xyz:proxyUp
        ///                   w:weight
        /// </summary>
        public Vector4 packedData4;

        /// <summary>
        ///     DirectionalLightData��:
        ///                  xyz:color
        ///                  w:angleOffset -Sun disk highlight
        ///     LightData��:
        ///                  xyz:color
        ///                  w:angleOffset -Spot light
        ///     EnvLightData��:
        ///                  xyz:proxyExtents -Box: extents = box extents,Sphere: extents.x = sphere radius
        ///                  w:multiplier
        /// </summary>
        public Vector4 packedData5;

        /// <summary>
        ///     DirectionalLightData��:
        ///                  xyzw:shadowMaskSelector -Used with ShadowMask feature
        ///     LightData��:
        ///                  xyzw:shadowMaskSelector -Used with ShadowMask feature
        ///     EnvLightData��:
        ///                  xyz:influencePositionRWS 
        ///                  w:envIndex
        /// </summary>
        public Vector4 packedData6;

        /// <summary>
        /// xyzw:
        ///     DirectionalLightData��:
        ///                   x :cookieIndex -1 if unused
        ///                   y:tileCookie
        ///                   z:shadowIndex - -1 if unused
        ///                   w:contactShadowIndex - -1 if unused 
        ///     LightData��:
        ///                  x: lightType
        ///                  y:range
        ///                  z:rangeAttenuationScale
        ///                  w:rangeAttenuationBias
        ///     EnvLightData��:
        ///                  xyz:influenceForward,
        ///                  w:boxSideFadeNegative.x
        /// </summary>
        public Vector4 packedData7;

        /// <summary>
        ///     DirectionalLightData��:
        ///                  x: shadowDimmer 
        ///                  y:volumetricShadowDimmer -Replaces 'shadowDimmer'
        ///                  z:nonLightMappedOnly -Used with ShadowMask 
        ///                  w:minRoughness -Hack
        ///     LightData��:
        ///                  x: cookieIndex  - -1 if unused
        ///                  y:tileCookie
        ///                  z:shadowIndex - -1 if unused 
        ///                  w:contactShadowIndex -negative if unused
        ///     EnvLightData��:
        ///                  xyz:influenceUp
        ///                  w:boxSideFadeNegative.y
        /// </summary>
        public Vector4 packedData8;

        /// <summary>
        ///    DirectionalLightData��:
        ///                   x:diffuseDimmer
        ///                   y:specularDimmer
        ///                   z:unused
        ///                   w:unused
        ///     LightData��:
        ///                   x:shadowDimmer
        ///                   y:volumetricShadowDimmer -Replaces 'shadowDimmer'
        ///                   z:nonLightMappedOnly -Used with ShadowMask feature
        ///                   w:minRoughness -This is use to give a small "area" to punctual light, as if we have a light with a radius.
        ///     EnvLightData��:
        ///                   xyz:influenceRight
        ///                   w:boxSideFadeNegative.z
        /// </summary>
        public Vector4 packedData9;

        /// <summary>
        ///    DirectionalLightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData��:
        ///                   xy:size -Used by area (X = length or width, Y = height) and punctual lights (X = radius)
        ///                   z:diffuseDimmer
        ///                   w:specularDimmer
        ///     EnvLightData��:
        ///                   xyz:influenceExtents
        ///                   w:boxSideFadePositive.x
        /// </summary>
        public Vector4 packedData10;

        /// <summary>
        ///    DirectionalLightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData��:
        ///                   xyz:blendDistancePositive
        ///                   w:boxSideFadePositive.y
        /// </summary>
        public Vector4 packedData11;

        /// <summary>
        ///    DirectionalLightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData��:
        ///                   xyz:blendDistanceNegative
        ///                   w:boxSideFadePositive.z
        /// </summary>
        public Vector4 packedData12;

        /// <summary>
        ///    DirectionalLightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData��:
        ///                   xyz:blendNormalDistancePositive
        ///                   w:unused
        /// </summary>
        public Vector4 packedData13;

        /// <summary>
        ///    DirectionalLightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData��:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData��:
        ///                   xyz:blendNormalDistanceNegative
        ///                   w:unused
        /// </summary>
        public Vector4 packedData14;
    }
}
