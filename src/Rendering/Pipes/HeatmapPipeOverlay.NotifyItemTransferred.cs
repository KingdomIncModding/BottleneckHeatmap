using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        public void NotifyItemTransferred()
        {
            _wasRejected = false;
            _lastTransferTime = Time.time;
            RefreshVisuals(false);
        }
    }
}
