#if SHADERPASS != SHADERPASS_FORWARD
#error SHADERPASS_is_not_correctly_define
#endif

#ifdef _WRITE_TRANSPARENT_VELOCITY
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VelocityVertexShaderCommon.hlsl"

PackedVaryingsType Vert(AttributesMesh inputMesh, AttributesPass inputPass)
{
    VaryingsType varyingsType;
    varyingsType.vmesh = VertMesh(inputMesh);
    return VelocityVS(varyingsType, inputMesh, inputPass);
}

#ifdef TESSELLATION_ON

PackedVaryingsToPS VertTesselation(VaryingsToDS input)
{
    VaryingsToPS output;
    output.vmesh = VertMeshTesselation(input.vmesh);
    VelocityPositionZBias(output);

    output.vpass.positionCS = input.vpass.positionCS;
    output.vpass.previousPositionCS = input.vpass.previousPositionCS;

    return PackVaryingsToPS(output);
}

#endif // TESSELLATION_ON

#else // _WRITE_TRANSPARENT_VELOCITY

#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/VertMesh.hlsl"

// TODO: Here we will also have all the vertex deformation (GPU skinning, vertex animation, morph target...) or we will need to generate a compute shaders instead (better! but require work to deal with unpacking like fp16)
// Make it inout so that VelocityPass can get the modified input values later.
VaryingsMeshType VertMesh1(AttributesMesh input)
{
	VaryingsMeshType output;

	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);

#if defined(HAVE_MESH_MODIFICATION)
	input = ApplyMeshModification(input);
#endif

	// This return the camera relative position (if enable)
	float3 positionRWS = TransformObjectToWorld(input.positionOS);
#ifdef ATTRIBUTES_NEED_NORMAL
	float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
#else
	float3 normalWS = float3(0.0, 0.0, 0.0); // We need this case to be able to compile ApplyVertexModification that doesn't use normal.
#endif

#ifdef ATTRIBUTES_NEED_TANGENT
	float4 tangentWS = float4(TransformObjectToWorldDir(input.tangentOS.xyz), input.tangentOS.w);
#endif

//	// Do vertex modification in camera relative space (if enable)
//#if defined(HAVE_VERTEX_MODIFICATION)
//	ApplyVertexModification(input, normalWS, positionRWS, _Time);
//#endif

#ifdef TESSELLATION_ON
	output.positionRWS = positionRWS;
	output.normalWS = normalWS;
#if defined(VARYINGS_NEED_TANGENT_TO_WORLD) || defined(VARYINGS_DS_NEED_TANGENT)
	output.tangentWS = tangentWS;
#endif
#else
#ifdef VARYINGS_NEED_POSITION_WS
	output.positionRWS =  positionRWS;
#endif
	output.positionCS = TransformWorldToHClip(positionRWS);
#ifdef VARYINGS_NEED_TANGENT_TO_WORLD
	output.normalWS = normalWS ;
	output.tangentWS = tangentWS;
#endif
#endif

#if defined(VARYINGS_NEED_TEXCOORD0) || defined(VARYINGS_DS_NEED_TEXCOORD0)
	output.texCoord0 = input.uv0;
#endif
#if defined(VARYINGS_NEED_TEXCOORD1) || defined(VARYINGS_DS_NEED_TEXCOORD1)
	output.texCoord1 = input.uv1;
#endif
#if defined(VARYINGS_NEED_TEXCOORD2) || defined(VARYINGS_DS_NEED_TEXCOORD2)
	output.texCoord2 = input.uv2;
#endif
#if defined(VARYINGS_NEED_TEXCOORD3) || defined(VARYINGS_DS_NEED_TEXCOORD3)
	output.texCoord3 = input.uv3;
#endif
#if defined(VARYINGS_NEED_COLOR) || defined(VARYINGS_DS_NEED_COLOR)
	output.color = input.color;
#endif

	return output;
}

PackedVaryingsType Vert(AttributesMesh inputMesh)
{
    VaryingsType varyingsType;
    varyingsType.vmesh = VertMesh1(inputMesh);

    return PackVaryingsType(varyingsType);
}

#ifdef TESSELLATION_ON

PackedVaryingsToPS VertTesselation(VaryingsToDS input)
{
    VaryingsToPS output;
    output.vmesh = VertMeshTesselation(input.vmesh);

    return PackVaryingsToPS(output);
}


#endif // TESSELLATION_ON

#endif // _WRITE_TRANSPARENT_VELOCITY


#ifdef TESSELLATION_ON
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/TessellationShare.hlsl"
#endif

// For image based lighting, a part of the BSDF is pre-integrated.
// This is done both for specular GGX height-correlated and DisneyDiffuse
// reflectivity is  Integral{(BSDF_GGX / F) - use for multiscattering
void GetPreIntegratedFGDGGXAndDisneyDiffuse1(FragInputs input, float NdotV, float perceptualRoughness, float3 fresnel0, out float3 specularFGD, out float diffuseFGD, out float reflectivity)
{
	// We want the LUT to contain the entire [0, 1] range, without losing half a texel at each side.
	float2 coordLUT = Remap01ToHalfTexelCoord(float2(sqrt(NdotV), perceptualRoughness), FGDTEXTURE_RESOLUTION);
	
	float3 preFGD = SAMPLE_TEXTURE2D_LOD(_PreIntegratedFGD_GGXDisneyDiffuse, s_linear_clamp_sampler, coordLUT, 0).xyz;
//	float3 preFGD = SAMPLE_TEXTURE2D_LOD(_PreIntegratedFGD_GGXDisneyDiffuse, s_linear_clamp_sampler, input.texCoord0, 0).xyz;

	// Pre-integrate GGX FGD
	// Integral{BSDF * <N,L> dw} =
	// Integral{(F0 + (1 - F0) * (1 - <V,H>)^5) * (BSDF / F) * <N,L> dw} =
	// (1 - F0) * Integral{(1 - <V,H>)^5 * (BSDF / F) * <N,L> dw} + F0 * Integral{(BSDF / F) * <N,L> dw}=
	// (1 - F0) * x + F0 * y = lerp(x, y, F0)
	specularFGD = lerp(preFGD.xxx, preFGD.yyy, fresnel0);

	// Pre integrate DisneyDiffuse FGD:
	// z = DisneyDiffuse
	// Remap from the [0, 1] to the [0.5, 1.5] range.
	diffuseFGD = preFGD.z + 0.5;
	reflectivity = preFGD.y;
}

PreLightData GetPreLightData1(FragInputs input, float3 V, PositionInputs posInput, inout BSDFData bsdfData,float3 normal)
{
	PreLightData preLightData;
	ZERO_INITIALIZE(PreLightData, preLightData);

	float3 N = bsdfData.normalWS;
	preLightData.NdotV = dot(N, V);
	preLightData.iblPerceptualRoughness = bsdfData.perceptualRoughness;

	float NdotV = ClampNdotV(preLightData.NdotV);

	// We modify the bsdfData.fresnel0 here for iridescence
	if (HasFlag(bsdfData.materialFeatures, MATERIALFEATUREFLAGS_LIT_IRIDESCENCE))
	{
		float viewAngle = NdotV;
		float topIor = 1.0; // Default is air
		if (HasFlag(bsdfData.materialFeatures, MATERIALFEATUREFLAGS_LIT_CLEAR_COAT))
		{
			topIor = lerp(1.0, CLEAR_COAT_IOR, bsdfData.coatMask);
			// HACK: Use the reflected direction to specify the Fresnel coefficient for pre-convolved envmaps
			viewAngle = sqrt(1.0 + Sq(1.0 / topIor) * (Sq(dot(bsdfData.normalWS, V)) - 1.0));
		}

		if (bsdfData.iridescenceMask > 0.0)
		{
			bsdfData.fresnel0 = lerp(bsdfData.fresnel0, EvalIridescence(topIor, viewAngle, bsdfData.iridescenceThickness, bsdfData.fresnel0), bsdfData.iridescenceMask);
		}
	}

	// We modify the bsdfData.fresnel0 here for clearCoat
	if (HasFlag(bsdfData.materialFeatures, MATERIALFEATUREFLAGS_LIT_CLEAR_COAT))
	{
		// Fresnel0 is deduced from interface between air and material (Assume to be 1.5 in Unity, or a metal).
		// but here we go from clear coat (1.5) to material, we need to update fresnel0
		// Note: Schlick is a poor approximation of Fresnel when ieta is 1 (1.5 / 1.5), schlick target 1.4 to 2.2 IOR.
		bsdfData.fresnel0 = lerp(bsdfData.fresnel0, ConvertF0ForAirInterfaceToF0ForClearCoat15(bsdfData.fresnel0), bsdfData.coatMask);

		preLightData.coatPartLambdaV = GetSmithJointGGXPartLambdaV(NdotV, CLEAR_COAT_ROUGHNESS);
		preLightData.coatIblR = reflect(-V, N);
		preLightData.coatIblF = F_Schlick(CLEAR_COAT_F0, NdotV) * bsdfData.coatMask;
	}

	// Handle IBL + area light + multiscattering.
	// Note: use the not modified by anisotropy iblPerceptualRoughness here.
	float specularReflectivity;
	GetPreIntegratedFGDGGXAndDisneyDiffuse1(input, NdotV, preLightData.iblPerceptualRoughness, bsdfData.fresnel0, preLightData.specularFGD, preLightData.diffuseFGD, specularReflectivity);
#ifdef USE_DIFFUSE_LAMBERT_BRDF
	preLightData.diffuseFGD = 1.0;
#endif

#ifdef LIT_USE_GGX_ENERGY_COMPENSATION
	// Ref: Practical multiple scattering compensation for microfacet models.
	// We only apply the formulation for metals.
	// For dielectrics, the change of reflectance is negligible.
	// We deem the intensity difference of a couple of percent for high values of roughness
	// to not be worth the cost of another precomputed table.
	// Note: this formulation bakes the BSDF non-symmetric!
	preLightData.energyCompensation = 1.0 / specularReflectivity - 1.0;
#else
	preLightData.energyCompensation = 0.0;
#endif // LIT_USE_GGX_ENERGY_COMPENSATION

	float3 iblN;

	// We avoid divergent evaluation of the GGX, as that nearly doubles the cost.
	// If the tile has anisotropy, all the pixels within the tile are evaluated as anisotropic.
	if (HasFlag(bsdfData.materialFeatures, MATERIALFEATUREFLAGS_LIT_ANISOTROPY))
	{
		float TdotV = dot(bsdfData.tangentWS, V);
		float BdotV = dot(bsdfData.bitangentWS, V);

		preLightData.partLambdaV = GetSmithJointGGXAnisoPartLambdaV(TdotV, BdotV, NdotV, bsdfData.roughnessT, bsdfData.roughnessB);

		// perceptualRoughness is use as input and output here
		GetGGXAnisotropicModifiedNormalAndRoughness(bsdfData.bitangentWS, bsdfData.tangentWS, N, V, bsdfData.anisotropy, preLightData.iblPerceptualRoughness, iblN, preLightData.iblPerceptualRoughness);
	}
	else
	{
		preLightData.partLambdaV = GetSmithJointGGXPartLambdaV(NdotV, bsdfData.roughnessT);
		iblN = N;
	}

	preLightData.iblR = reflect(-V, iblN);

	// Area light
	// UVs for sampling the LUTs
	float theta = FastACosPos(NdotV); // For Area light - UVs for sampling the LUTs
	float2 uv = Remap01ToHalfTexelCoord(float2(bsdfData.perceptualRoughness, theta * INV_HALF_PI), LTC_LUT_SIZE);

	// Note we load the matrix transpose (avoid to have to transpose it in shader)
#ifdef USE_DIFFUSE_LAMBERT_BRDF
	preLightData.ltcTransformDiffuse = k_identity3x3;
#else
	// Get the inverse LTC matrix for Disney Diffuse
	preLightData.ltcTransformDiffuse = 0.0;
	preLightData.ltcTransformDiffuse._m22 = 1.0;
	preLightData.ltcTransformDiffuse._m00_m02_m11_m20 = SAMPLE_TEXTURE2D_ARRAY_LOD(_LtcData, s_linear_clamp_sampler, uv, LTC_DISNEY_DIFFUSE_MATRIX_INDEX, 0);
#endif

	// Get the inverse LTC matrix for GGX
	// Note we load the matrix transpose (avoid to have to transpose it in shader)
	preLightData.ltcTransformSpecular = 0.0;
	preLightData.ltcTransformSpecular._m22 = 1.0;
	preLightData.ltcTransformSpecular._m00_m02_m11_m20 = SAMPLE_TEXTURE2D_ARRAY_LOD(_LtcData, s_linear_clamp_sampler, uv, LTC_GGX_MATRIX_INDEX, 0);

	// Construct a right-handed view-dependent orthogonal basis around the normal
	preLightData.orthoBasisViewNormal = GetOrthoBasisViewNormal(V, N, preLightData.NdotV);

	preLightData.ltcTransformCoat = 0.0;
	if (HasFlag(bsdfData.materialFeatures, MATERIALFEATUREFLAGS_LIT_CLEAR_COAT))
	{
		float2 uv = LTC_LUT_OFFSET + LTC_LUT_SCALE * float2(CLEAR_COAT_PERCEPTUAL_ROUGHNESS, theta * INV_HALF_PI);

		// Get the inverse LTC matrix for GGX
		// Note we load the matrix transpose (avoid to have to transpose it in shader)
		preLightData.ltcTransformCoat._m22 = 1.0;
		preLightData.ltcTransformCoat._m00_m02_m11_m20 = SAMPLE_TEXTURE2D_ARRAY_LOD(_LtcData, s_linear_clamp_sampler, uv, LTC_GGX_MATRIX_INDEX, 0);
	}

	// refraction (forward only)
#if HAS_REFRACTION
	RefractionModelResult refraction = REFRACTION_MODEL(V, posInput, bsdfData);
	preLightData.transparentRefractV = refraction.rayWS;
	preLightData.transparentPositionWS = refraction.positionWS;
	preLightData.transparentTransmittance = exp(-bsdfData.absorptionCoefficient * refraction.dist);
	// Empirical remap to try to match a bit the refraction probe blurring for the fallback
	// Use IblPerceptualRoughness so we can handle approx of clear coat.
	preLightData.transparentSSMipLevel = PositivePow(preLightData.iblPerceptualRoughness, 1.3) * uint(max(_ColorPyramidScale.z - 1, 0));
#endif

	return preLightData;
}

// This function allow to modify the content of (back) baked diffuse lighting when we gather builtinData
// This is use to apply lighting model specific code, like pre-integration, transmission etc...
// It is up to the lighting model implementer to chose if the modification are apply here or in PostEvaluateBSDF
void ModifyBakedDiffuseLighting1(FragInputs input, float3 V, PositionInputs posInput, SurfaceData surfaceData, inout BuiltinData builtinData)
{
	// In case of deferred, all lighting model operation are done before storage in GBuffer, as we store emissive with bakeDiffuseLighting

	// To get the data we need to do the whole process - compiler should optimize everything
	BSDFData bsdfData = ConvertSurfaceDataToBSDFData(posInput.positionSS, surfaceData);
	PreLightData preLightData = GetPreLightData1(input,V, posInput, bsdfData, surfaceData.normalWS);

	// Add GI transmission contribution to bakeDiffuseLighting, we then drop backBakeDiffuseLighting (i.e it is not used anymore, this save VGPR in forward and in deferred we can't store it anyway)
	if (HasFlag(bsdfData.materialFeatures, MATERIALFEATUREFLAGS_LIT_TRANSMISSION))
	{
		builtinData.bakeDiffuseLighting += builtinData.backBakeDiffuseLighting * bsdfData.transmittance;
	}

	// For SSS we need to take into account the state of diffuseColor 
	if (HasFlag(bsdfData.materialFeatures, MATERIALFEATUREFLAGS_LIT_SUBSURFACE_SCATTERING))
	{
		bsdfData.diffuseColor = GetModifiedDiffuseColorForSSS(bsdfData);
	}

	// Premultiply (back) bake diffuse lighting information with DisneyDiffuse pre-integration
	// Note: When baking reflection probes, we approximate the diffuse with the fresnel0
	builtinData.bakeDiffuseLighting *= ReplaceDiffuseForReflectionPass(bsdfData.fresnel0)
		? bsdfData.fresnel0 : preLightData.diffuseFGD * bsdfData.diffuseColor;
}

// InitBuiltinData must be call before calling PostInitBuiltinData
void PostInitBuiltinData1(FragInputs input, float3 V, PositionInputs posInput, SurfaceData surfaceData,
	inout BuiltinData builtinData)
{
	// Apply control from the indirect lighting volume settings - This is apply here so we don't affect emissive
	// color in case of lit deferred for example and avoid material to have to deal with it
	builtinData.bakeDiffuseLighting *= _IndirectLightingMultiplier.x;
	builtinData.backBakeDiffuseLighting *= _IndirectLightingMultiplier.x;
#ifdef MODIFY_BAKED_DIFFUSE_LIGHTING
	ModifyBakedDiffuseLighting1(input,V, posInput, surfaceData, builtinData);
#endif
	ApplyDebugToBuiltinData(builtinData);
}

// Ref: "Efficient Evaluation of Irradiance Environment Maps" from ShaderX 2
real3 SHEvalLinearL0L11(real3 N, real4 shAr, real4 shAg, real4 shAb)
{
	real4 vA = real4(N, 1.0);

	real3 x1;
	// Linear (L1) + constant (L0) polynomial terms
	x1.r = dot(shAr, vA);
	x1.g = dot(shAg, vA);
	x1.b = dot(shAb, vA);

	return x1;
}

real3 SHEvalLinearL21(real3 N, real4 shBr, real4 shBg, real4 shBb, real4 shC)
{
	real3 x2;
	// 4 of the quadratic (L2) polynomials
	real4 vB = N.xyzz * N.yzzx;

	x2.r = dot(shBr, vB);
	x2.g = dot(shBg, vB);
	x2.b = dot(shBb, vB);

	// Final (5th) quadratic (L2) polynomial
	real vC = N.x * N.x - N.y * N.y;
	real3 x3 = shC.rgb * vC;

	return x2 + x3;
}


#if HAS_HALF
half3 SampleSH91(half4 SHCoefficients[7], half3 N)
{
	half4 shAr = SHCoefficients[0];
	half4 shAg = SHCoefficients[1];
	half4 shAb = SHCoefficients[2];
	half4 shBr = SHCoefficients[3];
	half4 shBg = SHCoefficients[4];
	half4 shBb = SHCoefficients[5];
	half4 shCr = SHCoefficients[6];

	// Linear + constant polynomial terms
	half3 res = SHEvalLinearL0L11(N, shAr, shAg, shAb);

	// Quadratic polynomials
	res += SHEvalLinearL21(N, shBr, shBg, shBb, shCr);

	return res;
}
#endif
float3 SampleSH91(float4 SHCoefficients[7], float3 N)
{
	float4 shAr = SHCoefficients[0];
	float4 shAg = SHCoefficients[1];
	float4 shAb = SHCoefficients[2];
	float4 shBr = SHCoefficients[3];
	float4 shBg = SHCoefficients[4];
	float4 shBb = SHCoefficients[5];
	float4 shCr = SHCoefficients[6];

	// Linear + constant polynomial terms
	float3 res = SHEvalLinearL0L11(N, shAr, shAg, shAb);
	
	res += SHEvalLinearL21(N, shBr, shBg, shBb, shCr);

	return res;	// Quadratic polynomials
}

// In unity we can have a mix of fully baked lightmap (static lightmap) + enlighten realtime lightmap (dynamic lightmap)
// for each case we can have directional lightmap or not.
// Else we have lightprobe for dynamic/moving entity. Either SH9 per object lightprobe or SH4 per pixel per object volume probe
float3 SampleBakedGI1(float3 positionRWS, float3 normalWS, float2 uvStaticLightmap, float2 uvDynamicLightmap)
{
	// If there is no lightmap, it assume lightprobe
#if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)


	if (unity_ProbeVolumeParams.x == 0.0)
	{
		// TODO: pass a tab of coefficient instead!
		real4 SHCoefficients[7];

		SHCoefficients[0] = unity_SHAr;
		SHCoefficients[1] = unity_SHAg;
		SHCoefficients[2] = unity_SHAb;
		SHCoefficients[3] = unity_SHBr;
		SHCoefficients[4] = unity_SHBg;
		SHCoefficients[5] = unity_SHBb;
		SHCoefficients[6] = unity_SHC;	
	

		return SampleSH91(SHCoefficients, normalWS); 
	}
	else
	{
	
#if SHADEROPTIONS_RAYTRACING
		if (unity_ProbeVolumeParams.w == 1.0)
			return SampleProbeVolumeSH9(TEXTURE2D_ARGS(unity_ProbeVolumeSH, samplerunity_ProbeVolumeSH), positionRWS, normalWS, GetProbeVolumeWorldToObject(),
				unity_ProbeVolumeParams.y, unity_ProbeVolumeParams.z, unity_ProbeVolumeMin.xyz, unity_ProbeVolumeSizeInv.xyz);
		else
#endif
			return SampleProbeVolumeSH4(TEXTURE2D_ARGS(unity_ProbeVolumeSH, samplerunity_ProbeVolumeSH), positionRWS, normalWS, GetProbeVolumeWorldToObject(),
				unity_ProbeVolumeParams.y, unity_ProbeVolumeParams.z, unity_ProbeVolumeMin.xyz, unity_ProbeVolumeSizeInv.xyz);
	}

#else

	float3 bakeDiffuseLighting = float3(0.0, 0.0, 0.0);

#ifdef UNITY_LIGHTMAP_FULL_HDR
	bool useRGBMLightmap = false;
	float4 decodeInstructions = float4(0.0, 0.0, 0.0, 0.0); // Never used but needed for the interface since it supports gamma lightmaps
#else
	bool useRGBMLightmap = true;
#if defined(UNITY_LIGHTMAP_RGBM_ENCODING)
	float4 decodeInstructions = float4(34.493242, 2.2, 0.0, 0.0); // range^2.2 = 5^2.2, gamma = 2.2
#else
	float4 decodeInstructions = float4(2.0, 2.2, 0.0, 0.0); // range = 2.0^2.2 = 4.59
#endif
#endif

#ifdef LIGHTMAP_ON
#ifdef DIRLIGHTMAP_COMBINED
	bakeDiffuseLighting += SampleDirectionalLightmap(TEXTURE2D_ARGS(unity_Lightmap, samplerunity_Lightmap),
		TEXTURE2D_ARGS(unity_LightmapInd, samplerunity_Lightmap),
		uvStaticLightmap, unity_LightmapST, normalWS, useRGBMLightmap, decodeInstructions);
#else
	bakeDiffuseLighting += SampleSingleLightmap(TEXTURE2D_ARGS(unity_Lightmap, samplerunity_Lightmap), uvStaticLightmap, unity_LightmapST, useRGBMLightmap, decodeInstructions);
#endif
#endif

#ifdef DYNAMICLIGHTMAP_ON
#ifdef DIRLIGHTMAP_COMBINED
	bakeDiffuseLighting += SampleDirectionalLightmap(TEXTURE2D_ARGS(unity_DynamicLightmap, samplerunity_DynamicLightmap),
		TEXTURE2D_ARGS(unity_DynamicDirectionality, samplerunity_DynamicLightmap),
		uvDynamicLightmap, unity_DynamicLightmapST, normalWS, false, decodeInstructions);
#else
	bakeDiffuseLighting += SampleSingleLightmap(TEXTURE2D_ARGS(unity_DynamicLightmap, samplerunity_DynamicLightmap), uvDynamicLightmap, unity_DynamicLightmapST, false, decodeInstructions);
#endif
#endif

	return bakeDiffuseLighting;

#endif
}

// For builtinData we want to allow the user to overwrite default GI in the surface shader / shader graph.
// So we perform the following order of operation:
// 1. InitBuiltinData - Init bakeDiffuseLighting and backBakeDiffuseLighting
// 2. User can overwrite these value in the surface shader / shader graph
// 3. PostInitBuiltinData - Handle debug mode + allow the current lighting model to update the data with ModifyBakedDiffuseLighting

// This method initialize BuiltinData usual values and after update of builtinData by the caller must be follow by PostInitBuiltinData
void InitBuiltinData1(PositionInputs posInput, float alpha, float3 normalWS, float3 backNormalWS, float4 texCoord1, float4 texCoord2,
	out BuiltinData builtinData)
{
	ZERO_INITIALIZE(BuiltinData, builtinData);

	builtinData.opacity = alpha;

#if SHADEROPTIONS_RAYTRACING && (SHADERPASS != SHADERPASS_RAYTRACING_INDIRECT) && (SHADERPASS != SHADERPASS_RAYTRACING_FORWARD)
	if (_RaytracedIndirectDiffuse == 1)
	{
		builtinData.bakeDiffuseLighting = LOAD_TEXTURE2D(_IndirectDiffuseTexture, posInput.positionSS).xyz;
	}
	else
#endif
		// Sample lightmap/lightprobe/volume proxy
		builtinData.bakeDiffuseLighting = SampleBakedGI1(posInput.positionWS, normalWS, texCoord1.xy, texCoord2.xy);
	// We also sample the back lighting in case we have transmission. If not use this will be optimize out by the compiler
	// For now simply recall the function with inverted normal, the compiler should be able to optimize the lightmap case to not resample the directional lightmap
	// however it may not optimize the lightprobe case due to the proxy volume relying on dynamic if (to verify), not a problem for SH9, but a problem for proxy volume.
	// TODO: optimize more this code.
	builtinData.backBakeDiffuseLighting = SampleBakedGI1(posInput.positionWS, backNormalWS, texCoord1.xy, texCoord2.xy);

#ifdef SHADOWS_SHADOWMASK
	float4 shadowMask = SampleShadowMask(posInput.positionWS, texCoord1.xy);
	builtinData.shadowMask0 = shadowMask.x;
	builtinData.shadowMask1 = shadowMask.y;
	builtinData.shadowMask2 = shadowMask.z;
	builtinData.shadowMask3 = shadowMask.w;
#endif

	// Use uniform directly - The float need to be cast to uint (as unity don't support to set a uint as uniform)
	builtinData.renderingLayers = _EnableLightLayers ? asuint(unity_RenderingLayer.x) : DEFAULT_LIGHT_LAYERS;
}

void GetBuiltinData1(FragInputs input, float3 V, inout PositionInputs posInput, SurfaceData surfaceData, float alpha, float3 bentNormalWS, float depthOffset, out BuiltinData builtinData)
{
	// For back lighting we use the oposite vertex normal
	InitBuiltinData1(posInput, alpha, bentNormalWS, -input.worldToTangent[2], input.texCoord1, input.texCoord2, builtinData);

	builtinData.emissiveColor = _EmissiveColor * lerp(float3(1.0, 1.0, 1.0), surfaceData.baseColor.rgb, _AlbedoAffectEmissive);
#ifdef _EMISSIVE_COLOR_MAP

	// Use layer0 of LayerTexCoord to retrieve emissive color mapping information
	LayerTexCoord layerTexCoord;
	ZERO_INITIALIZE(LayerTexCoord, layerTexCoord);
	layerTexCoord.vertexNormalWS = input.worldToTangent[2].xyz;
	layerTexCoord.triplanarWeights = ComputeTriplanarWeights(layerTexCoord.vertexNormalWS);

	int mappingType = UV_MAPPING_UVSET;
#if defined(_EMISSIVE_MAPPING_PLANAR)
	mappingType = UV_MAPPING_PLANAR;
#elif defined(_EMISSIVE_MAPPING_TRIPLANAR)
	mappingType = UV_MAPPING_TRIPLANAR;
#endif

	// Be sure that the compiler is aware that we don't use UV1 to UV3 for main layer so it can optimize code
#ifndef LAYERED_LIT_SHADER
	ComputeLayerTexCoord(
#else
	ComputeLayerTexCoord0(
#endif
		input.texCoord0.xy, input.texCoord1.xy, input.texCoord2.xy, input.texCoord3.xy, _UVMappingMaskEmissive, _UVMappingMaskEmissive,
		_EmissiveColorMap_ST.xy, _EmissiveColorMap_ST.zw, float2(0.0, 0.0), float2(0.0, 0.0), 1.0, false,
		input.positionRWS, _TexWorldScaleEmissive,
		mappingType, layerTexCoord);

#ifndef LAYERED_LIT_SHADER
	UVMapping emissiveMapMapping = layerTexCoord.base;
#else
	UVMapping emissiveMapMapping = layerTexCoord.base0;
#endif

	builtinData.emissiveColor *= SAMPLE_UVMAPPING_TEXTURE2D(_EmissiveColorMap, sampler_EmissiveColorMap, emissiveMapMapping).rgb;
#endif // _EMISSIVE_COLOR_MAP

	// Inverse pre-expose using _EmissiveExposureWeight weight
	float3 emissiveRcpExposure = builtinData.emissiveColor * GetInverseCurrentExposureMultiplier();
	builtinData.emissiveColor = lerp(emissiveRcpExposure, builtinData.emissiveColor, _EmissiveExposureWeight);

#if (SHADERPASS == SHADERPASS_DISTORTION) || defined(DEBUG_DISPLAY)
	float3 distortion = SAMPLE_TEXTURE2D(_DistortionVectorMap, sampler_DistortionVectorMap, input.texCoord0.xy).rgb;
	distortion.rg = distortion.rg * _DistortionVectorScale.xx + _DistortionVectorBias.xx;
	builtinData.distortion = distortion.rg * _DistortionScale;
	builtinData.distortionBlur = clamp(distortion.b * _DistortionBlurScale, 0.0, 1.0) * (_DistortionBlurRemapMax - _DistortionBlurRemapMin) + _DistortionBlurRemapMin;
#endif

	builtinData.depthOffset = depthOffset;

	PostInitBuiltinData1(input,V, posInput, surfaceData, builtinData);
}

real3 SurfaceGradientResolveNormal1(real3 nrmVertexNormal, real3 surfGrad)
{
	return normalize(nrmVertexNormal - surfGrad);
}


real3 TransformWorldToTangent1(real3 dirWS, real3x3 worldToTangent)
{
	return mul(worldToTangent, dirWS);
}

// This function convert the tangent space normal/tangent to world space and orthonormalize it + apply a correction of the normal if it is not pointing towards the near plane
void GetNormalWS1(FragInputs input, float3 normalTS, out float3 normalWS, float3 doubleSidedConstants)
{
#ifdef SURFACE_GRADIENT

#ifdef _DOUBLESIDED_ON
	// Flip the displacements (the entire surface gradient) in the 'flip normal' mode.
	float flipSign = input.isFrontFace ? 1.0 : doubleSidedConstants.x;
	normalTS *= flipSign;
#endif

	normalWS = SurfaceGradientResolveNormal1(input.worldToTangent[2], normalTS);

#else // SURFACE_GRADIENT

#ifdef _DOUBLESIDED_ON
	// Just flip the TB in the 'flip normal' mode. Conceptually wrong, but it works.
	float flipSign = input.isFrontFace ? 1.0 : doubleSidedConstants.x;
	input.worldToTangent[0] = flipSign * input.worldToTangent[0]; // tangent
	input.worldToTangent[1] = flipSign * input.worldToTangent[1]; // bitangent
#endif // _DOUBLESIDED_ON

	// We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
	normalWS = normalize(TransformTangentToWorld1(normalTS, input.worldToTangent));

#endif // SURFACE_GRADIENT

#if INV_WS_NORMAL_Z
	normalWS.z *= -1;
#endif

}


void GetSurfaceAndBuiltinData1(FragInputs input, float3 V, inout PositionInputs posInput, out SurfaceData surfaceData, out BuiltinData builtinData)
{
	ZERO_INITIALIZE(BuiltinData,builtinData);
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
	float alpha = GetSurfaceData(input, layerTexCoord, surfaceData, normalTS, bentNormalTS);
	GetNormalWS1(input, normalTS, surfaceData.normalWS, doubleSidedConstants);

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
	surfaceData.perceptualSmoothness = GeometricNormalFiltering(surfaceData.perceptualSmoothness, input.worldToTangent[2], _SpecularAAScreenSpaceVariance, _SpecularAAThreshold);
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
	GetBuiltinData1(input, V, posInput, surfaceData, alpha, bentNormalWS, depthOffset, builtinData);
}

// This function assumes the bitangent flip is encoded in tangentWS.w
float3x3 BuildWorldToTangent1(float4 tangentWS, float3 normalWS)
{
	// tangentWS must not be normalized (mikkts requirement)

	// Normalize normalWS vector but keep the renormFactor to apply it to bitangent and tangent
	float3 unnormalizedNormalWS = normalWS;
	float renormFactor = 1.0 / length(unnormalizedNormalWS);

	// bitangent on the fly option in xnormal to reduce vertex shader outputs.
	// this is the mikktspace transformation (must use unnormalized attributes)
	float3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);

	// surface gradient based formulation requires a unit length initial normal. We can maintain compliance with mikkts
	// by uniformly scaling all 3 vectors since normalization of the perturbed normal will cancel it.
	worldToTangent[0] = worldToTangent[0] * renormFactor;
	worldToTangent[1] = worldToTangent[1] * renormFactor;
	worldToTangent[2] = worldToTangent[2] * renormFactor;		// normalizes the interpolated vertex normal

	return worldToTangent;
}

FragInputs UnpackVaryingsMeshToFragInputs1(PackedVaryingsMeshToPS input)
{
	FragInputs output;
	ZERO_INITIALIZE(FragInputs, output);

	UNITY_SETUP_INSTANCE_ID(input);

	// Init to some default value to make the computer quiet (else it output "divide by zero" warning even if value is not used).
	// TODO: this is a really poor workaround, but the variable is used in a bunch of places
	// to compute normals which are then passed on elsewhere to compute other values...
	output.worldToTangent = k_identity3x3;

	output.positionSS = input.positionCS; // input.positionCS is SV_Position

#ifdef VARYINGS_NEED_POSITION_WS
	output.positionRWS.xyz = input.interpolators0.xyz;
#endif

#ifdef VARYINGS_NEED_TANGENT_TO_WORLD
	float4 tangentWS = float4(input.interpolators2.xyz, input.interpolators2.w > 0.0 ? 1.0 : -1.0); // must not be normalized (mikkts requirement)
	output.worldToTangent = BuildWorldToTangent1(tangentWS, input.interpolators1.xyz);
#endif // VARYINGS_NEED_TANGENT_TO_WORLD

#ifdef VARYINGS_NEED_TEXCOORD0
	output.texCoord0.xy = input.interpolators3.xy;
#endif
#ifdef VARYINGS_NEED_TEXCOORD1
	output.texCoord1.xy = input.interpolators3.zw;
#endif
#ifdef VARYINGS_NEED_TEXCOORD2
	output.texCoord2.xy = input.interpolators4.xy;
#endif
#ifdef VARYINGS_NEED_TEXCOORD3
	output.texCoord3.xy = input.interpolators4.zw;
#endif
#ifdef VARYINGS_NEED_COLOR
	output.color = input.interpolators5;
#endif

#if defined(VARYINGS_NEED_CULLFACE) && SHADER_STAGE_FRAGMENT
	output.isFrontFace = IS_FRONT_VFACE(input.cullFace, true, false);
#endif

	return output;
}

void Frag(PackedVaryingsToPS packedInput,
        #ifdef OUTPUT_SPLIT_LIGHTING
            out float4 outColor : SV_Target0,  // outSpecularLighting
            out float4 outDiffuseLighting : SV_Target1,
            OUTPUT_SSSBUFFER(outSSSBuffer)
        #else
            out float4 outColor : SV_Target0
        #ifdef _WRITE_TRANSPARENT_VELOCITY
          , out float4 outVelocity : SV_Target1
        #endif // _WRITE_TRANSPARENT_VELOCITY
        #endif // OUTPUT_SPLIT_LIGHTING
        #ifdef _DEPTHOFFSET_ON
            , out float outputDepth : SV_Depth
        #endif
          )
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(packedInput);
    FragInputs input = UnpackVaryingsMeshToFragInputs1(packedInput.vmesh);

	//outColor = SAMPLE_TEXTURE2D(_PreIntegratedFGD_GGXDisneyDiffuse, s_linear_clamp_sampler,input.texCoord0);
	//outColor = float4(outColor.z, outColor.z, outColor.z, 1);
	//return;
    uint2 tileIndex = uint2(input.positionSS.xy) / GetTileSize();
#if defined(UNITY_SINGLE_PASS_STEREO)
    tileIndex.x -= unity_StereoEyeIndex * _NumTileClusteredX;
#endif

    // input.positionSS is SV_Position
    PositionInputs posInput = GetPositionInput_Stereo(input.positionSS.xy, _ScreenSize.zw, input.positionSS.z, input.positionSS.w, input.positionRWS.xyz, tileIndex, unity_StereoEyeIndex);

#ifdef VARYINGS_NEED_POSITION_WS
    float3 V = GetWorldSpaceNormalizeViewDir(input.positionRWS);
#else
    // Unused
    float3 V = float3(1.0, 1.0, 1.0); // Avoid the division by 0
#endif

    SurfaceData surfaceData;
    BuiltinData builtinData;
    GetSurfaceAndBuiltinData1(input, V, posInput, surfaceData, builtinData);
	outColor = float4(builtinData.bakeDiffuseLighting,1);
	return;
    BSDFData bsdfData = ConvertSurfaceDataToBSDFData(input.positionSS.xy, surfaceData);

    PreLightData preLightData = GetPreLightData(V, posInput, bsdfData);

    outColor = float4(0.0, 0.0, 0.0, 0.0);

    // We need to skip lighting when doing debug pass because the debug pass is done before lighting so some buffers may not be properly initialized potentially causing crashes on PS4.
#ifdef DEBUG_DISPLAY
    // Init in debug display mode to quiet warning
    #ifdef OUTPUT_SPLIT_LIGHTING
    outDiffuseLighting = 0;
    ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
    #endif

    // Same code in ShaderPassForwardUnlit.shader
    if (_DebugViewMaterial != 0)
    {
        float3 result = float3(1.0, 0.0, 1.0);

        bool needLinearToSRGB = false;

        GetPropertiesDataDebug(_DebugViewMaterial, result, needLinearToSRGB);
        GetVaryingsDataDebug(_DebugViewMaterial, input, result, needLinearToSRGB);
        GetBuiltinDataDebug(_DebugViewMaterial, builtinData, result, needLinearToSRGB);
        GetSurfaceDataDebug(_DebugViewMaterial, surfaceData, result, needLinearToSRGB);
        GetBSDFDataDebug(_DebugViewMaterial, bsdfData, result, needLinearToSRGB);

        // TEMP!
        // For now, the final blit in the backbuffer performs an sRGB write
        // So in the meantime we apply the inverse transform to linear data to compensate.
        if (!needLinearToSRGB)
            result = SRGBToLinear(max(0, result));

        outColor = float4(result, 1.0);
    }
    else if (_DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_DIFFUSE_COLOR || _DebugFullScreenMode == FULLSCREENDEBUGMODE_VALIDATE_SPECULAR_COLOR)
    {
        float3 result = float3(0.0, 0.0, 0.0);

        GetPBRValidatorDebug(surfaceData, result);

        outColor = float4(result, 1.0f);
    }
    else
#endif
    {
#ifdef _SURFACE_TYPE_TRANSPARENT
        uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_TRANSPARENT;
#else
        uint featureFlags = LIGHT_FEATURE_MASK_FLAGS_OPAQUE;
#endif
        float3 diffuseLighting;
        float3 specularLighting;

        LightLoop1(V, posInput, preLightData, bsdfData, builtinData, featureFlags, diffuseLighting, specularLighting);
		outColor = float4(diffuseLighting, 1);
		return;
        diffuseLighting *= GetCurrentExposureMultiplier();
        specularLighting *= GetCurrentExposureMultiplier();

#ifdef OUTPUT_SPLIT_LIGHTING
        if (_EnableSubsurfaceScattering != 0 && ShouldOutputSplitLighting(bsdfData))
        {
            outColor = float4(specularLighting, 1.0);
            outDiffuseLighting = float4(TagLightingForSSS(diffuseLighting), 1.0);
        }
        else
        {
            outColor = float4(diffuseLighting + specularLighting, 1.0);
            outDiffuseLighting = 0;
        }
        ENCODE_INTO_SSSBUFFER(surfaceData, posInput.positionSS, outSSSBuffer);
#else
        outColor = ApplyBlendMode(diffuseLighting, specularLighting, builtinData.opacity);
        outColor = EvaluateAtmosphericScattering(posInput, V, outColor);
#endif
#ifdef _WRITE_TRANSPARENT_VELOCITY
        VaryingsPassToPS inputPass = UnpackVaryingsPassToPS(packedInput.vpass);
        bool forceNoMotion = any(unity_MotionVectorsParams.yw == 0.0);
        if (forceNoMotion)
        {
            outVelocity = float4(2.0, 0.0, 0.0, 0.0);
        }
        else
        {
            float2 velocity = CalculateVelocity(inputPass.positionCS, inputPass.previousPositionCS);
            EncodeVelocity(velocity * 0.5, outVelocity);
            outVelocity.zw = 1.0;
        }
#endif
    }

#ifdef _DEPTHOFFSET_ON
    outputDepth = posInput.deviceDepth;
#endif
}
