using UnityEngine;

namespace Voxels.Simulation
{
    public abstract class Simulation : MonoBehaviour
    {
        protected SimulationSpace m_simulationSpace;
        public virtual void Init(SimulationSpace simulationSpace)
        {
            m_simulationSpace = simulationSpace;
        }
        public abstract void Iterate(float deltaTime);
        public abstract void OnFinalize();
    }
}
