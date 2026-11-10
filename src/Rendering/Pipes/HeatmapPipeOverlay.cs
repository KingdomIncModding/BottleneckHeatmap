using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        WorldFlowPipe _worldPipe;
        LineRenderer _nativeLine;
        LineRenderer _overlayLine;
        float _lastTransferTime = -100f;
        bool _wasRejected;
        float _nextCheckTime;
        BottleneckState _currentState = BottleneckState.Idle;
        Vector3[] _positionsBuffer = new Vector3[0];

        public BottleneckState CurrentState => _currentState;


        public static HeatmapPipeOverlay Ensure(WorldFlowPipe worldPipe)
        {
            if (!worldPipe) return null;
            var overlay = worldPipe.GetComponent<HeatmapPipeOverlay>();
            if (!overlay)
            {
                overlay = worldPipe.gameObject.AddComponent<HeatmapPipeOverlay>();
            }
            return overlay;
        }
    }
}
