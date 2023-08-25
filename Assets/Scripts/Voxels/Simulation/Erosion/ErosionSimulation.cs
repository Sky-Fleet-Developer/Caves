using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;
using World;
using Extensions;
using Voxels.Simulation.Erosion.Materials;
using Voxels.Volume;

namespace Voxels.Simulation.Erosion
{
    public class ErosionSimulation : Simulation
    {
        [SerializeField] private MaterialPropertiesConfig materialPropertiesConfig;
        [SerializeField] private ComputeShader erodeCompute;
        [SerializeField] private ComputeShader syncCompute;
        [SerializeField] private float defaultVoxelWetness = 0.1f;
        private ComputeBuffer m_materialPropertiesBuffer;

        private class DoubleBuffer
        {
            private readonly ComputeBuffer m_a;
            private readonly ComputeBuffer m_b;
            private bool m_switcher;

            public DoubleBuffer(ComputeBuffer bufferA, ComputeBuffer bufferB)
            {
                m_a = bufferA;
                m_b = bufferB;
                m_switcher = false;
            }

            public ComputeBuffer GetBufferForRead()
            {
                return m_switcher ? m_b : m_a;
            }

            public ComputeBuffer GetBufferForWrite()
            {
                return m_switcher ? m_a : m_b;
            }

            public void SwitchBuffers()
            {
                m_switcher = !m_switcher;
            }

            public void Release()
            {
                m_a.Release();
                m_b.Release();
            }
        }

        private Dictionary<int3, DoubleBuffer> m_buffers;

        private static int3[] neighbours =
        {
            new(0, 0, 1),
            new(1, 0, 0),
            new(0, 1, 0),
        };


        public override void Init(SimulationSpace simulationSpace)
        {
            base.Init(simulationSpace);
            CreateBuffers();
            UpdateConfigs();
        }

        private void UpdateConfigs()
        {
            VoxelVolumeConfig voxelVolumeConfig = WorldManager.VoxelConfig.VoxelVolumeConfig;
            erodeCompute.SetInts(ComputeShaderProperties.NumberOfVoxels, voxelVolumeConfig.NumberOfVoxels.x, voxelVolumeConfig.NumberOfVoxels.y, voxelVolumeConfig.NumberOfVoxels.z);
            erodeCompute.SetFloat(ComputeShaderProperties.VoxelSpacing, voxelVolumeConfig.VoxelSpacing);
            syncCompute.SetInts(ComputeShaderProperties.NumberOfVoxels, voxelVolumeConfig.NumberOfVoxels.x, voxelVolumeConfig.NumberOfVoxels.y, voxelVolumeConfig.NumberOfVoxels.z);
            syncCompute.SetFloat(ComputeShaderProperties.VoxelSpacing, voxelVolumeConfig.VoxelSpacing);
            erodeCompute.SetBuffer(0, ComputeShaderProperties.MaterialPropertiesConfig, m_materialPropertiesBuffer);
            erodeCompute.SetBuffer(1, ComputeShaderProperties.MaterialPropertiesConfig, m_materialPropertiesBuffer);
        }

        private void OnDestroy()
        {
            ReleaseBuffers();
        }

        private void CreateBuffers()
        {
            if (m_buffers != null)
            {
                ReleaseBuffers();
            }

            m_buffers = new Dictionary<int3, DoubleBuffer>(m_simulationSpace.Chunks.Count);

            m_materialPropertiesBuffer = materialPropertiesConfig.GetConfigsBuffer();

            foreach (KeyValuePair<int3, Chunk> chunkKv in m_simulationSpace.Chunks)
            {
                ComputeBuffer voxelsBuffer = chunkKv.Value.GetVoxelsBuffer();
                ComputeBuffer bufferA = new(voxelsBuffer.count, PackedVoxel.GetSize());
                ComputeBuffer bufferB = new(voxelsBuffer.count, PackedVoxel.GetSize());
                PackedVoxel[] voxelsArray = new PackedVoxel[voxelsBuffer.count];
                uint[] oldBufferData = new uint[voxelsBuffer.count * Chunk.ElementsInVoxel];
                voxelsBuffer.GetData(oldBufferData);

                for (int i = 0; i < voxelsArray.Length; i++)
                {
                    uint valueAndMaterial = oldBufferData[i * 2];
                    ushort value = (ushort) valueAndMaterial;
                    ushort material = (ushort) (valueAndMaterial >> 16);
                    voxelsArray[i] = new PackedVoxel(value, material, defaultVoxelWetness, float3.zero);
                }

                bufferA.SetData(voxelsArray);
                bufferB.SetData(voxelsArray);
                m_buffers.Add(chunkKv.Key, new DoubleBuffer(bufferA, bufferB));
            }
        }

        public void ReleaseBuffers()
        {
            foreach (KeyValuePair<int3, DoubleBuffer> chunkKv in m_buffers)
            {
                chunkKv.Value.Release();
            }

            m_materialPropertiesBuffer.Release();
        }

        public override void Iterate(float deltaTime)
        {
            erodeCompute.SetFloat(ComputeShaderProperties.DeltaTime, deltaTime);

            CalculateGradient();

            Sync();

            Erode();
        }
        private void CalculateGradient()
        {
            foreach (KeyValuePair<int3, DoubleBuffer> chunkKv in m_buffers)
            {
                erodeCompute.SetBuffer(0, ComputeShaderProperties.VoxelVolumeRead, chunkKv.Value.GetBufferForRead());
                erodeCompute.SetBuffer(0, ComputeShaderProperties.VoxelVolumeWrite, chunkKv.Value.GetBufferForWrite());

                erodeCompute.Dispatch(0, WorldManager.VoxelConfig.VoxelVolumeConfig.NumberOfVoxelsAlongAxis);
                chunkKv.Value.SwitchBuffers();
            }
        }
        
        private void Sync()
        {
            int3 min = m_simulationSpace.GetMinimum();
            int3 max = m_simulationSpace.GetMaximum();
            for (int3 coord = min; coord.x < max.x; coord.x++)
            {
                for (coord.y = min.y; coord.y < max.y; coord.y++)
                {
                    for (coord.z = min.z; coord.z < max.z; coord.z++)
                    {
                        for (int i = 0; i < neighbours.Length; i++)
                        {
                            IterateSync(coord, i, max);
                        }
                    }
                }
            }
        }

        private void IterateSync(int3 coord, int neighbourIdx, int3 max)
        {
            int3 neighbour = coord + neighbours[neighbourIdx];
            if (neighbour.x >= max.x || neighbour.y >= max.y || neighbour.z >= max.z) return;
            syncCompute.SetBuffer(0, ComputeShaderProperties.VoxelVolumeA, m_buffers[coord].GetBufferForRead());
            syncCompute.SetBuffer(0, ComputeShaderProperties.VoxelVolumeB, m_buffers[neighbour].GetBufferForRead());
            syncCompute.SetInt(ComputeShaderProperties.Face, neighbourIdx);
            int numberOfVoxelsAlongAxis = WorldManager.VoxelConfig.VoxelVolumeConfig.NumberOfVoxelsAlongAxis;
            syncCompute.Dispatch(0, new int3(numberOfVoxelsAlongAxis, numberOfVoxelsAlongAxis, 2));
        }

        private void Erode()
        {
            foreach (KeyValuePair<int3, DoubleBuffer> chunkKv in m_buffers)
            {
                erodeCompute.SetBuffer(1, ComputeShaderProperties.VoxelVolumeRead, chunkKv.Value.GetBufferForRead());
                erodeCompute.SetBuffer(1, ComputeShaderProperties.VoxelVolumeWrite, chunkKv.Value.GetBufferForWrite());

                erodeCompute.Dispatch(1, WorldManager.VoxelConfig.VoxelVolumeConfig.NumberOfVoxelsAlongAxis);
                chunkKv.Value.SwitchBuffers();
            }
        }
        

        public override void OnFinalize()
        {
            foreach (KeyValuePair<int3, DoubleBuffer> chunkKv in m_buffers)
            {
                erodeCompute.SetBuffer(2, ComputeShaderProperties.VoxelVolumeRead, chunkKv.Value.GetBufferForRead());
                erodeCompute.SetBuffer(2, ComputeShaderProperties.VoxelVolumeWrite, chunkKv.Value.GetBufferForWrite());

                erodeCompute.Dispatch(2, WorldManager.VoxelConfig.VoxelVolumeConfig.NumberOfVoxelsAlongAxis);
                chunkKv.Value.SwitchBuffers();
            }
            
            foreach (KeyValuePair<int3, Chunk> chunkKv in m_simulationSpace.Chunks)
            {
                ComputeBuffer voxelsBuffer = chunkKv.Value.GetVoxelsBuffer();
                uint[] data = new uint[voxelsBuffer.count * Chunk.ElementsInVoxel];
                voxelsBuffer.GetData(data);
                PackedVoxel[] voxelsArray = new PackedVoxel[voxelsBuffer.count];
                m_buffers[chunkKv.Key].GetBufferForRead().GetData(voxelsArray);

                for (int i = 0; i < voxelsArray.Length; i++)
                {
                    data[i * 2] = voxelsArray[i].value | ((uint) voxelsArray[i].material << 16);
                }

                voxelsBuffer.SetData(data);
                chunkKv.Value.RegenerateMesh();
            }

            ReleaseBuffers();
        }
    }
}