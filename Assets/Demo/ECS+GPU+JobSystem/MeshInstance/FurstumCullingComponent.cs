using Unity.Mathematics;
using Unity.Entities;


public struct FurstumCulledComponent : IComponentData
{
}

public struct FurstumCullingComponent : IComponentData
{
    public float3 BoundingSphereCenter;
    public float BoundingSphereRadius;
    public float CullStatus;
}

