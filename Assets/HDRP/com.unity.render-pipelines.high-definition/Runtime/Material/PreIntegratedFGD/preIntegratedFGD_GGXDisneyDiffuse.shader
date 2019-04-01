Shader "Hidden/HDRP/preIntegratedFGD_GGXDisneyDiffuse"
{
    SubShader
    {
        Tags{ "RenderPipeline" = "HDRenderPipeline" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM

            #pragma editor_sync_compilation

            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5
            #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

            #include "Assets/HDRP/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Assets/HDRP/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
            #include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Material/PreIntegratedFGD/PreIntegratedFGD.cs.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texCoord   : TEXCOORD0;
            };

			// Ref: Listing 18 in "Moving Frostbite to PBR" + https://knarkowicz.wordpress.com/2014/12/27/analytical-dfg-term-for-ibl/
			real4 IntegrateGGXAndDisneyDiffuseFGD1(real NdotV, real roughness, uint sampleCount = 4096)
			{
				// Note that our LUT covers the full [0, 1] range.
				// Therefore, we don't really want to clamp NdotV here (else the lerp slope is wrong).
				// However, if NdotV is 0, the integral is 0, so that's not what we want, either.
				// Our runtime NdotV bias is quite large, so we use a smaller one here instead.
				NdotV = max(NdotV, FLT_EPS);
				real3 V = real3(sqrt(1 - NdotV * NdotV), 0, NdotV);
				real4 acc = real4(0.0, 0.0, 0.0, 0.0);

				real3x3 localToWorld = k_identity3x3;

				for (uint i = 0; i < sampleCount; ++i)
				{
					real2 u = Hammersley2d(i, sampleCount);

					real VdotH;
					real NdotL;
					real weightOverPdf;

					real3 L; // Unused
					ImportanceSampleGGX(u, V, localToWorld, roughness, NdotV,
						L, VdotH, NdotL, weightOverPdf);

					if (NdotL > 0.0)
					{
						// Integral{BSDF * <N,L> dw} =
						// Integral{(F0 + (1 - F0) * (1 - <V,H>)^5) * (BSDF / F) * <N,L> dw} =
						// (1 - F0) * Integral{(1 - <V,H>)^5 * (BSDF / F) * <N,L> dw} + F0 * Integral{(BSDF / F) * <N,L> dw}=
						// (1 - F0) * x + F0 * y = lerp(x, y, F0)

						acc.x += weightOverPdf * pow(1 - VdotH, 5);
						acc.y += weightOverPdf;
					}

					// for Disney we still use a Cosine importance sampling, true Disney importance sampling imply a look up table
					ImportanceSampleLambert(u, localToWorld, L, NdotL, weightOverPdf);

					if (NdotL > 0.0)
					{
						real LdotV = dot(L, V);
						real disneyDiffuse = DisneyDiffuseNoPI(NdotV, NdotL, LdotV, RoughnessToPerceptualRoughness(roughness));

						acc.z += disneyDiffuse * weightOverPdf;
					}
				}

				acc /= sampleCount;

				// Remap from the [0.5, 1.5] to the [0, 1] range.
				acc.z -= 0.5;

				return acc;
			}


            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.texCoord   = GetFullScreenTriangleTexCoord(input.vertexID);

                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                // We want the LUT to contain the entire [0, 1] range, without losing half a texel at each side.
                float2 coordLUT = RemapHalfTexelCoordTo01(input.texCoord, FGDTEXTURE_RESOLUTION);

                // The FGD texture is parametrized as follows:
                // X = sqrt(dot(N, V))
                // Y = perceptualRoughness
                // These coordinate sampling must match the decoding in GetPreIntegratedDFG in Lit.hlsl,
                // i.e here we use perceptualRoughness, must be the same in shader
                // Note: with this angular parametrization, the LUT is almost perfectly linear,
                // except for the grazing angle when (NdotV -> 0).
                float NdotV = coordLUT.x * coordLUT.x;
                float perceptualRoughness = coordLUT.y;

                // Pre integrate GGX with smithJoint visibility as well as DisneyDiffuse
                float4 preFGD = IntegrateGGXAndDisneyDiffuseFGD1(NdotV, PerceptualRoughnessToRoughness(perceptualRoughness));
                return float4(preFGD.xyz, 1.0);
            }

            ENDHLSL
        }
    }
    Fallback Off
}
