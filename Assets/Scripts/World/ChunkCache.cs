using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace World
{
    public class ChunkCache
    {
        private string m_path;

        public ChunkCache(string path, int3 coord)
        {
            m_path = string.Format(path, coord.x, coord.y, coord.z);
        }
        
        public static bool IsCacheExist(string path, int3 coord) => File.Exists(string.Format(path, coord.x, coord.y, coord.z));

        public void Write(ComputeBuffer buffer)
        {
            int size = buffer.count * Chunk.ElementsInVoxel;
            uint[] data = new uint[size];
            buffer.GetData(data);
        
            using (FileStream stream = File.Open(m_path, FileMode.OpenOrCreate))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(buffer.count);

                    for (var i = 0; i < data.Length; i++)
                    {
                        writer.Write(data[i]);
                    }
                }
            }
        }

        public void Read(ComputeBuffer buffer)
        {
            using (FileStream stream = File.Open(m_path, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int elementsCount = reader.ReadInt32();
                    
                    ulong[] data = new ulong[elementsCount]; // 2 ushort = 1 voxel
                    for (int i = 0; i < elementsCount; i++)
                    {
                        data[i] = reader.ReadUInt64();
                    }
                    
                    buffer.SetData(data);
                }
            }
        }
    }
}
