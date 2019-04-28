using UnityEngine;

namespace UnityEngine.Experimental.Rendering.HDPipeline
{
    /// <summary>
    /// GPUDataPack工具，用于将CPU的数据打包到GPU，以节约寄存器
    /// </summary>
    public class GPUDataPackedUtils
    {
        const float Epsilon = 0.000000001f;

        public static PackedLightData ToPackedLightData(LightData data)
        {
            PackedLightData outData = default(PackedLightData);
            outData.packedData1 = data.positionRWS;
            outData.packedData1.w = (float)data.lightLayers + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData2 = data.forward;
            outData.packedData2.w = data.lightDimmer;

            outData.packedData3 = data.right;
            outData.packedData3.w = data.volumetricLightDimmer;

            outData.packedData4 = data.up;
            outData.packedData4.w = data.angleScale;

            outData.packedData5 = data.color;
            outData.packedData5.w = data.angleOffset;

            outData.packedData6 = data.shadowMaskSelector;

            outData.packedData7.x = (float)(int)data.lightType + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData7.y = data.range;
            outData.packedData7.z = data.rangeAttenuationScale;
            outData.packedData7.w = data.rangeAttenuationBias;

            outData.packedData8.x = (float)data.cookieIndex + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData8.y = (float)data.tileCookie + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData8.z = (float)data.shadowIndex + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData8.w = (float)data.contactShadowIndex + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData9.x = data.shadowDimmer;
            outData.packedData9.y = data.volumetricShadowDimmer;
            outData.packedData9.z = (float)data.nonLightMappedOnly + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData9.w = data.minRoughness;

            outData.packedData10 = data.size;
            outData.packedData10.z = data.diffuseDimmer;
            outData.packedData10.w = data.specularDimmer;

            //not use
            outData.packedData11 = Vector4.zero;
            outData.packedData12 = Vector4.zero;
            outData.packedData13 = Vector4.zero;
            outData.packedData14 = Vector4.zero;

            return outData;
        }

        public static PackedLightData ToPackedLightData(EnvLightData data)
        {
            PackedLightData outData = default(PackedLightData);
            outData.packedData1 = data.capturePositionRWS;
            outData.packedData1.w = (float)data.lightLayers + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData2 = data.proxyForward;
            outData.packedData2.w = (int)data.influenceShapeType + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData3 = data.proxyRight;
            outData.packedData3.w = data.minProjectionDistance;

            outData.packedData4 = data.proxyUp;
            outData.packedData4.w = data.weight;

            outData.packedData5 = data.proxyExtents;
            outData.packedData5.w = data.multiplier;

            outData.packedData6 = data.influencePositionRWS;
            outData.packedData6.w = data.envIndex + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData7 = data.influenceForward;
            outData.packedData7.w = data.boxSideFadeNegative.x;

            outData.packedData8 = data.influenceUp;
            outData.packedData8.w = data.boxSideFadeNegative.y;

            outData.packedData9 = data.influenceRight;
            outData.packedData9.w = data.boxSideFadeNegative.z;

            outData.packedData10 = data.influenceExtents;
            outData.packedData10.w = data.boxSideFadePositive.x;

            outData.packedData11 = data.blendDistancePositive;
            outData.packedData11.w = data.boxSideFadePositive.y;

            outData.packedData12 = data.blendDistanceNegative;
            outData.packedData12.w = data.boxSideFadePositive.z;

            outData.packedData13 = data.blendNormalDistancePositive;
            outData.packedData13.w = 0;

            outData.packedData14 = data.blendNormalDistanceNegative;
            outData.packedData14.w = 0;

            return outData;
        }

        public static PackedLightData ToPackedLightData(DirectionalLightData data)
        {
            PackedLightData outData = default(PackedLightData);
            outData.packedData1 = data.positionRWS;
            outData.packedData1.w = (float)data.lightLayers + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData2 = data.forward;
            outData.packedData2.w = data.lightDimmer;

            outData.packedData3 = data.right;
            outData.packedData3.w = data.volumetricLightDimmer;

            outData.packedData4 = data.up;
            outData.packedData4.w = data.angleScale;

            outData.packedData5 = data.color;
            outData.packedData5.w = data.angleOffset;

            outData.packedData6 = data.shadowMaskSelector;

            outData.packedData7.x = (float)data.cookieIndex + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData7.y = (float)data.tileCookie + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData7.z = (float)data.shadowIndex + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData7.w = (float)data.contactShadowIndex + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData8.x = data.shadowDimmer;
            outData.packedData8.y = data.volumetricShadowDimmer;
            outData.packedData8.z = (float)data.nonLightMappedOnly + Epsilon; //防止1，转换为float变成0.999999999998;
            outData.packedData8.w = data.minRoughness;

            outData.packedData9.x = data.diffuseDimmer;
            outData.packedData9.y = data.specularDimmer;
            outData.packedData9.z = 0;
            outData.packedData9.w = 0;

            //not use
            outData.packedData10 = Vector4.zero;
            outData.packedData11 = Vector4.zero;
            outData.packedData12 = Vector4.zero;
            outData.packedData13 = Vector4.zero;
            outData.packedData14 = Vector4.zero;

            return outData;
        }

        public static DirectionalLightData UnPackedLightDataToDirectionalLightData(PackedLightData data)
        {
            DirectionalLightData outData = default(DirectionalLightData);
            outData.positionRWS = data.packedData1;
            outData.lightLayers = (uint)data.packedData1.w;

            outData.forward = data.packedData2;
            outData.lightDimmer = data.packedData2.w;

            outData.right = data.packedData3;
            outData.volumetricLightDimmer = data.packedData3.w;

            outData.up = data.packedData4;
            outData.angleScale = data.packedData4.w;

            outData.color = data.packedData5;
            outData.angleOffset = data.packedData5.w;

            outData.shadowMaskSelector = data.packedData6;

            outData.cookieIndex = (int)data.packedData7.x;
            outData.tileCookie = (int)data.packedData7.y;
            outData.shadowIndex = (int)data.packedData7.z;
            outData.contactShadowIndex = (int)data.packedData7.w;

            outData.shadowDimmer = data.packedData8.x;
            outData.volumetricShadowDimmer = data.packedData8.y;
            outData.nonLightMappedOnly = (int)data.packedData8.z;
            outData.minRoughness = data.packedData8.w;

            outData.diffuseDimmer = data.packedData9.x;
            outData.specularDimmer = data.packedData9.y;

            return outData;
        }

        public static EnvLightData UnPackedLightDataToEnvLightData(PackedLightData data)
        {
            EnvLightData outData = default(EnvLightData);
            outData.capturePositionRWS = data.packedData1;
            outData.lightLayers = (uint)data.packedData1.w;

            outData.proxyForward = data.packedData2;
            outData.influenceShapeType = (EnvShapeType)(int)data.packedData2.w;

            outData.proxyRight = data.packedData3;
            outData.minProjectionDistance = data.packedData3.w;

            outData.proxyUp = data.packedData4;
            outData.weight = data.packedData4.w;

            outData.proxyExtents = data.packedData5;
            outData.multiplier = data.packedData5.w;

            outData.influencePositionRWS = data.packedData6;
            outData.envIndex = (int)data.packedData6.w;

            outData.influenceForward = data.packedData7;
            outData.boxSideFadeNegative.x = data.packedData7.w;

            outData.influenceUp = data.packedData8;
            outData.boxSideFadeNegative.y = data.packedData8.w;

            outData.influenceRight = data.packedData9;
            outData.boxSideFadeNegative.z = data.packedData9.w;

            outData.influenceExtents = data.packedData10;
            outData.boxSideFadePositive.x = data.packedData10.w;

            outData.blendDistancePositive = data.packedData11;
            outData.boxSideFadePositive.y = data.packedData11.w;

            outData.blendDistanceNegative = data.packedData12;
            outData.boxSideFadePositive.z = data.packedData12.w;

            outData.blendNormalDistancePositive = data.packedData13;

            outData.blendNormalDistanceNegative = data.packedData14;

            return outData;
        }

        public static LightData UnPackedLightDataToLightData(PackedLightData data)
        {
            LightData outData = default(LightData);

            outData.positionRWS = data.packedData1;
            outData.lightLayers = (uint)data.packedData1.w;

            outData.forward = data.packedData2;
            outData.lightDimmer = data.packedData2.w;

            outData.right = data.packedData3;
            outData.volumetricLightDimmer = data.packedData3.w;

            outData.up = data.packedData4;
            outData.angleScale = data.packedData4.w;

            outData.color = data.packedData5;
            outData.angleOffset = data.packedData5.w;

            outData.shadowMaskSelector = data.packedData6;

            outData.lightType = (GPULightType)(int)data.packedData7.x;
            outData.range = data.packedData7.y;
            outData.rangeAttenuationScale = data.packedData7.z;
            outData.rangeAttenuationBias = data.packedData7.w;

            outData.cookieIndex = (int)data.packedData8.x;
            outData.tileCookie = (int)data.packedData8.y;
            outData.shadowIndex = (int)data.packedData8.z;
            outData.contactShadowIndex = (int)data.packedData8.w;

            outData.shadowDimmer = data.packedData9.x;
            outData.volumetricShadowDimmer = data.packedData9.y;
            outData.nonLightMappedOnly = (int)data.packedData9.z;
            outData.minRoughness = data.packedData9.w;

            outData.size = data.packedData10;
            outData.diffuseDimmer = data.packedData10.z;
            outData.specularDimmer = data.packedData10.w;

            return outData;
        }

        public static PackedShadowData ToPackedShadowData(HDShadowData data)
        {
            PackedShadowData outData = default(PackedShadowData);

            outData.packedData1 = data.rot0;
            outData.packedData1.w = data.edgeTolerance;

            outData.packedData2 = data.rot1;
            outData.packedData2.w = (float)data.flags + Epsilon; //防止1，转换为float变成0.999999999998;

            outData.packedData3 = data.rot2;
            outData.packedData3.w = data.shadowFilterParams0.x;

            outData.packedData4 = data.pos;
            outData.packedData4.w = data.shadowFilterParams0.y;

            outData.packedData5 = data.proj;

            outData.packedData6 = data.atlasOffset;
            outData.packedData6.z = data.shadowFilterParams0.z;
            outData.packedData6.w = data.shadowFilterParams0.w;

            outData.packedData7 = data.zBufferParam;

            outData.packedData8 = data.shadowMapSize;

            outData.packedData9 = data.viewBias;

            outData.packedData10 = data.normalBias;
            outData.packedData10.w = data._padding;

            outData.packedData11 = data.shadowToWorld.GetColumn(0);
            outData.packedData12 = data.shadowToWorld.GetColumn(1);
            outData.packedData13 = data.shadowToWorld.GetColumn(2);
            outData.packedData14 = data.shadowToWorld.GetColumn(3);

            return outData;
        }

        unsafe public static PackedShadowData ToPackedShadowData(HDDirectionalShadowData data)
        {
            PackedShadowData outData = default(PackedShadowData);

            outData.packedData1 = new Vector4(
                                            data.sphereCascades[4 * 0 + 0],
                                            data.sphereCascades[4 * 0 + 1],
                                            data.sphereCascades[4 * 0 + 2],
                                            data.sphereCascades[4 * 0 + 3]);

            outData.packedData2 = new Vector4(
                                data.sphereCascades[4 * 1 + 0],
                                data.sphereCascades[4 * 1 + 1],
                                data.sphereCascades[4 * 1 + 2],
                                data.sphereCascades[4 * 1 + 3]);

            outData.packedData3 = new Vector4(
                    data.sphereCascades[4 * 2 + 0],
                    data.sphereCascades[4 * 2 + 1],
                    data.sphereCascades[4 * 2 + 2],
                    data.sphereCascades[4 * 2 + 3]);


            outData.packedData4 = new Vector4(
                    data.sphereCascades[4 * 3 + 0],
                    data.sphereCascades[4 * 3 + 1],
                    data.sphereCascades[4 * 3 + 2],
                    data.sphereCascades[4 * 3 + 3]);

            outData.packedData5 = data.cascadeDirection;

            outData.packedData6 = new Vector4(
                    data.cascadeBorders[4 * 0 + 0],
                    data.cascadeBorders[4 * 0 + 1],
                    data.cascadeBorders[4 * 0 + 2],
                    data.cascadeBorders[4 * 0 + 3]);

            return outData;
        }

        unsafe public static HDDirectionalShadowData UnPackedShadowDataToDirectionShadowData(PackedShadowData data)
        {
            HDDirectionalShadowData outData = default(HDDirectionalShadowData);

            outData.sphereCascades[4 * 0 + 0] = data.packedData1.x;
            outData.sphereCascades[4 * 0 + 1] = data.packedData1.y;
            outData.sphereCascades[4 * 0 + 2] = data.packedData1.z;
            outData.sphereCascades[4 * 0 + 3] = data.packedData1.w;

            outData.sphereCascades[4 * 1 + 0] = data.packedData2.x;
            outData.sphereCascades[4 * 1 + 1] = data.packedData2.y;
            outData.sphereCascades[4 * 1 + 2] = data.packedData2.z;
            outData.sphereCascades[4 * 1 + 3] = data.packedData2.w;


            outData.sphereCascades[4 * 2 + 0] = data.packedData3.x;
            outData.sphereCascades[4 * 2 + 1] = data.packedData3.y;
            outData.sphereCascades[4 * 2 + 2] = data.packedData3.z;
            outData.sphereCascades[4 * 2 + 3] = data.packedData3.w;

            outData.sphereCascades[4 * 3 + 0] = data.packedData4.x;
            outData.sphereCascades[4 * 3 + 1] = data.packedData4.y;
            outData.sphereCascades[4 * 3 + 2] = data.packedData4.z;
            outData.sphereCascades[4 * 3 + 3] = data.packedData4.w;

            outData.cascadeDirection = data.packedData5;

            outData.cascadeBorders[4 * 0 + 0] = data.packedData6.x;
            outData.cascadeBorders[4 * 0 + 1] = data.packedData6.y;
            outData.cascadeBorders[4 * 0 + 2] = data.packedData6.z;
            outData.cascadeBorders[4 * 0 + 3] = data.packedData6.w;

            return outData;
        }

        public static HDShadowData UnPackedShadowDataToShadowData(PackedShadowData data)
        {
            HDShadowData outData = default(HDShadowData);

            outData.rot0 = data.packedData1;
            outData.edgeTolerance = data.packedData1.w;

            outData.rot1 = data.packedData2;
            outData.flags = (int)data.packedData2.w;

            outData.rot2 = data.packedData3;
            outData.shadowFilterParams0.x = data.packedData3.w;

            outData.pos = data.packedData4;
            outData.shadowFilterParams0.y = data.packedData4.w;

            outData.proj = data.packedData5;

            outData.atlasOffset = data.packedData6;
            outData.shadowFilterParams0.z = data.packedData6.z;
            outData.shadowFilterParams0.w = data.packedData6.w;

            outData.zBufferParam = data.packedData7;

            outData.shadowMapSize = data.packedData8;

            outData.viewBias = data.packedData9;

            outData.normalBias = data.packedData10.normalized;
            outData._padding = data.packedData10.w;

            outData.shadowToWorld.SetColumn(0,data.packedData11);
            outData.shadowToWorld.SetColumn(1, data.packedData12);
            outData.shadowToWorld.SetColumn(2, data.packedData13);
            outData.shadowToWorld.SetColumn(3, data.packedData14);

            return outData;
        }
    }

    
}
