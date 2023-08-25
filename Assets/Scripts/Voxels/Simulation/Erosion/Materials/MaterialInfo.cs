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
        private float loosenessInverted;

        public void CalculateInverted()
        {
            if (looseness == 0)
            {
                loosenessInverted = float.MaxValue;
            }
            else
            {
                loosenessInverted = 1 / looseness;
            }
        }
    }
}
