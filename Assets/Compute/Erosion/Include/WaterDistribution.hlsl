#ifndef WATER_DISTRIBUTION
#define WATER_DISTRIBUTION
#include "Assets/Compute/Erosion/Include/ErosionVoxelBuffer.hlsl"
#include "Assets/Compute/Erosion/Include/MaterialConfig.hlsl"

static const int3 neighbourOffset[] =
{
    int3(0, 0, 0),
    int3(1, 0, 0),
    int3(0, 1, 0),
    int3(0, 0, 1),
    int3(1, 1, 0),
    int3(0, 1, 1),
    int3(1, 0, 1),
    int3(1, 1, 1),
};

static const float3 neighbourDirection[] =
{
    float3(-0.5f, -0.5f, -0.5f),
    float3(0.5f, -0.5f, -0.5f),
    float3(-0.5f, 0.5f, -0.5f),
    float3(-0.5f,-0.5f, 0.5f),
    float3(0.5f, 0.5f, -0.5f),
    float3(-0.5f, 0.5f, 0.5f),
    float3(0.5f, -0.5f, 0.5f),
    float3(0.5f, 0.5f, 0.5f),
};

static const int neighboursCount = 8;
static const float3 gravityDirection = float3(0.0f, -0.5f, 0.0f);

struct NullableVoxel
{
    Voxel value;
    bool hasValue;

    static NullableVoxel Create(Voxel voxel)
    {
        NullableVoxel result;
        result.value = voxel;
        result.hasValue = true;
        return  result;
    }
    static NullableVoxel Null()
    {
        NullableVoxel result;
        result.hasValue = false;
        result.value = Voxel::Create();
        return result;
    }
};

void GetDistributionVoxels(uint3 voxelId, out NullableVoxel voxels[neighboursCount])
{
    for(int i = 0; i < neighboursCount; i++)
    {
        int3 coord = voxelId + neighbourOffset[i];
        if(IsOutOfVoxelVolumeBounds(coord))
        {
            voxels[i] = NullableVoxel::Null();
            continue;
        }
        voxels[i] = NullableVoxel::Create(GetVoxel(coord));
    }
}

float GetWetness(Voxel voxel)
{
    return min(voxel.waterAmount * GetLoosenessInverted(voxel.GetValue(), voxel.materialIndex), 1.1f);
}

float3 CalculateWaterGradient(NullableVoxel voxels[neighboursCount], out float waterSum)
{
    float3 result = 0.0f;

    waterSum = 0;    
    float wetnessSum = 0;    
    for(int i = 0; i < neighboursCount; i++)
    {
        if(voxels[i].hasValue)
        {
            waterSum += voxels[i].value.waterAmount;
            float wetness = GetWetness(voxels[i].value);
            wetnessSum += wetness;
            result += (float3)neighbourOffset[i] * wetness;
        }
    }
    result = 1.0f / wetnessSum;
    float3 delta = float3(0.5f, 0.5f, 0.5f) - result;
    return delta;
}

void DistributeWater(float deltaTime, float3 waterGradient, float waterSum, NullableVoxel voxels[neighboursCount])
{
    waterGradient += gravityDirection;
    for(int i = 0; i < neighboursCount; i++)
    {
        if(voxels[i].hasValue)
        {
            Voxel voxel = voxels[i].value;
            float wantedDelta = dot(waterGradient, neighbourDirection[i]) * waterSum * deltaTime;
            float maxCapacity = GetLooseness(voxel.GetValue(), voxel.materialIndex);
            float maxDelta = maxCapacity - voxel.waterAmount;
            float minDelta = -voxel.waterAmount;

            voxel.waterAmount += max(min(wantedDelta, maxDelta), minDelta);
            voxels[i].value.waterAmount = min(voxel.waterAmount, maxCapacity);
        }
    }
}

void SetDistributionVoxel(uint3 voxelId, NullableVoxel voxels[neighboursCount])
{
    for(int i = 0; i < neighboursCount; i++)
    {
        if(voxels[i].hasValue)
        {
            int3 coord = voxelId + neighbourOffset[i];

            /*Voxel oldVoxel = GetWrittenVoxel(coord);
            voxels[i].value.waterAmount += oldVoxel.waterAmount;
            voxels[i].value.waterAmount *= 0.5f;*/
            SetVoxel(coord, voxels[i].value);
        }
    }
}

#endif
