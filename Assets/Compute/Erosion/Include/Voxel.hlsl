#ifndef EROSION_VOXEL
#define EROSION_VOXEL
#include "Assets/Compute/Include/Packing.hlsl"

struct Voxel
{
    float4 valueAndGradient;
    uint materialIndex;
    float waterAmount;

    float GetValue()
    {
        return valueAndGradient.x;
    }

    void SetValue(float v)
    {
        valueAndGradient.x = v;
    }

    void SetGradient(float3 gradient)
    {
        valueAndGradient.yzw = gradient;
    }

    float3 GetGradient()
    {
        return valueAndGradient.yzw;
    }

    static Voxel Create()
    {
        Voxel voxel;
        voxel.valueAndGradient = 0.0f;
        voxel.materialIndex = 0;
        voxel.waterAmount = 0;
        return voxel;
    }
};

struct PackedVoxel
{
    uint valueAndMaterial;
    uint gradientXAndWater;
    uint gradientYZ;

    static PackedVoxel Create(uint valueAndMaterial, uint gradientXAndWater, uint gradientYZ)
    {
        PackedVoxel packed_voxel;
        packed_voxel.valueAndMaterial = valueAndMaterial;
        packed_voxel.gradientXAndWater = gradientXAndWater;
        packed_voxel.gradientYZ = gradientYZ;
        return packed_voxel;
    }
};

Voxel UnpackVoxel(PackedVoxel packed_voxel)
{
    Voxel voxel;

    float value = f16tof32(packed_voxel.valueAndMaterial);
    voxel.materialIndex = packed_voxel.valueAndMaterial >> 16;
    
    float2 unpacked_x_wetness = UnpackFloats(packed_voxel.gradientXAndWater);
    float2 unpacked_y_z = UnpackFloats(packed_voxel.gradientYZ);
    float3 gradient = float3(unpacked_x_wetness.x, unpacked_y_z.x, unpacked_y_z.y);
    voxel.valueAndGradient = float4(value, gradient);
    voxel.waterAmount = unpacked_x_wetness.y;
    return voxel;
}

PackedVoxel PackVoxel(Voxel voxel)
{
    PackedVoxel packed_voxel;
    packed_voxel.valueAndMaterial = f32tof16(voxel.valueAndGradient.x) | voxel.materialIndex << 16;
    packed_voxel.gradientXAndWater = PackFloats(float2(voxel.valueAndGradient.y, voxel.waterAmount));
    packed_voxel.gradientYZ = PackFloats(voxel.valueAndGradient.zw);
    return packed_voxel;
}
#endif
