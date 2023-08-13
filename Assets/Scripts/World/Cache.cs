using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace World
{
    public class Cache
    {
        private string m_path;
        private readonly Dictionary<int3, ChunkCache> m_existChunks = new Dictionary<int3, ChunkCache>();
        public Cache(string path)
        {
            m_path = path;
        }

        public void Write(Chunk chunk, int3 coord)
        {
            if (!m_existChunks.TryGetValue(coord, out ChunkCache element))
            {
                element = new ChunkCache(m_path, coord);
                m_existChunks.Add(coord, element);
            }
            element.Write(chunk.GetVoxelsBuffer());
        }

        public bool ReadIfExist(Chunk chunk, int3 coord)
        {
            if (!m_existChunks.TryGetValue(coord, out ChunkCache element))
            {
                if (ChunkCache.IsCacheExist(m_path, coord))
                {
                    element = new ChunkCache(m_path, coord);
                    m_existChunks.Add(coord, element);
                }
            }
            
            if (element != null)
            {
                element.Read(chunk.GetVoxelsBuffer());
                return true;
            }

            return false;
        }
    }
}