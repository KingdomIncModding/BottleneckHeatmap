using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        void Update()
        {
            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + 0.12f;

            RefreshVisuals(false);
        }
    }
}
