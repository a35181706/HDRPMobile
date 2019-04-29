using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// 在HDRP中，目前有三种光源数据：
    /// DirectionalLightData，EnvLightData，LightData
    /// 三种光源数据打包，以节约寄存器，GPU的align为float4,所以packer以float4为单位
    /// </summary>
    [GenerateHLSL(PackingRules.Exact, false)]
    public struct PackedLightData
    {
        /// <summary>
        ///     DirectionalLightData中:
        ///                 xyz:positionRWS
        ///                 w:lightLayers
        ///     LightData中:positionRWS
        ///                 xyz:positionRWS
        ///                 w:lightLayers
        ///     EnvLightData中:
        ///                 xyz:capturePositionRWS
        ///                 w:lightLayers
        /// </summary>
        public Vector4 packedData1;

        /// <summary>
        ///     DirectionalLightData中:
        ///                 xyz:forward
        ///                 w:lightDimmer
        ///     LightData中:
        ///                 xyz:forward
        ///                 w:lightDimmer
        ///     EnvLightData中:
        ///                 xyz:proxyForward
        ///                 w:influenceShapeType
        /// </summary>
        public Vector4 packedData2;

        /// <summary>
        ///     DirectionalLightData中:
        ///                  xyz:right -Rescaled by (2 / shapeWidth)
        ///                  w:volumetricLightDimmer -Replaces 'lightDimer'
        ///     LightData中:
        ///                  xyz:right -If spot: rescaled by cot(outerHalfAngle); if projector: rescaled by (2 / shapeWidth)
        ///                  w:volumetricLightDimmer -Replaces 'lightDimer'
        ///     
        ///     EnvLightData中:
        ///                  xyz:proxyRight
        ///                  w:minProjectionDistance
        /// </summary>
        public Vector4 packedData3;

        /// <summary>
        ///     DirectionalLightData中:
        ///                   xyz:up -Rescaled by (2 / shapeWidth)
        ///                   w:angleScale -Sun disk highlight
        ///     LightData中:
        ///                   xyz:up - If spot: rescaled by cot(outerHalfAngle); if projector: rescaled by (2 / shapeHeight)
        ///                   w:angleScale Spot light
        ///     EnvLightData中:
        ///                   xyz:proxyUp
        ///                   w:weight
        /// </summary>
        public Vector4 packedData4;

        /// <summary>
        ///     DirectionalLightData中:
        ///                  xyz:color
        ///                  w:angleOffset -Sun disk highlight
        ///     LightData中:
        ///                  xyz:color
        ///                  w:angleOffset -Spot light
        ///     EnvLightData中:
        ///                  xyz:proxyExtents -Box: extents = box extents,Sphere: extents.x = sphere radius
        ///                  w:multiplier
        /// </summary>
        public Vector4 packedData5;

        /// <summary>
        ///     DirectionalLightData中:
        ///                  xyzw:shadowMaskSelector -Used with ShadowMask feature
        ///     LightData中:
        ///                  xyzw:shadowMaskSelector -Used with ShadowMask feature
        ///     EnvLightData中:
        ///                  xyz:influencePositionRWS 
        ///                  w:envIndex
        /// </summary>
        public Vector4 packedData6;

        /// <summary>
        /// xyzw:
        ///     DirectionalLightData中:
        ///                   x :cookieIndex -1 if unused
        ///                   y:tileCookie
        ///                   z:shadowIndex - -1 if unused
        ///                   w:contactShadowIndex - -1 if unused 
        ///     LightData中:
        ///                  x: lightType
        ///                  y:range
        ///                  z:rangeAttenuationScale
        ///                  w:rangeAttenuationBias
        ///     EnvLightData中:
        ///                  xyz:influenceForward,
        ///                  w:boxSideFadeNegative.x
        /// </summary>
        public Vector4 packedData7;

        /// <summary>
        ///     DirectionalLightData中:
        ///                  x: shadowDimmer 
        ///                  y:volumetricShadowDimmer -Replaces 'shadowDimmer'
        ///                  z:nonLightMappedOnly -Used with ShadowMask 
        ///                  w:minRoughness -Hack
        ///     LightData中:
        ///                  x: cookieIndex  - -1 if unused
        ///                  y:tileCookie
        ///                  z:shadowIndex - -1 if unused 
        ///                  w:contactShadowIndex -negative if unused
        ///     EnvLightData中:
        ///                  xyz:influenceUp
        ///                  w:boxSideFadeNegative.y
        /// </summary>
        public Vector4 packedData8;

        /// <summary>
        ///    DirectionalLightData中:
        ///                   x:diffuseDimmer
        ///                   y:specularDimmer
        ///                   z:unused
        ///                   w:unused
        ///     LightData中:
        ///                   x:shadowDimmer
        ///                   y:volumetricShadowDimmer -Replaces 'shadowDimmer'
        ///                   z:nonLightMappedOnly -Used with ShadowMask feature
        ///                   w:minRoughness -This is use to give a small "area" to punctual light, as if we have a light with a radius.
        ///     EnvLightData中:
        ///                   xyz:influenceRight
        ///                   w:boxSideFadeNegative.z
        /// </summary>
        public Vector4 packedData9;

        /// <summary>
        ///    DirectionalLightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData中:
        ///                   xy:size -Used by area (X = length or width, Y = height) and punctual lights (X = radius)
        ///                   z:diffuseDimmer
        ///                   w:specularDimmer
        ///     EnvLightData中:
        ///                   xyz:influenceExtents
        ///                   w:boxSideFadePositive.x
        /// </summary>
        public Vector4 packedData10;

        /// <summary>
        ///    DirectionalLightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData中:
        ///                   xyz:blendDistancePositive
        ///                   w:boxSideFadePositive.y
        /// </summary>
        public Vector4 packedData11;

        /// <summary>
        ///    DirectionalLightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData中:
        ///                   xyz:blendDistanceNegative
        ///                   w:boxSideFadePositive.z
        /// </summary>
        public Vector4 packedData12;

        /// <summary>
        ///    DirectionalLightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData中:
        ///                   xyz:blendNormalDistancePositive
        ///                   w:unused
        /// </summary>
        public Vector4 packedData13;

        /// <summary>
        ///    DirectionalLightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     LightData中:
        ///                   x:unused
        ///                   y:unused
        ///                   z:unused
        ///                   w:unused
        ///     EnvLightData中:
        ///                   xyz:blendNormalDistanceNegative
        ///                   w:unused
        /// </summary>
        public Vector4 packedData14;
    }
}
