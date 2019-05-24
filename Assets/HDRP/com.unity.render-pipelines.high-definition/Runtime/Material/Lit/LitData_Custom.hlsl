//-------------------------------------------------------------------------------------
// Defines
//-------------------------------------------------------------------------------------

// Use surface gradient normal mapping as it handle correctly triplanar normal mapping and multiple UVSet
#define SURFACE_GRADIENT

//-------------------------------------------------------------------------------------
// Fill SurfaceData/Builtin data function
//-------------------------------------------------------------------------------------
#include "Assets/HDRP/com.unity.render-pipelines.core/ShaderLibrary/Sampling/SampleUVMapping.hlsl"
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/MaterialUtilities.hlsl"
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/Decal/DecalUtilities.hlsl"
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDecalData.hlsl"

//#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/SphericalCapPivot/SPTDistribution.hlsl"
//#define SPECULAR_OCCLUSION_USE_SPTD

// Struct that gather UVMapping info of all layers + common calculation
// This is use to abstract the mapping that can differ on layers
struct LayerTexCoord
{
#ifndef LAYERED_LIT_SHADER
    UVMapping base;
    UVMapping details;
#else
    // Regular texcoord
    UVMapping base0;
    UVMapping base1;
    UVMapping base2;
    UVMapping base3;

    UVMapping details0;
    UVMapping details1;
    UVMapping details2;
    UVMapping details3;

    // Dedicated for blend mask
    UVMapping blendMask;
#endif

    // Store information that will be share by all UVMapping
    float3 vertexNormalWS; // TODO: store also object normal map for object triplanar
    float3 triplanarWeights;

#ifdef SURFACE_GRADIENT
    // tangent basis for each UVSet - up to 4 for now
    float3 vertexTangentWS0, vertexBitangentWS0;
    float3 vertexTangentWS1, vertexBitangentWS1;
    float3 vertexTangentWS2, vertexBitangentWS2;
    float3 vertexTangentWS3, vertexBitangentWS3;
#endif
};

#ifdef SURFACE_GRADIENT
void GenerateLayerTexCoordBasisTB(FragInputs input, inout LayerTexCoord layerTexCoord)
{
    float3 vertexNormalWS = input.worldToTangent[2];

    layerTexCoord.vertexTangentWS0 = input.worldToTangent[0];
    layerTexCoord.vertexBitangentWS0 = input.worldToTangent[1];

    float3 dPdx = ddx_fine(input.positionRWS);
    float3 dPdy = ddy_fine(input.positionRWS);

    float3 sigmaX = dPdx - dot(dPdx, vertexNormalWS) * vertexNormalWS;
    float3 sigmaY = dPdy - dot(dPdy, vertexNormalWS) * vertexNormalWS;
    //float flipSign = dot(sigmaY, cross(vertexNormalWS, sigmaX) ) ? -1.0 : 1.0;
    float flipSign = dot(dPdy, cross(vertexNormalWS, dPdx)) < 0.0 ? -1.0 : 1.0; // gives same as the commented out line above

    // TODO: Optimize! The compiler will not be able to remove the tangent space that are not use because it can't know due to our UVMapping constant we use for both base and details
    // To solve this we should track which UVSet is use for normal mapping... Maybe not as simple as it sounds
    SurfaceGradientGenBasisTB(vertexNormalWS, sigmaX, sigmaY, flipSign, input.texCoord1.xy, layerTexCoord.vertexTangentWS1, layerTexCoord.vertexBitangentWS1);
    #if defined(_REQUIRE_UV2) || defined(_REQUIRE_UV3)
    SurfaceGradientGenBasisTB(vertexNormalWS, sigmaX, sigmaY, flipSign, input.texCoord2.xy, layerTexCoord.vertexTangentWS2, layerTexCoord.vertexBitangentWS2);
    #endif
    #if defined(_REQUIRE_UV3)
    SurfaceGradientGenBasisTB(vertexNormalWS, sigmaX, sigmaY, flipSign, input.texCoord3.xy, layerTexCoord.vertexTangentWS3, layerTexCoord.vertexBitangentWS3);
    #endif
}
#endif

#ifndef LAYERED_LIT_SHADER

// Want to use only one sampler for normalmap/bentnormalmap either we use OS or TS. And either we have normal map or bent normal or both.
#ifdef _NORMALMAP_TANGENT_SPACE
    #if defined(_NORMALMAP)
    #define SAMPLER_NORMALMAP_IDX sampler_NormalMap
    #elif defined(_BENTNORMALMAP)
    #define SAMPLER_NORMALMAP_IDX sampler_BentNormalMap
    #endif
#else
    #if defined(_NORMALMAP)
    #define SAMPLER_NORMALMAP_IDX sampler_NormalMapOS
    #elif defined(_BENTNORMALMAP)
    #define SAMPLER_NORMALMAP_IDX sampler_BentNormalMapOS
    #endif
#endif

#define SAMPLER_DETAILMAP_IDX sampler_DetailMap
#define SAMPLER_MASKMAP_IDX sampler_MaskMap
#define SAMPLER_HEIGHTMAP_IDX sampler_HeightMap

#define SAMPLER_SUBSURFACE_MASK_MAP_IDX sampler_SubsurfaceMaskMap
#define SAMPLER_THICKNESSMAP_IDX sampler_ThicknessMap

// include LitDataIndividualLayer to define GetSurfaceData
#define LAYER_INDEX 0
#define ADD_IDX(Name) Name
#define ADD_ZERO_IDX(Name) Name
#ifdef _NORMALMAP
#define _NORMALMAP_IDX
#endif
#ifdef _NORMALMAP_TANGENT_SPACE
#define _NORMALMAP_TANGENT_SPACE_IDX
#endif
#ifdef _DETAIL_MAP
#define _DETAIL_MAP_IDX
#endif
#ifdef _SUBSURFACE_MASK_MAP
#define _SUBSURFACE_MASK_MAP_IDX
#endif
#ifdef _THICKNESSMAP
#define _THICKNESSMAP_IDX
#endif
#ifdef _MASKMAP
#define _MASKMAP_IDX
#endif
#ifdef _BENTNORMALMAP
#define _BENTNORMALMAP_IDX
#endif
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDataIndividualLayer.hlsl"

// This maybe call directly by tessellation (domain) shader, thus all part regarding surface gradient must be done
// in function with FragInputs input as parameters
// layerTexCoord must have been initialize to 0 outside of this function
void GetLayerTexCoord(float2 texCoord0, float2 texCoord1, float2 texCoord2, float2 texCoord3,
                      float3 positionRWS, float3 vertexNormalWS, inout LayerTexCoord layerTexCoord)
{
    layerTexCoord.vertexNormalWS = vertexNormalWS;
    layerTexCoord.triplanarWeights = ComputeTriplanarWeights(vertexNormalWS);

    int mappingType = UV_MAPPING_UVSET;
#if defined(_MAPPING_PLANAR)
    mappingType = UV_MAPPING_PLANAR;
#elif defined(_MAPPING_TRIPLANAR)
    mappingType = UV_MAPPING_TRIPLANAR;
#endif

    // Be sure that the compiler is aware that we don't use UV1 to UV3 for main layer so it can optimize code
    ComputeLayerTexCoord(   texCoord0, texCoord1, texCoord2, texCoord3, _UVMappingMask, _UVDetailsMappingMask,
                            _BaseColorMap_ST.xy, _BaseColorMap_ST.zw, _DetailMap_ST.xy, _DetailMap_ST.zw, 1.0, _LinkDetailsWithBase,
                            positionRWS, _TexWorldScale,
                            mappingType, layerTexCoord);
}

// This is call only in this file
// layerTexCoord must have been initialize to 0 outside of this function
void GetLayerTexCoord(FragInputs input, inout LayerTexCoord layerTexCoord)
{
#ifdef SURFACE_GRADIENT
    GenerateLayerTexCoordBasisTB(input, layerTexCoord);
#endif

    GetLayerTexCoord(   input.texCoord0.xy, input.texCoord1.xy, input.texCoord2.xy, input.texCoord3.xy,
                        input.positionRWS, input.worldToTangent[2].xyz, layerTexCoord);
}

#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDataDisplacement.hlsl"
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitBuiltinData.hlsl"

// Return opacity
float ADD_IDX(GetSurfaceData1)(FragInputs input, LayerTexCoord layerTexCoord, out SurfaceData surfaceData, out float3 normalTS, out float3 bentNormalTS)
{
	float alpha = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_BaseColorMap), ADD_ZERO_IDX(sampler_BaseColorMap), ADD_IDX(layerTexCoord.base)).a * ADD_IDX(_BaseColor).a;

	// Perform alha test very early to save performance (a killed pixel will not sample textures)
#if defined(_ALPHATEST_ON) && !defined(LAYERED_LIT_SHADER)
	float alphaCutoff = _AlphaCutoff;
#ifdef CUTOFF_TRANSPARENT_DEPTH_PREPASS
	alphaCutoff = _AlphaCutoffPrepass;
#elif defined(CUTOFF_TRANSPARENT_DEPTH_POSTPASS)
	alphaCutoff = _AlphaCutoffPostpass;
#endif

#if SHADERPASS == SHADERPASS_SHADOWS 
	DoAlphaTest(alpha, _UseShadowThreshold ? _AlphaCutoffShadow : alphaCutoff);
#else
	DoAlphaTest(alpha, alphaCutoff);
#endif
#endif

	float3 detailNormalTS = float3(0.0, 0.0, 0.0);
	float detailMask = 0.0;
#ifdef _DETAIL_MAP_IDX
	detailMask = 1.0;
#ifdef _MASKMAP_IDX
	detailMask = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_MaskMap), SAMPLER_MASKMAP_IDX, ADD_IDX(layerTexCoord.base)).b;
#endif
	float2 detailAlbedoAndSmoothness = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_DetailMap), SAMPLER_DETAILMAP_IDX, ADD_IDX(layerTexCoord.details)).rb;
	float detailAlbedo = detailAlbedoAndSmoothness.r * 2.0 - 1.0;
	float detailSmoothness = detailAlbedoAndSmoothness.g * 2.0 - 1.0;
	// Resample the detail map but this time for the normal map. This call should be optimize by the compiler
	// We split both call due to trilinear mapping
	detailNormalTS = SAMPLE_UVMAPPING_NORMALMAP_AG(ADD_IDX(_DetailMap), SAMPLER_DETAILMAP_IDX, ADD_IDX(layerTexCoord.details), ADD_IDX(_DetailNormalScale));
#endif

	surfaceData.baseColor = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_BaseColorMap), ADD_ZERO_IDX(sampler_BaseColorMap), ADD_IDX(layerTexCoord.base)).rgb * ADD_IDX(_BaseColor).rgb;
#ifdef _DETAIL_MAP_IDX

	// Goal: we want the detail albedo map to be able to darken down to black and brighten up to white the surface albedo.
	// The scale control the speed of the gradient. We simply remap detailAlbedo from [0..1] to [-1..1] then perform a lerp to black or white
	// with a factor based on speed.
	// For base color we interpolate in sRGB space (approximate here as square) as it get a nicer perceptual gradient
	float albedoDetailSpeed = saturate(abs(detailAlbedo) * ADD_IDX(_DetailAlbedoScale));
	float3 baseColorOverlay = lerp(sqrt(surfaceData.baseColor), (detailAlbedo < 0.0) ? float3(0.0, 0.0, 0.0) : float3(1.0, 1.0, 1.0), albedoDetailSpeed * albedoDetailSpeed);
	baseColorOverlay *= baseColorOverlay;
	// Lerp with details mask
	surfaceData.baseColor = lerp(surfaceData.baseColor, saturate(baseColorOverlay), detailMask);
#endif

	surfaceData.specularOcclusion = 1.0; // Will be setup outside of this function

	surfaceData.normalWS = float3(0.0, 0.0, 0.0); // Need to init this to keep quiet the compiler, but this is overriden later (0, 0, 0) so if we forget to override the compiler may comply.
	surfaceData.geomNormalWS = float3(0.0, 0.0, 0.0); // Not used, just to keep compiler quiet.

	normalTS = ADD_IDX(GetNormalTS)(input, layerTexCoord, detailNormalTS, detailMask);
	bentNormalTS = ADD_IDX(GetBentNormalTS)(input, layerTexCoord, normalTS, detailNormalTS, detailMask);

#if defined(_MASKMAP_IDX)
	surfaceData.perceptualSmoothness = 0;// SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_MaskMap), SAMPLER_MASKMAP_IDX, ADD_IDX(layerTexCoord.base)).a;
	surfaceData.perceptualSmoothness = 0;// lerp(ADD_IDX(_SmoothnessRemapMin), ADD_IDX(_SmoothnessRemapMax), surfaceData.perceptualSmoothness);
#else
	surfaceData.perceptualSmoothness = 0;// ADD_IDX(_Smoothness);
#endif

#ifdef _DETAIL_MAP_IDX
	// See comment for baseColorOverlay
	float smoothnessDetailSpeed = saturate(abs(detailSmoothness) * ADD_IDX(_DetailSmoothnessScale));
	float smoothnessOverlay = lerp(surfaceData.perceptualSmoothness, (detailSmoothness < 0.0) ? 0.0 : 1.0, smoothnessDetailSpeed);
	// Lerp with details mask
	surfaceData.perceptualSmoothness = 0;// lerp(surfaceData.perceptualSmoothness, saturate(smoothnessOverlay), detailMask);
#endif

	// MaskMap is RGBA: Metallic, Ambient Occlusion (Optional), detail Mask (Optional), Smoothness
#ifdef _MASKMAP_IDX
	surfaceData.metallic = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_MaskMap), SAMPLER_MASKMAP_IDX, ADD_IDX(layerTexCoord.base)).r;
	surfaceData.ambientOcclusion = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_MaskMap), SAMPLER_MASKMAP_IDX, ADD_IDX(layerTexCoord.base)).g;
	surfaceData.ambientOcclusion = lerp(ADD_IDX(_AORemapMin), ADD_IDX(_AORemapMax), surfaceData.ambientOcclusion);
#else
	surfaceData.metallic = 1.0;
	surfaceData.ambientOcclusion = 1.0;
#endif
	surfaceData.metallic *= ADD_IDX(_Metallic);

	surfaceData.diffusionProfileHash = asuint(ADD_IDX(_DiffusionProfileHash));
	surfaceData.subsurfaceMask = ADD_IDX(_SubsurfaceMask);

#ifdef _SUBSURFACE_MASK_MAP_IDX
	surfaceData.subsurfaceMask *= SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_SubsurfaceMaskMap), SAMPLER_SUBSURFACE_MASK_MAP_IDX, ADD_IDX(layerTexCoord.base)).r;
#endif

#ifdef _THICKNESSMAP_IDX
	surfaceData.thickness = SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_ThicknessMap), SAMPLER_THICKNESSMAP_IDX, ADD_IDX(layerTexCoord.base)).r;
	surfaceData.thickness = ADD_IDX(_ThicknessRemap).x + ADD_IDX(_ThicknessRemap).y * surfaceData.thickness;
#else
	surfaceData.thickness = ADD_IDX(_Thickness);
#endif

	// This part of the code is not used in case of layered shader but we keep the same macro system for simplicity
#if !defined(LAYERED_LIT_SHADER)

	// These static material feature allow compile time optimization
	surfaceData.materialFeatures = MATERIALFEATUREFLAGS_LIT_STANDARD;

#ifdef _MATERIAL_FEATURE_SUBSURFACE_SCATTERING
	surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING;
#endif
#ifdef _MATERIAL_FEATURE_TRANSMISSION
	surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_TRANSMISSION;
#endif
#ifdef _MATERIAL_FEATURE_ANISOTROPY
	surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_ANISOTROPY;
#endif
#ifdef _MATERIAL_FEATURE_CLEAR_COAT
	surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_CLEAR_COAT;
#endif
#ifdef _MATERIAL_FEATURE_IRIDESCENCE
	surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_IRIDESCENCE;
#endif
#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
	surfaceData.materialFeatures |= MATERIALFEATUREFLAGS_LIT_SPECULAR_COLOR;
#endif

#ifdef _TANGENTMAP
#ifdef _NORMALMAP_TANGENT_SPACE_IDX // Normal and tangent use same space
	// Tangent space vectors always use only 2 channels.
	float3 tangentTS = UnpackNormalmapRGorAG(SAMPLE_UVMAPPING_TEXTURE2D(_TangentMap, sampler_TangentMap, layerTexCoord.base), 1.0);
	surfaceData.tangentWS = TransformTangentToWorld(tangentTS, input.worldToTangent);
#else // Object space
	// Note: There is no such a thing like triplanar with object space normal, so we call directly 2D function
	float3 tangentOS = UnpackNormalRGB(SAMPLE_TEXTURE2D(_TangentMapOS, sampler_TangentMapOS, layerTexCoord.base.uv), 1.0);
	surfaceData.tangentWS = TransformObjectToWorldNormal(tangentOS);
#endif
#else
	// Note we don't normalize tangentWS either with a tangentmap above or using the interpolated tangent from the TBN frame
	// as it will be normalized later with a call to Orthonormalize():
	surfaceData.tangentWS = input.worldToTangent[0].xyz; // The tangent is not normalize in worldToTangent for mikkt. TODO: Check if it expected that we normalize with Morten. Tag: SURFACE_GRADIENT
#endif

#ifdef _ANISOTROPYMAP
	surfaceData.anisotropy = SAMPLE_UVMAPPING_TEXTURE2D(_AnisotropyMap, sampler_AnisotropyMap, layerTexCoord.base).r;
#else
	surfaceData.anisotropy = 1.0;
#endif
	surfaceData.anisotropy *= ADD_IDX(_Anisotropy);

	surfaceData.specularColor = _SpecularColor.rgb;
#ifdef _SPECULARCOLORMAP
	surfaceData.specularColor *= SAMPLE_UVMAPPING_TEXTURE2D(_SpecularColorMap, sampler_SpecularColorMap, layerTexCoord.base).rgb;
#endif
#ifdef _MATERIAL_FEATURE_SPECULAR_COLOR
	// Require to have setup baseColor
	// Reproduce the energy conservation done in legacy Unity. Not ideal but better for compatibility and users can unchek it
	surfaceData.baseColor *= _EnergyConservingSpecularColor > 0.0 ? (1.0 - Max3(surfaceData.specularColor.r, surfaceData.specularColor.g, surfaceData.specularColor.b)) : 1.0;
#endif

#if HAS_REFRACTION
	if (_EnableSSRefraction)
	{
		surfaceData.ior = _Ior;
		surfaceData.transmittanceColor = _TransmittanceColor;
#ifdef _TRANSMITTANCECOLORMAP
		surfaceData.transmittanceColor *= SAMPLE_UVMAPPING_TEXTURE2D(_TransmittanceColorMap, sampler_TransmittanceColorMap, ADD_IDX(layerTexCoord.base)).rgb;
#endif

		surfaceData.atDistance = _ATDistance;
		// Thickness already defined with SSS (from both thickness and thicknessMap)
		surfaceData.thickness *= _ThicknessMultiplier;
		// Rough refraction don't use opacity. Instead we use opacity as a transmittance mask.
		surfaceData.transmittanceMask = (1.0 - alpha);
		alpha = 1.0;
	}
	else
	{
		surfaceData.ior = 1.0;
		surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
		surfaceData.atDistance = 1.0;
		surfaceData.transmittanceMask = 0.0;
		alpha = 1.0;
	}
#else
	surfaceData.ior = 1.0;
	surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
	surfaceData.atDistance = 1.0;
	surfaceData.transmittanceMask = 0.0;
#endif

#ifdef _MATERIAL_FEATURE_CLEAR_COAT
	surfaceData.coatMask = _CoatMask;
	// To shader feature for keyword to limit the variant
	surfaceData.coatMask *= SAMPLE_UVMAPPING_TEXTURE2D(ADD_IDX(_CoatMaskMap), ADD_ZERO_IDX(sampler_CoatMaskMap), ADD_IDX(layerTexCoord.base)).r;
#else
	surfaceData.coatMask = 0.0;
#endif

#ifdef _MATERIAL_FEATURE_IRIDESCENCE
#ifdef _IRIDESCENCE_THICKNESSMAP
	surfaceData.iridescenceThickness = SAMPLE_UVMAPPING_TEXTURE2D(_IridescenceThicknessMap, sampler_IridescenceThicknessMap, layerTexCoord.base).r;
	surfaceData.iridescenceThickness = _IridescenceThicknessRemap.x + _IridescenceThicknessRemap.y * surfaceData.iridescenceThickness;
#else
	surfaceData.iridescenceThickness = _IridescenceThickness;
#endif
	surfaceData.iridescenceMask = _IridescenceMask;
	surfaceData.iridescenceMask *= SAMPLE_UVMAPPING_TEXTURE2D(_IridescenceMaskMap, sampler_IridescenceMaskMap, layerTexCoord.base).r;
#else
	surfaceData.iridescenceThickness = 0.0;
	surfaceData.iridescenceMask = 0.0;
#endif

#else // #if !defined(LAYERED_LIT_SHADER)

	// Mandatory to setup value to keep compiler quiet

	// Layered shader material feature are define outside of this call
	surfaceData.materialFeatures = 0;

	// All these parameters are ignore as they are re-setup outside of the layers function
	// Note: any parameters set here must also be set in GetSurfaceAndBuiltinData() layer version
	surfaceData.tangentWS = float3(0.0, 0.0, 0.0);
	surfaceData.anisotropy = 0.0;
	surfaceData.specularColor = float3(0.0, 0.0, 0.0);
	surfaceData.iridescenceThickness = 0.0;
	surfaceData.iridescenceMask = 0.0;
	surfaceData.coatMask = 0.0;

	// Transparency
	surfaceData.ior = 1.0;
	surfaceData.transmittanceColor = float3(1.0, 1.0, 1.0);
	surfaceData.atDistance = 1000000.0;
	surfaceData.transmittanceMask = 0.0;

#endif // #if !defined(LAYERED_LIT_SHADER)

	return alpha;
}

void GetSurfaceAndBuiltinData(FragInputs input, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
{
#ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
    uint3 fadeMaskSeed = asuint((int3)(V * _ScreenSize.xyx)); // Quantize V to _ScreenSize values
    LODDitheringTransition(fadeMaskSeed, unity_LODFade.x);
#endif

#ifdef _DOUBLESIDED_ON
    float3 doubleSidedConstants = _DoubleSidedConstants.xyz;
#else
    float3 doubleSidedConstants = float3(1.0, 1.0, 1.0);
#endif

    ApplyDoubleSidedFlipOrMirror(input, doubleSidedConstants); // Apply double sided flip on the vertex normal

    LayerTexCoord layerTexCoord;
    ZERO_INITIALIZE(LayerTexCoord, layerTexCoord);
    GetLayerTexCoord(input, layerTexCoord);

    float depthOffset = ApplyPerPixelDisplacement(input, V, layerTexCoord);

#ifdef _DEPTHOFFSET_ON
    ApplyDepthOffsetPositionInput(V, depthOffset, GetViewForwardDir(), GetWorldToHClipMatrix(), posInput);
#endif

    // We perform the conversion to world of the normalTS outside of the GetSurfaceData
    // so it allow us to correctly deal with detail normal map and optimize the code for the layered shaders
    float3 normalTS;
    float3 bentNormalTS;
    float3 bentNormalWS;
    float alpha = GetSurfaceData1(input, layerTexCoord, surfaceData, normalTS, bentNormalTS);
    GetNormalWS(input, normalTS, surfaceData.normalWS, doubleSidedConstants);

    // Use bent normal to sample GI if available
#ifdef _BENTNORMALMAP
    GetNormalWS(input, bentNormalTS, bentNormalWS, doubleSidedConstants);
#else
    bentNormalWS = surfaceData.normalWS;
#endif

    surfaceData.geomNormalWS = input.worldToTangent[2];

    // By default we use the ambient occlusion with Tri-ace trick (apply outside) for specular occlusion.
    // If user provide bent normal then we process a better term
#if defined(_BENTNORMALMAP) && defined(_ENABLESPECULAROCCLUSION)
    // If we have bent normal and ambient occlusion, process a specular occlusion
    #ifdef SPECULAR_OCCLUSION_USE_SPTD
    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAOPivot(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToPerceptualRoughness(surfaceData.perceptualSmoothness));
    #else
    surfaceData.specularOcclusion = GetSpecularOcclusionFromBentAO(V, bentNormalWS, surfaceData.normalWS, surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
    #endif
#elif defined(_MASKMAP)
    surfaceData.specularOcclusion = GetSpecularOcclusionFromAmbientOcclusion(ClampNdotV(dot(surfaceData.normalWS, V)), surfaceData.ambientOcclusion, PerceptualSmoothnessToRoughness(surfaceData.perceptualSmoothness));
#else
    surfaceData.specularOcclusion = 1.0;
#endif

    // This is use with anisotropic material
    surfaceData.tangentWS = Orthonormalize(surfaceData.tangentWS, surfaceData.normalWS);

#if HAVE_DECALS
    if (_EnableDecals)
    {
        DecalSurfaceData decalSurfaceData = GetDecalSurfaceData(posInput, alpha);
        ApplyDecalToSurfaceData(decalSurfaceData, surfaceData);
    }
#endif

#ifdef _ENABLE_GEOMETRIC_SPECULAR_AA
    // Specular AA
	surfaceData.perceptualSmoothness = 0;// GeometricNormalFiltering(surfaceData.perceptualSmoothness, input.worldToTangent[2], _SpecularAAScreenSpaceVariance, _SpecularAAThreshold);
#endif

#if defined(DEBUG_DISPLAY)
    if (_DebugMipMapMode != DEBUGMIPMAPMODE_NONE)
    {
        surfaceData.baseColor = GetTextureDataDebug(_DebugMipMapMode, layerTexCoord.base.uv, _BaseColorMap, _BaseColorMap_TexelSize, _BaseColorMap_MipInfo, surfaceData.baseColor);
        surfaceData.metallic = 0;
    }

    // We need to call ApplyDebugToSurfaceData after filling the surfarcedata and before filling builtinData
    // as it can modify attribute use for static lighting
    ApplyDebugToSurfaceData(input.worldToTangent, surfaceData);
#endif

    // Caution: surfaceData must be fully initialize before calling GetBuiltinData
    GetBuiltinData(input, V, posInput, surfaceData, alpha, bentNormalWS, depthOffset, builtinData);
}

#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/Lit/LitDataMeshModification.hlsl"

#endif // #ifndef LAYERED_LIT_SHADER
