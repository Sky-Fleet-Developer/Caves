using System.Runtime.InteropServices;
using UnityEngine;

namespace Voxels.Simulation.Erosion.Materials
{
    [System.Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct MaterialInfo
    {
        [Range(0, 1)]
        public float looseness;
    }
}
