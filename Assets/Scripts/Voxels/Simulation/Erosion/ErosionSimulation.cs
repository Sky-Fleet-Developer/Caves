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
        [SerializeField] private ComputeShader compute;
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
        
        public override void Init(SimulationSpace simulationSpace)
        {
            base.Init(simulationSpace);
            CreateBuffers();
            VoxelVolumeConfig voxelVolumeConfig = WorldManager.VoxelConfig.VoxelVolumeConfig;
            compute.SetInts(ComputeShaderProperties.NumberOfVoxels, voxelVolumeConfig.NumberOfVoxels.x, voxelVolumeConfig.NumberOfVoxels.y, voxelVolumeConfig.NumberOfVoxels.z);
            compute.SetFloat(ComputeShaderProperties.VoxelSpacing, voxelVolumeConfig.VoxelSpacing);
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
                    ushort value = (ushort)valueAndMaterial;
                    ushort material = (ushort)(valueAndMaterial >> 16);
                    voxelsArray[i] = new PackedVoxel(value, material, defaultVoxelWetness,
                        new float3(-1, 0.5f, 3));
                }
                
                bufferA.SetData(voxelsArray);
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
            compute.SetBuffer(0, ComputeShaderProperties.MaterialPropertiesConfig, m_materialPropertiesBuffer);
            compute.SetBuffer(1, ComputeShaderProperties.MaterialPropertiesConfig, m_materialPropertiesBuffer);

            foreach (KeyValuePair<int3, DoubleBuffer> chunkKv in m_buffers)
            {
                compute.SetBuffer(0, ComputeShaderProperties.VoxelVolumeRead, chunkKv.Value.GetBufferForRead());
                compute.SetBuffer(0, ComputeShaderProperties.VoxelVolumeWrite, chunkKv.Value.GetBufferForWrite());

                compute.Dispatch(0, WorldManager.VoxelConfig.VoxelVolumeConfig.NumberOfCellsAlongAxis);
                chunkKv.Value.SwitchBuffers();

                compute.SetBuffer(1, ComputeShaderProperties.VoxelVolumeRead, chunkKv.Value.GetBufferForRead());
                compute.SetBuffer(1, ComputeShaderProperties.VoxelVolumeWrite, chunkKv.Value.GetBufferForWrite());

                compute.Dispatch(1, WorldManager.VoxelConfig.VoxelVolumeConfig.NumberOfCellsAlongAxis);
                chunkKv.Value.SwitchBuffers();
            }
        }

        public override void OnFinalize()
        {
            foreach (KeyValuePair<int3, Chunk> chunkKv in m_simulationSpace.Chunks)
            {
                ComputeBuffer voxelsBuffer = chunkKv.Value.GetVoxelsBuffer();
                uint[] data = new uint[voxelsBuffer.count * Chunk.ElementsInVoxel];
                voxelsBuffer.GetData(data);
                PackedVoxel[] voxelsArray = new PackedVoxel[voxelsBuffer.count];
                m_buffers[chunkKv.Key].GetBufferForRead().GetData(voxelsArray);

                for (int i = 0; i < voxelsArray.Length; i++)
                {
                    float3 gr = voxelsArray[i].GetGradient();

                    uint newValue = voxelsArray[i].value | ((uint) voxelsArray[i].material << 16);
                    if (newValue != data[i * 2])
                    {
                        
                    }
                    data[i * 2] = newValue;
                }
                voxelsBuffer.SetData(data);
                chunkKv.Value.RegenerateMesh();
            }
            ReleaseBuffers();
        }
    }
}
