using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace Voxels.Simulation.Erosion
{
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct PackedVoxel
    {
        [FieldOffset(2)]
        public ushort material;
        [FieldOffset(0)]
        public ushort value;
        [FieldOffset(4)]
        public uint gradientXAndWetness;
        [FieldOffset(8)]
        public uint gradientYZ;
        //[FieldOffset(12)]
        //public float aligmnent;

        public PackedVoxel(ushort value, ushort material, float wetness, float3 gradient)
        {
            this.material = material;
            this.value = value;
            gradientXAndWetness = math.f32tof16(gradient.x) | math.f32tof16(wetness) << 16;
            gradientYZ = math.f32tof16(gradient.y) | math.f32tof16(gradient.z) << 16;
            //aligmnent = 0;
        }

        public float3 GetGradient()
        {
            return new float3(math.f16tof32(gradientXAndWetness), math.f16tof32(gradientYZ),
                math.f16tof32(gradientYZ >> 16));
        }

        public static int GetSize() => Marshal.SizeOf<PackedVoxel>();
    }
}