using System.Reflection;
using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class HeatmapNodeOverlay : MonoBehaviour
    {
        void CreateOverlayUI()
        {
            if (_overlayImage) return;

            var bg = BackgroundField?.GetValue(_world) as Image;
            var overlayObj = new GameObject("HeatmapNodeOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlayObj.transform.SetParent(transform, false);

            var rect = (RectTransform)overlayObj.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            _overlayImage = overlayObj.GetComponent<Image>();
            _overlayImage.raycastTarget = false;
            _overlayImage.type = Image.Type.Sliced;
            _overlayImage.pixelsPerUnitMultiplier = 0.5f;

            if (bg != null && bg.sprite != null)
            {
                _overlayImage.sprite = bg.sprite;
                overlayObj.transform.SetSiblingIndex(bg.transform.GetSiblingIndex() + 1);
            }
            else if (_world.BackgroundSprite != null)
            {
                _overlayImage.sprite = _world.BackgroundSprite;
            }

            _overlayImage.enabled = false;
        }
    }
}
