#ifndef LIGHTLOOP_HD_SHADOW_HLSL
#define LIGHTLOOP_HD_SHADOW_HLSL

#define SHADOW_OPTIMIZE_REGISTER_USAGE 1

#ifndef SHADOW_USE_VIEW_BIAS_SCALING
#define SHADOW_USE_VIEW_BIAS_SCALING            1   // Enable view bias scaling to mitigate light leaking across edges. Uses the light vector if SHADOW_USE_ONLY_VIEW_BASED_BIASING is defined, otherwise uses the normal.
#endif
// Note: Sample biasing work well but is very costly in term of VGPR, disable it for now
#define SHADOW_USE_SAMPLE_BIASING               0   // Enable per sample biasing for wide multi-tap PCF filters. Incompatible with SHADOW_USE_ONLY_VIEW_BASED_BIASING.
#define SHADOW_USE_DEPTH_BIAS                   0   // Enable clip space z biasing

#if SHADOW_OPTIMIZE_REGISTER_USAGE == 1
#   pragma warning( disable : 3557 ) // loop only executes for 1 iteration(s)
#endif

# include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/HDShadowContext.hlsl"

float GetDirectionalShadowAttenuation(HDShadowContext shadowContext, float2 positionSS, float3 positionWS, float3 normalWS, int shadowDataIndex, float3 L)
{
    return EvalShadow_CascadedDepth_Blend(shadowContext, _ShadowmapCascadeAtlas, s_linear_clamp_compare_sampler, positionSS, positionWS, normalWS, shadowDataIndex, L);
}

float GetDirectionalShadowAttenuation(HDShadowContext shadowContext, float3 positionWS, float3 normalWS, int shadowDataIndex, float3 L, float2 positionSS)
{
    return GetDirectionalShadowAttenuation(shadowContext, positionSS, positionWS, normalWS, shadowDataIndex, L);
}

float GetPunctualShadowAttenuation(HDShadowContext shadowContext, float2 positionSS, float3 positionWS, float3 normalWS, int shadowDataIndex, float3 L, float L_dist, bool pointLight, bool perspecive)
{
#if (defined(SUPPORTS_WAVE_INTRINSICS) && !defined(LIGHTLOOP_DISABLE_TILE_AND_CLUSTER))
    shadowDataIndex = WaveReadLaneFirst(shadowDataIndex);
#endif

    // Note: Here we assume that all the shadow map cube faces have been added contiguously in the buffer to retreive the shadow information
    HDShadowData sd = FetchHDShadowData(shadowDataIndex);

    if (pointLight)
    {
		HDShadowData newsd = FetchHDShadowData(shadowDataIndex + CubeMapFaceID(-L));
        sd.rot0 = newsd.rot0;
        sd.rot1 = newsd.rot1;
        sd.rot2 = newsd.rot2;
        sd.atlasOffset = newsd.atlasOffset;
    }

    return EvalShadow_PunctualDepth(sd, _ShadowmapAtlas, s_linear_clamp_compare_sampler, positionSS, positionWS, normalWS, L, L_dist, perspecive);
}

float GetPunctualShadowAttenuation(HDShadowContext shadowContext, float3 positionWS, float3 normalWS, int shadowDataIndex, float3 L, float L_dist, float2 positionSS, bool pointLight, bool perspecive)
{
    return GetPunctualShadowAttenuation(shadowContext, positionSS, positionWS, normalWS, shadowDataIndex, L, L_dist, pointLight, perspecive);
}

float GetPunctualShadowClosestDistance(HDShadowContext shadowContext, SamplerState sampl, real3 positionWS, int shadowDataIndex, float3 L, float3 lightPositionWS, bool pointLight)
{
#if (defined(SUPPORTS_WAVE_INTRINSICS) && !defined(LIGHTLOOP_DISABLE_TILE_AND_CLUSTER))
    shadowDataIndex = WaveReadLaneFirst(shadowDataIndex);
#endif

    // Note: Here we assume that all the shadow map cube faces have been added contiguously in the buffer to retreive the shadow information
    // TODO: if on the light type to retrieve the good shadow data
    HDShadowData sd = FetchHDShadowData(shadowDataIndex);
    
    if (pointLight)
    {
		HDShadowData newsd = FetchHDShadowData(shadowDataIndex + CubeMapFaceID(-L));

        sd.shadowToWorld = newsd.shadowToWorld;
        sd.atlasOffset = newsd.atlasOffset;
        sd.rot0 = newsd.rot0;
        sd.rot1 = newsd.rot1;
        sd.rot2 = newsd.rot2;
    }
    
    return EvalShadow_SampleClosestDistance_Punctual(sd, _ShadowmapAtlas, sampl, positionWS, L, lightPositionWS);
}

float GetAreaLightAttenuation(HDShadowContext shadowContext, float2 positionSS, float3 positionWS, float3 normalWS, int shadowDataIndex, float3 L, float L_dist)
{
    HDShadowData sd = FetchHDShadowData(shadowDataIndex);
    return EvalShadow_AreaDepth(sd, _AreaShadowmapMomentAtlas, positionSS, positionWS, normalWS, L, L_dist, true);
}


#endif // LIGHTLOOP_HD_SHADOW_HLSL
