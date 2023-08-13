#ifndef EROSION_BUFFER
#define EROSION_BUFFER
#include "Assets/Compute/Erosion/Include/Voxel.hlsl"
#include "Assets/Compute/Voxels/Include/VoxelVolume.hlsl"

StructuredBuffer<PackedVoxel> voxel_volume_read; 
RWStructuredBuffer<PackedVoxel> voxel_volume_write;

Voxel GetVoxel(uint3 coordinate)
{
    return UnpackVoxel(voxel_volume_read[CalculateVoxelVolumeIndex(coordinate)]);
}

void SetVoxel(uint3 coordinate, Voxel voxel)
{
    voxel_volume_write[CalculateVoxelVolumeIndex(coordinate)] = PackVoxel(voxel);
}

#endif