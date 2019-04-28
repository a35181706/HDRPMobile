#ifndef HD_SHADOW_CONTEXT_HLSL
#define HD_SHADOW_CONTEXT_HLSL

#include "Assets/HDRP/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/HDShadowManager.cs.hlsl"

#define USE_PACKED_SHADOWDATA // π”√pakcedshadow

#ifdef USE_PACKED_SHADOWDATA
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/PackedShadowData.cs.hlsl"

struct HDShadowContext
{
	StructuredBuffer<PackedShadowData>  shadowDatas;
	HDDirectionalShadowData         directionalShadowData;
};

StructuredBuffer<PackedShadowData> _PackedShadowData;

HDDirectionalShadowData UnPackedShadowDataToDirectionShadowData(PackedShadowData data)
{
	HDDirectionalShadowData outData;
	ZERO_INITIALIZE(HDDirectionalShadowData, outData);

	outData.sphereCascades[0] = data.packedData1;
	outData.sphereCascades[1] = data.packedData2;
	outData.sphereCascades[2] = data.packedData3;
	outData.sphereCascades[3] = data.packedData4;

	outData.cascadeDirection = data.packedData5;

	outData.cascadeBorders[4 * 0 + 0] = data.packedData6.x;
	outData.cascadeBorders[4 * 0 + 1] = data.packedData6.y;
	outData.cascadeBorders[4 * 0 + 2] = data.packedData6.z;
	outData.cascadeBorders[4 * 0 + 3] = data.packedData6.w;

	return outData;
}

HDShadowData UnPackedShadowDataToShadowData(PackedShadowData data)
{
	HDShadowData outData;
	ZERO_INITIALIZE(HDShadowData, outData);

	outData.rot0 = data.packedData1.xyz;
	outData.edgeTolerance = data.packedData1.w;

	outData.rot1 = data.packedData2.xyz;
	outData.flags = asint(data.packedData2.w);

	outData.rot2 = data.packedData3.xyz;
	outData.shadowFilterParams0.x = data.packedData3.w;

	outData.pos = data.packedData4.xyz;
	outData.shadowFilterParams0.y = data.packedData4.w;

	outData.proj = data.packedData5;

	outData.atlasOffset = data.packedData6.xy;
	outData.shadowFilterParams0.z = data.packedData6.z;
	outData.shadowFilterParams0.w = data.packedData6.w;

	outData.zBufferParam = data.packedData7;

	outData.shadowMapSize = data.packedData8;

	outData.viewBias = data.packedData9;

	outData.normalBias = data.packedData10.xyz;
	outData._padding = data.packedData10.w;

	outData.shadowToWorld[0] = data.packedData11;
	outData.shadowToWorld[1] = data.packedData12;
	outData.shadowToWorld[2] = data.packedData13;
	outData.shadowToWorld[3] = data.packedData14;

	return outData;
}

HDShadowData FetchHDShadow(int index)
{
	return UnPackedShadowDataToShadowData(_PackedShadowData[1 + index]);
}

HDDirectionalShadowData FetchDirectionalShadowData(int index) 
{
	return UnPackedShadowDataToDirectionShadowData(_PackedShadowData[0]); //always use 0
}

#else

struct HDShadowContext
{
	StructuredBuffer<HDShadowData>  shadowDatas;
	HDDirectionalShadowData         directionalShadowData;
};

StructuredBuffer<HDShadowData>              _HDShadowDatas;
// Only the first element is used since we only support one directional light
StructuredBuffer<HDDirectionalShadowData>   _HDDirectionalShadowData;

HDShadowData FetchHDShadow(int index)
{
	return _HDShadowDatas[index];
}

HDDirectionalShadowData FetchDirectionalShadowData(int index)
{
	return _HDDirectionalShadowData[0]; //always use 0
}

#endif //USE_PACKED_SHADOWDATA


// HD shadow sampling bindings
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/HDShadowSampling.hlsl"
#include "Assets/HDRP/com.unity.render-pipelines.high-definition/Runtime/Lighting/Shadow/HDShadowAlgorithms.hlsl"

TEXTURE2D(_ShadowmapAtlas);
TEXTURE2D(_ShadowmapCascadeAtlas);
TEXTURE2D(_AreaShadowmapAtlas);
TEXTURE2D(_AreaShadowmapMomentAtlas);


HDShadowContext InitShadowContext()
{
    HDShadowContext         sc;

#ifdef USE_PACKED_SHADOWDATA
	sc.shadowDatas = _PackedShadowData;
	sc.directionalShadowData = UnPackedShadowDataToDirectionShadowData(_PackedShadowData[0]);
#else
    sc.shadowDatas = _HDShadowDatas;
    sc.directionalShadowData = _HDDirectionalShadowData[0];
#endif

    return sc;
}

#endif // HD_SHADOW_CONTEXT_HLSL
