
#ifdef SHADER_VARIABLES_INCLUDE_CB

#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/ShaderVariablesLightLoop.cs.hlsl"

#else
    #include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightDefinition.cs.hlsl"

#ifdef USE_PACKED_LIGHTDATA
	#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/PackedLightData.cs.hlsl"

#endif

#ifdef USE_PACKED_LIGHTLIST
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/PackedLightList.cs.hlsl"
#endif


// used for compute shader
StructuredBuffer<uint>  g_vBigTileLightList;

#ifdef USE_PACKED_LIGHTLIST

StructuredBuffer<PackedLightList> g_PackedLightListBuffer;
#else
StructuredBuffer<uint>  g_vLayeredOffsetsBuffer;
StructuredBuffer<float> g_logBaseBuffer;
StructuredBuffer<uint>  g_vLightListGlobal;
#endif




#ifdef USE_INDIRECT
        StructuredBuffer<uint> g_TileFeatureFlags;
#endif


#ifdef USE_PACKED_LIGHTDATA
		StructuredBuffer<PackedLightData> _PackedLightDatas;
#else
		StructuredBuffer<DirectionalLightData> _DirectionalLightDatas;
		StructuredBuffer<LightData>            _LightDatas;
		StructuredBuffer<EnvLightData>         _EnvLightDatas;
#endif

    // Used by directional and spot lights
    TEXTURE2D_ARRAY(_CookieTextures);
    // Used by area lights
    TEXTURE2D_ARRAY(_AreaCookieTextures);

    // Used by point lights
    TEXTURECUBE_ARRAY_ABSTRACT(_CookieCubeTextures);

    // Use texture array for reflection (or LatLong 2D array for mobile)
    TEXTURECUBE_ARRAY_ABSTRACT(_EnvCubemapTextures);
    TEXTURE2D_ARRAY(_Env2DTextures);

    // Contact shadows
    TEXTURE2D_X(_DeferredShadowTexture);

#if SHADEROPTIONS_RAYTRACING
    // Area shadow paper texture
    TEXTURE2D_ARRAY(_AreaShadowTexture);

    // Indirect Diffuse Texture
    TEXTURE2D_X(_IndirectDiffuseTexture);
#endif

#endif
