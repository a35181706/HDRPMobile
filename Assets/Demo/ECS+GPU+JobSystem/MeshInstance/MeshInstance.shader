// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Instanced/MeshInstance" 
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader
	{
			Tags { "RenderType" = "HDUnlitShader" "RenderPipeline" = "HDRenderPipeline"  }
			LOD 100
		Pass 
		{

			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#pragma multi_compile_instancing
			#pragma target 4.5

			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "AutoLight.cginc"

			sampler2D _MainTex;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv_MainTex : TEXCOORD0;
				float3 ambient : TEXCOORD1;
				float3 diffuse : TEXCOORD2;
				float3 color : TEXCOORD3;
				SHADOW_COORDS(4)
			};


			v2f vert(appdata_full v)
			{
				UNITY_SETUP_INSTANCE_ID(v);
				float3 worldNormal = v.normal;
				float3 ndotl = saturate(dot(worldNormal, _WorldSpaceLightPos0.xyz));
				float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
				float3 diffuse = (ndotl * _LightColor0.rgb);
				float3 color = v.color;

				v2f o;

				o.pos = UnityObjectToClipPos( v.vertex);
	
				o.uv_MainTex = v.texcoord;
				o.ambient = ambient;
				o.diffuse = diffuse;
				o.color = color;
				TRANSFER_SHADOW(o)
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed shadow = SHADOW_ATTENUATION(i);
				fixed4 albedo = tex2D(_MainTex, i.uv_MainTex);
				float3 lighting = i.diffuse * shadow + i.ambient;
				fixed4 output = fixed4(albedo.rgb * i.color * lighting, albedo.w);
				UNITY_APPLY_FOG(i.fogCoord, output);
				return output;
			}

			ENDCG
		}

		//Pass
		//{
		//	Name "ShadowCaster"
		//	Tags { "LightMode" = "ShadowCaster" }

		//	CGPROGRAM
		//	#pragma vertex vert
		//	#pragma fragment frag
		//	#pragma target 4.5
		//	#pragma multi_compile_shadowcaster
		//	#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
		//	#include "UnityCG.cginc"

		//	struct v2f {
		//		V2F_SHADOW_CASTER;
		//		UNITY_VERTEX_OUTPUT_STEREO
		//	};

		//	v2f vert(appdata_base v)
		//	{
		//		v2f o;
		//		UNITY_SETUP_INSTANCE_ID(v);
		//		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		//		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
		//		return o;
		//	}

		//	float4 frag(v2f i) : SV_Target
		//	{
		//		SHADOW_CASTER_FRAGMENT(i)
		//	}
		//	ENDCG
		//}
	}
}