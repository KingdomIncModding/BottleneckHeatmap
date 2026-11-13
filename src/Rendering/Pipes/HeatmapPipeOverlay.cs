using KingdomFoundation.Api;
using UnityEngine;
using UnityEngine.Rendering;

namespace KingdomBottleneckHeatmap
{
    public sealed class HeatmapPipeOverlay : MonoBehaviour
    {
        void Awake()
        {
            _worldPipe = FoundationApi.WorkspaceVisuals.PipeFromView(this)?.World;
            _nativeLine = _worldPipe.GetComponentInChildren<LineRenderer>();
            CreateOverlayLine();
        }

        void CreateOverlayLine()
        {
            if (_overlayLine || !_nativeLine)
                return;
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

        Component _worldPipe;
        LineRenderer _nativeLine;
        LineRenderer _overlayLine;
        float _lastTransferTime = -100f;
        bool _wasRejected;
        float _nextCheckTime;
        BottleneckState _currentState = BottleneckState.Idle;
        Vector3[] _positionsBuffer = new Vector3[0];
        public BottleneckState CurrentState => _currentState;

        public static HeatmapPipeOverlay Ensure(Component worldPipe)
        {
            if (!worldPipe)
                return null;
            var overlay = worldPipe.GetComponent<HeatmapPipeOverlay>();
            if (!overlay)
            {
                overlay = worldPipe.gameObject.AddComponent<HeatmapPipeOverlay>();
            }

            return overlay;
        }

        public void NotifyItemRejected()
        {
            _wasRejected = true;
            RefreshVisuals(false);
        }

        public void NotifyItemTransferred()
        {
            _wasRejected = false;
            _lastTransferTime = Time.time;
            RefreshVisuals(false);
        }

        void OnDestroy()
        {
            if (_overlayLine)
            {
                Destroy(_overlayLine.gameObject);
                _overlayLine = null;
            }
        }

        public void RefreshGeometry()
        {
            if (!_overlayLine || !_nativeLine)
                return;
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

        public void RefreshVisuals(bool force)
        {
            if (!_overlayLine || !_nativeLine)
            {
                CreateOverlayLine();
                if (!_overlayLine)
                    return;
            }

            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin == null || !plugin.IsHeatmapEnabled || !_nativeLine.enabled || _nativeLine.positionCount < 2)
            {
                if (_overlayLine.enabled)
                    _overlayLine.enabled = false;
                return;
            }

            var newState = BottleneckStateEvaluator.Evaluate(_worldPipe, _lastTransferTime, _wasRejected);
            if (newState == BottleneckState.Idle && !plugin.ShowIdle && plugin.EnableIdleState)
            {
                if (_overlayLine.enabled)
                    _overlayLine.enabled = false;
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

        void Start()
        {
            RefreshGeometry();
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
