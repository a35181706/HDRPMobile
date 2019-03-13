Shader "Unlit/GPUParticle"
{
	Properties
	{
		_ColorLow("Color Slow Speed", Color) = (0, 0, 0.5, 1)
		_ColorHigh("Color High Speed", Color) = (1, 0, 0, 1)
		_HighSpeedValue("High speed Value", Range(0, 50)) = 25
	}
		SubShader
	{
		Tags { "RenderType" = "HDUnlitShader" "RenderPipeline" = "HDRenderPipeline"  }
		LOD 100

		Pass
		{
			Blend SrcAlpha One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		// make fog work
		#pragma multi_compile_fog

		#include "UnityCG.cginc"
		// 自定义的数据结构, 必须跟 c# 和 compute shader 里的结构体一致
		// Particle's data
		struct GPUParticle
		{
			float2 position;
			float2 velocity;
		};

	//从ComputeShader计算过来的buffer
	StructuredBuffer<GPUParticle> GPUParticles;

	struct appdata
	{
		float4 vertex : POSITION;
		float2 uv : TEXCOORD0;
	};

	struct v2f
	{
		float4 vertex : SV_POSITION;
		float4 color : COLOR;
		UNITY_FOG_COORDS(1)

	};

	uniform float4 _ColorLow;
	uniform float4 _ColorHigh;
	uniform float _HighSpeedValue;

	v2f  vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
	{
		v2f o;
		//取出GPUbuffer里的速度
		float speed = length(GPUParticles[instance_id].velocity);
		//然后做一下差值
		float lerpValue = clamp(speed / _HighSpeedValue, 0.0f, 1.0f);
		o.vertex = UnityObjectToClipPos(float4(GPUParticles[instance_id].position,0.0f, 1.0f));

		//颜色之间的差值
		o.color = lerp(_ColorLow, _ColorHigh, lerpValue);

		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}
	// v2f  vert(appdata v)
 //     {
 //         v2f o;
 //         o.vertex = UnityObjectToClipPos(v.vertex);
		  //o.color = 1;
 //         UNITY_TRANSFER_FOG(o,o.vertex);
 //         return o;
 //     }
	  fixed4 frag(v2f i) : SV_Target
	  {
		  // sample the texture
		  fixed4 col = i.color;
	  // apply fog
	  //UNITY_APPLY_FOG(i.fogCoord, col);
	  return col;
  }
  ENDCG
}
	}
}
