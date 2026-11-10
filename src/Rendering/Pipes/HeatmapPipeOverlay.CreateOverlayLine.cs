using Assets.Behaviour.Flow;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapPipeOverlay : MonoBehaviour
    {
        void CreateOverlayLine()
        {
            if (_overlayLine || !_nativeLine) return;

            var overlayObj = new GameObject("HeatmapPipeOverlay");
            overlayObj.layer = _nativeLine.gameObject.layer;
            overlayObj.transform.SetParent(transform, false);

            _overlayLine = overlayObj.AddComponent<LineRenderer>();
            _overlayLine.sharedMaterial = HeatmapMaterials.LineMaterial;
            _overlayLine.alignment = _nativeLine.alignment;
            _overlayLine.textureMode = LineTextureMode.Stretch;
            _overlayLine.numCornerVertices = 5;
            _overlayLine.numCapVertices = 5;
            _overlayLine.shadowCastingMode = ShadowCastingMode.Off;
            _overlayLine.receiveShadows = false;
            _overlayLine.sortingLayerID = _nativeLine.sortingLayerID;
            _overlayLine.sortingOrder = _nativeLine.sortingOrder + 1;
            _overlayLine.widthCurve = AnimationCurve.Constant(0, 1, 1);
            _overlayLine.enabled = false;
        }
    }
}
