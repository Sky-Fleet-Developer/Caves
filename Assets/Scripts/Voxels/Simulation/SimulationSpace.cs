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
        
        public void Init(Dictionary<int3, Chunk> chunks)
        {
            Chunks = chunks.Where(x => bounds.Contains(new Vector3Int(x.Key.x, x.Key.y, x.Key.z)))
                .ToDictionary(k => k.Key, v => v.Value);
        }
    }
}