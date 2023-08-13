using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Voxels.Simulation.Erosion.Materials
{
    [CreateAssetMenu(fileName = "Material Properties Config", menuName = "Voxels/Material Properties Config")]
    public class MaterialPropertiesConfig : ScriptableObject
    {
        [SerializeField] private List<MaterialInfo> materialInfos;

        public ComputeBuffer GetConfigsBuffer()
        {
            ComputeBuffer buffer = new ComputeBuffer(materialInfos.Count, Marshal.SizeOf<MaterialInfo>());
            buffer.SetData(materialInfos.ToArray());
            return buffer;
        }
    }
}
