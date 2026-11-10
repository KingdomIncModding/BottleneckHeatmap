using System.Reflection;
using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapNodeOverlay : MonoBehaviour
    {
        public void RefreshVisuals(bool force)
        {
            if (!_overlayImage)
            {
                CreateOverlayUI();
                if (!_overlayImage) return;
            }

            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin == null || !plugin.IsHeatmapEnabled)
            {
                if (_overlayImage.enabled) _overlayImage.enabled = false;
                return;
            }

            var newState = BottleneckStateEvaluator.Evaluate(_world);

            if (newState == BottleneckState.Idle && !plugin.ShowIdle && plugin.EnableIdleState)
            {
                if (_overlayImage.enabled) _overlayImage.enabled = false;
                _currentState = newState;
                return;
            }

            if (force || newState != _currentState || !_overlayImage.enabled)
            {
                _currentState = newState;
                var col = HeatmapMaterials.GetNodeColor(_currentState, plugin.NodeOpacity);
                if (col.a <= 0f)
                {
                    _overlayImage.enabled = false;
                }
                else
                {
                    _overlayImage.enabled = true;
                    _overlayImage.color = col;
                }
            }
        }
    }
}
