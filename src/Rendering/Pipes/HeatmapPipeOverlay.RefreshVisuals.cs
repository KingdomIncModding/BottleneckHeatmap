using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        public void RefreshVisuals(bool force)
        {
            if (!_overlayLine || !_nativeLine)
            {
                CreateOverlayLine();
                if (!_overlayLine) return;
            }

            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin == null || !plugin.IsHeatmapEnabled || !_nativeLine.enabled || _nativeLine.positionCount < 2)
            {
                if (_overlayLine.enabled) _overlayLine.enabled = false;
                return;
            }

            var newState = BottleneckStateEvaluator.Evaluate(_worldPipe, _lastTransferTime, _wasRejected);

            if (newState == BottleneckState.Idle && !plugin.ShowIdle && plugin.EnableIdleState)
            {
                if (_overlayLine.enabled) _overlayLine.enabled = false;
                _currentState = newState;
                return;
            }

            if (force || newState != _currentState || !_overlayLine.enabled)
            {
                _currentState = newState;
                var color = HeatmapMaterials.GetPipeColor(_currentState, plugin.PipeOpacity);
                if (color.a <= 0f)
                {
                    _overlayLine.enabled = false;
                }
                else
                {
                    RefreshGeometry();
                    _overlayLine.enabled = true;
                    _overlayLine.startColor = color;
                    _overlayLine.endColor = color;
                }
            }
        }
    }
}
