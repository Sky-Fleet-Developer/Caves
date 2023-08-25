#ifndef SYNC_BUFFER
#define SYNC_BUFFER
#include "Assets/Compute/Erosion/Include/Voxel.hlsl"
#include "Assets/Compute/Voxels/Include/VoxelVolume.hlsl"

RWStructuredBuffer<PackedVoxel> voxel_volume_a;
RWStructuredBuffer<PackedVoxel> voxel_volume_b;

Voxel GetVoxelA(uint3 coordinate)
{
    return UnpackVoxel(voxel_volume_a[CalculateVoxelVolumeIndex(coordinate)]);
}
Voxel GetVoxelB(uint3 coordinate)
{
    return UnpackVoxel(voxel_volume_b[CalculateVoxelVolumeIndex(coordinate)]);
}

void SetVoxelA(uint3 coordinate, Voxel voxel)
{
    voxel_volume_a[CalculateVoxelVolumeIndex(coordinate)] = PackVoxel(voxel);
}

void SetVoxelB(uint3 coordinate, Voxel voxel)
{
    voxel_volume_b[CalculateVoxelVolumeIndex(coordinate)] = PackVoxel(voxel);
}

#endif