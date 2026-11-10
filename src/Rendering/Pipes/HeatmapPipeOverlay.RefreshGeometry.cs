using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        public void RefreshGeometry()
        {
            if (!_overlayLine || !_nativeLine) return;

            int count = _nativeLine.positionCount;
            if (count < 2)
            {
                _overlayLine.enabled = false;
                return;
            }

            if (_positionsBuffer.Length != count)
            {
                _positionsBuffer = new Vector3[count];
            }

            _nativeLine.GetPositions(_positionsBuffer);

            _overlayLine.useWorldSpace = _nativeLine.useWorldSpace;
            _overlayLine.sortingLayerID = _nativeLine.sortingLayerID;
            _overlayLine.sortingOrder = _nativeLine.sortingOrder + 1;

            var plugin = BottleneckHeatmapPlugin.Instance;
            float widthScale = plugin != null ? plugin.PipeWidthMultiplier : 1.3f;
            _overlayLine.widthMultiplier = _nativeLine.widthMultiplier * widthScale;

            _overlayLine.positionCount = count;
            _overlayLine.SetPositions(_positionsBuffer);
        }
    }
}
