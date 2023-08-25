#ifndef EROSION_GRADIENT
#define EROSION_GRADIENT
#include "Assets/Compute/Erosion/Include/ErosionVoxelBuffer.hlsl"
#include "Assets/Compute/Erosion/Include/MaterialConfig.hlsl"

float3 GetGradient(float value, uint materialIndex, uint3 coordinate)
{
    float3 gradient;
    float my_looseness = GetLooseness(value, materialIndex);
    for(int3 offset = int3(-1, -1, -1); offset.x <= 1; offset.x ++)
    {
        for(offset.y = -1; offset.y <= 1; offset.y ++)
        {
            for(offset.z = -1; offset.z <= 1; offset.z ++)
            {
                int3 coord = coordinate + offset;
                if(IsOutOfVoxelVolumeBounds(coord))
                {
                    continue;
                }
                Voxel voxel = GetVoxel(coord);
                float looseness = GetLooseness(voxel.GetValue(), voxel.materialIndex);
                gradient += offset * (looseness - my_looseness);
            }
        }
    }
    return gradient;
}

#endif
