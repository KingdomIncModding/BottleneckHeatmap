using System.Reflection;
using Assets.Behaviour.Flow;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed class HeatmapNodeOverlay : MonoBehaviour
    {
        static readonly FieldInfo BackgroundField = AccessTools.Field(typeof(WorldFlowElement), "_background");

        WorldFlowElement _world;
        Image _overlayImage;
        float _nextCheckTime;
        BottleneckState _currentState = BottleneckState.Idle;

        public BottleneckState CurrentState => _currentState;

        public static HeatmapNodeOverlay Ensure(WorldFlowElement world)
        {
            if (!world) return null;
            var overlay = world.GetComponent<HeatmapNodeOverlay>();
            if (!overlay)
            {
                overlay = world.gameObject.AddComponent<HeatmapNodeOverlay>();
            }
            return overlay;
        }

        void Awake()
        {
            _world = GetComponent<WorldFlowElement>();
            CreateOverlayUI();
        }

        void Start()
        {
            RefreshVisuals(true);
        }

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

        void Update()
        {
            if (Time.time < _nextCheckTime) return;
            _nextCheckTime = Time.time + 0.12f;

            RefreshVisuals(false);
        }

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

        void OnDestroy()
        {
            if (_overlayImage)
            {
                Destroy(_overlayImage.gameObject);
                _overlayImage = null;
            }
        }
    }
}

