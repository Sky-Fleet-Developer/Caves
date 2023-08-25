using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using World;

namespace Voxels.Simulation
{
    [System.Serializable]
    public class SimulationSpace
    {
        [SerializeField, InlineProperty] private BoundsInt bounds;
        public Dictionary<int3, Chunk> Chunks;

        public int3 GetMinimum()
        {
            Vector3Int value = bounds.min;
            return new int3(value.x, value.y, value.z);
        }
        
        public int3 GetMaximum()
        {
            Vector3Int value = bounds.max;
            return new int3(value.x, value.y, value.z);
        }
        
        public void Init(Dictionary<int3, Chunk> chunks)
        {
            Chunks = chunks.Where(x => bounds.Contains(new Vector3Int(x.Key.x, x.Key.y, x.Key.z)))
                .ToDictionary(k => k.Key, v => v.Value);
        }
    }
}