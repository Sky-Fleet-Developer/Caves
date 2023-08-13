using System;
using Sirenix.OdinInspector;
using UnityEngine;
using World;

namespace Voxels.Simulation
{
    public class Simulator : MonoBehaviour
    {
        [SerializeField] private SimulationSpace simulationSpace;
        private Simulation[] m_simulations;
        
        private void Awake()
        {
            m_simulations = GetComponentsInChildren<Simulation>();
        }

        [Button]
        public void Execute()
        {
            Init();
            SimulationIteration(0);
            OnFinalize();   
        }
        
        private void Init()
        {
            simulationSpace.Init(WorldManager.GetActiveChunks());
            foreach (Simulation simulation in m_simulations)
            {
                simulation.Init(simulationSpace);   
            }
        }
        
        private void SimulationIteration(float deltaTime)
        {
            foreach (Simulation simulation in m_simulations)
            {
                simulation.Iterate(deltaTime);
            }
        }

        private void OnFinalize()
        {
            foreach (Simulation simulation in m_simulations)
            {
                simulation.OnFinalize();
            }
        }
    }
}