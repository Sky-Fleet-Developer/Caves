using UnityEngine;
using UnityEngine.Serialization;

namespace Voxels
{
    [System.Serializable]
    public class PreviewCutoffConfig
    {
        [SerializeField] private bool usePreviewCutoff;
        [FormerlySerializedAs("invertEffect")] [SerializeField] private bool effectInverted;
        [SerializeField] private Bounds cutoffBounds;
        public bool IsEffectInverted => effectInverted;
        public Bounds? GetConfig()
        {
            return usePreviewCutoff ? cutoffBounds : null;
        }
    }
}