#ifndef TUNTENFISCH_VOXELS_VOXEL_BUFFER
#define TUNTENFISCH_VOXELS_VOXEL_BUFFER
#include "Assets/Compute/Voxels/Include/Voxel.hlsl"
#include "Assets/Compute/Voxels/Include/VoxelVolume.hlsl"

RWStructuredBuffer<PackedVoxel> voxelVolume;

Voxel GetVoxel(uint3 coordinate)
{
    return UnpackVoxel(voxelVolume[CalculateVoxelVolumeIndex(coordinate)]);
}

void SetVoxel(uint3 coordinate, Voxel voxel)
{
    voxelVolume[CalculateVoxelVolumeIndex(coordinate)] = PackVoxel(voxel);
}

#endif