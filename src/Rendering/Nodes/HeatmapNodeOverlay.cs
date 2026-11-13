using KingdomFoundation.Api;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed class HeatmapNodeOverlay : MonoBehaviour
    {
        void Awake()
        {
            _world = FoundationApi.WorkspaceAccess.FindBuildingView(gameObject);
            CreateOverlayUI();
        }

        void CreateOverlayUI()
        {
            if (_overlayImage)
                return;
            var bg = FoundationApi.WorkspaceVisuals.Background(_world);
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
            else if (FoundationApi.WorkspaceVisuals.BackgroundSprite(_world) != null)
            {
                _overlayImage.sprite = FoundationApi.WorkspaceVisuals.BackgroundSprite(_world);
            }

            _overlayImage.enabled = false;
        }

        Component _world;
        Image _overlayImage;
        float _nextCheckTime;
        BottleneckState _currentState = BottleneckState.Idle;
        public BottleneckState CurrentState => _currentState;

        public static HeatmapNodeOverlay Ensure(Component world)
        {
            if (!world)
                return null;
            var overlay = world.GetComponent<HeatmapNodeOverlay>();
            if (!overlay)
            {
                overlay = world.gameObject.AddComponent<HeatmapNodeOverlay>();
            }

            return overlay;
        }

        void OnDestroy()
        {
            if (_overlayImage)
            {
                Destroy(_overlayImage.gameObject);
                _overlayImage = null;
            }
        }

        public void RefreshVisuals(bool force)
        {
            if (!_overlayImage)
            {
                CreateOverlayUI();
                if (!_overlayImage)
                    return;
            }

            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin == null || !plugin.IsHeatmapEnabled)
            {
                if (_overlayImage.enabled)
                    _overlayImage.enabled = false;
                return;
            }

            var newState = BottleneckStateEvaluator.Evaluate(_world);
            if (newState == BottleneckState.Idle && !plugin.ShowIdle && plugin.EnableIdleState)
            {
                if (_overlayImage.enabled)
                    _overlayImage.enabled = false;
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

        void Start()
        {
            RefreshVisuals(true);
        }

        void Update()
        {
            if (Time.time < _nextCheckTime)
                return;
            _nextCheckTime = Time.time + 0.12f;
            RefreshVisuals(false);
        }
    }
}
