using UnityEngine;

namespace Voxels
{
    [System.Serializable]
    public class PreviewCutoffConfig
    {
        [SerializeField] private bool usePreviewCutoff;
        [SerializeField] private Bounds cutoffBounds;

        public Bounds? GetConfig()
        {
            return usePreviewCutoff ? cutoffBounds : null;
        }
    }
}