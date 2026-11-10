using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        void OnDestroy()
        {
            if (_overlayLine)
            {
                Destroy(_overlayLine.gameObject);
                _overlayLine = null;
            }
        }
    }
}
