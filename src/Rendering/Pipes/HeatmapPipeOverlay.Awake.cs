using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        void Awake()
        {
            _worldPipe = GetComponent<WorldFlowPipe>();
            _nativeLine = _worldPipe.GetComponentInChildren<LineRenderer>();
            CreateOverlayLine();
        }
    }
}
