using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public enum PickerMode { SaturationValue, Hue }

    public sealed class PickerSurface : MaskableGraphic, IPointerDownHandler, IDragHandler
    {
        internal PickerMode Mode;
        internal float Hue, Saturation = 1f, Brightness = 1f;
        internal Action<Vector2> Changed;

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = rectTransform.rect;
            int columns = Mode == PickerMode.SaturationValue ? 20 : 1;
            int rows = Mode == PickerMode.SaturationValue ? 16 : 24;

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    float x0 = (float)x / columns, x1 = (float)(x + 1) / columns;
                    float y0 = (float)y / rows, y1 = (float)(y + 1) / rows;
                    Quad(mesh, new Rect(rect.xMin + x0 * rect.width, rect.yMin + y0 * rect.height,
                        (x1 - x0) * rect.width, (y1 - y0) * rect.height),
                        At(x0, y0), At(x1, y0), At(x1, y1), At(x0, y1));
                }
            }

            Vector2 marker;
            if (Mode == PickerMode.SaturationValue)
                marker = new Vector2(rect.xMin + Saturation * rect.width, rect.yMin + Brightness * rect.height);
            else
                marker = new Vector2(rect.center.x, rect.yMax - Hue * rect.height);

            if (Mode == PickerMode.SaturationValue)
            {
                Ring(mesh, marker, 6, 4, Color.black, Color.white);
            }
            else
            {
                var line = new Rect(rect.xMin - 2, marker.y - 3, rect.width + 4, 6);
                Quad(mesh, line, Color.black);
                line.xMin += 1; line.xMax -= 1; line.yMin += 1; line.yMax -= 1;
                Quad(mesh, line, Color.white);
            }
        }

        Color At(float x, float y)
        {
            if (Mode == PickerMode.SaturationValue) return Color.HSVToRGB(Hue, x, y);
            return Color.HSVToRGB(1 - y, 1, 1);
        }

        public void OnPointerDown(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) Select(data);
        }

        public void OnDrag(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left) Select(data);
        }

        void Select(PointerEventData data)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out var local))
            {
                var rect = rectTransform.rect;
                Changed?.Invoke(new Vector2(Mathf.Clamp01((local.x - rect.xMin) / rect.width), Mathf.Clamp01((local.y - rect.yMin) / rect.height)));
            }
        }

        static void Quad(VertexHelper mesh, Rect r, Color color) => Quad(mesh, r, color, color, color, color);

        static void Quad(VertexHelper mesh, Rect r, Color bl, Color br, Color tr, Color tl)
        {
            int i = mesh.currentVertCount;
            mesh.AddVert(new Vector3(r.xMin, r.yMin), bl, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax, r.yMin), br, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMax, r.yMax), tr, Vector2.zero);
            mesh.AddVert(new Vector3(r.xMin, r.yMax), tl, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2);
            mesh.AddTriangle(i, i + 2, i + 3);
        }

        static void Ring(VertexHelper mesh, Vector2 center, float outer, float inner, Color outline, Color fill)
        {
            for (int layer = 0; layer < 2; layer++)
            {
                float a = layer == 0 ? outer : outer - 1, b = layer == 0 ? inner - 1 : inner;
                for (int n = 0; n < 24; n++)
                {
                    float start = n * Mathf.PI / 12, end = (n + 1) * Mathf.PI / 12;
                    Vector2 d0 = new Vector2(Mathf.Cos(start), Mathf.Sin(start)), d1 = new Vector2(Mathf.Cos(end), Mathf.Sin(end));
                    int i = mesh.currentVertCount; Color c = layer == 0 ? outline : fill;
                    mesh.AddVert(center + d0 * a, c, Vector2.zero);
                    mesh.AddVert(center + d1 * a, c, Vector2.zero);
                    mesh.AddVert(center + d1 * b, c, Vector2.zero);
                    mesh.AddVert(center + d0 * b, c, Vector2.zero);
                    mesh.AddTriangle(i, i + 1, i + 2);
                    mesh.AddTriangle(i, i + 2, i + 3);
                }
            }
        }
    }

    public sealed class ColorPalettePopup : MonoBehaviour
    {
        static ColorPalettePopup _instance;

        BottleneckState _currentState = BottleneckState.Running;

        // Current base color in HSV
        float _baseHue = 0.38f;
        float _baseSaturation = 0.85f;
        float _baseValue = 0.98f;

        // UI references
        PickerSurface _svSurface;
        PickerSurface _hueSurface;
        Image _baseColorPreview;
        TMP_Text _baseHexText;
        TMP_Text _headerSubtitle;
        TMP_Text _variationsTitle;
        Button _applyButton;

        // Tab buttons
        Button _tabRunning, _tabStarved, _tabClogged, _tabFullStorage, _tabIdle;

        // 5 Variation Cards
        struct VariationCard
        {
            public GameObject Root;
            public Image Swatch;
            public TMP_Text Title;
            public TMP_Text Hex;
            public Color Color;
        }

        readonly VariationCard[] _cards = new VariationCard[5];
        (string Label, string Hex)[] _currentGeneratedVariations;

        static readonly string[] Presets = new string[]
        {
            "26FA73", "00FFB2", "00D9FF", "2E75FF", "8C28FF",
            "B042FF", "DF2BFF", "FF2B85", "FF3842", "FFD92E"
        };

        public static void Open(BottleneckState targetState = BottleneckState.Running)
        {
            if (!_instance)
            {
                var canvas = FindAnyObjectByType<Canvas>();
                if (!canvas) return;

                var go = new GameObject("BottleneckColorPalettePopup", typeof(RectTransform));
                go.transform.SetParent(canvas.transform, false);
                _instance = go.AddComponent<ColorPalettePopup>();
            }

            _instance.gameObject.SetActive(true);
            _instance.transform.SetAsLastSibling();
            _instance.SelectState(targetState);
        }

        public static void Close()
        {
            if (_instance)
            {
                _instance.gameObject.SetActive(false);
            }
        }

        void Awake()
        {
            BuildUI();
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        public void SelectState(BottleneckState state)
        {
            _currentState = state;
            var plugin = BottleneckHeatmapPlugin.Instance;
            Color curColor = Color.green;

            if (plugin != null)
            {
                string hex = state switch
                {
                    BottleneckState.Running => plugin.RunningColorConfig.Value,
                    BottleneckState.Starved => plugin.StarvedColorConfig.Value,
                    BottleneckState.Clogged => plugin.CloggedColorConfig.Value,
                    BottleneckState.FullStorage => plugin.FullStorageColorConfig.Value,
                    BottleneckState.Idle => plugin.IdleColorConfig.Value,
                    _ => "#26FA73"
                };
                ColorUtility.TryParseHtmlString(hex, out curColor);
            }

            Color.RGBToHSV(curColor, out _baseHue, out _baseSaturation, out _baseValue);
            if (_baseSaturation < 0.2f && state != BottleneckState.Idle) _baseSaturation = 0.85f;
            if (_baseValue < 0.4f) _baseValue = 0.95f;

            UpdateTabsVisuals();
            UpdateSurfaces();
            UpdateGeneratedVariations();

            string stateName = GetStateDisplayName(_currentState);
            if (_headerSubtitle) _headerSubtitle.text = $"Select base color for <color=yellow>{stateName}</color> to auto-generate slight variation hues";
            if (_variationsTitle) _variationsTitle.text = $"Auto-Generated Surrounding Hues for {stateName}:";
            if (_applyButton)
            {
                var tmp = _applyButton.GetComponentInChildren<TMP_Text>(true);
                if (tmp) tmp.text = $"Apply to {stateName}";
            }
        }

        string GetStateDisplayName(BottleneckState state) => state switch
        {
            BottleneckState.Running => "Running",
            BottleneckState.Starved => "Starved",
            BottleneckState.Clogged => "Clogged",
            BottleneckState.FullStorage => "Full Storage",
            BottleneckState.Idle => "Idle",
            _ => state.ToString()
        };

        void UpdateTabsVisuals()
        {
            SetTabActive(_tabRunning, _currentState == BottleneckState.Running);
            SetTabActive(_tabStarved, _currentState == BottleneckState.Starved);
            SetTabActive(_tabClogged, _currentState == BottleneckState.Clogged);
            SetTabActive(_tabFullStorage, _currentState == BottleneckState.FullStorage);
            SetTabActive(_tabIdle, _currentState == BottleneckState.Idle);
        }

        void SetTabActive(Button tab, bool active)
        {
            if (!tab) return;
            var img = tab.GetComponent<Image>();
            if (img) img.color = active ? new Color(0.35f, 0.32f, 0.25f, 1f) : new Color(0.18f, 0.17f, 0.20f, 0.95f);
            var text = tab.GetComponentInChildren<TMP_Text>(true);
            if (text) text.color = active ? Color.yellow : new Color(0.75f, 0.72f, 0.68f);
        }

        void UpdateSurfaces()
        {
            if (_svSurface)
            {
                _svSurface.Hue = _baseHue;
                _svSurface.Saturation = _baseSaturation;
                _svSurface.Brightness = _baseValue;
                _svSurface.SetVerticesDirty();
            }

            if (_hueSurface)
            {
                _hueSurface.Hue = _baseHue;
                _hueSurface.SetVerticesDirty();
            }
        }

        void UpdateGeneratedVariations()
        {
            Color baseColor = Color.HSVToRGB(_baseHue, Mathf.Clamp01(_baseSaturation), Mathf.Clamp01(_baseValue));
            if (_baseColorPreview) _baseColorPreview.color = baseColor;
            if (_baseHexText) _baseHexText.text = "#" + ColorUtility.ToHtmlStringRGB(baseColor);

            string stateName = GetStateDisplayName(_currentState);
            _currentGeneratedVariations = FoundationIntegration.GenerateVariations(baseColor, stateName);

            string[] cardTitles = { "Selected Base", "Warm Tone", "Cool Tone", "Bright Tone", "Deep Tone" };
            for (int i = 0; i < _cards.Length && i < _currentGeneratedVariations.Length; i++)
            {
                ColorUtility.TryParseHtmlString(_currentGeneratedVariations[i].Hex, out var col);
                SetCard(i, cardTitles[i], col, _currentGeneratedVariations[i].Hex);
            }
        }

        void SetCard(int idx, string title, Color col, string hex)
        {
            if (idx >= _cards.Length) return;
            _cards[idx].Color = col;
            if (_cards[idx].Swatch) _cards[idx].Swatch.color = col;
            if (_cards[idx].Title) _cards[idx].Title.text = title;
            if (_cards[idx].Hex) _cards[idx].Hex.text = hex;
        }

        void ApplyCurrentColor()
        {
            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin != null)
            {
                Color baseColor = Color.HSVToRGB(_baseHue, Mathf.Clamp01(_baseSaturation), Mathf.Clamp01(_baseValue));
                string hex = "#" + ColorUtility.ToHtmlStringRGB(baseColor);

                switch (_currentState)
                {
                    case BottleneckState.Running:
                        plugin.RunningColorConfig.Value = hex;
                        FoundationIntegration.RunningVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Running, _currentGeneratedVariations);
                        break;
                    case BottleneckState.Starved:
                        plugin.StarvedColorConfig.Value = hex;
                        FoundationIntegration.StarvedVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Starved, _currentGeneratedVariations);
                        break;
                    case BottleneckState.Clogged:
                        plugin.CloggedColorConfig.Value = hex;
                        FoundationIntegration.CloggedVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Clogged, _currentGeneratedVariations);
                        break;
                    case BottleneckState.FullStorage:
                        plugin.FullStorageColorConfig.Value = hex;
                        FoundationIntegration.FullStorageVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.FullStorage, _currentGeneratedVariations);
                        break;
                    case BottleneckState.Idle:
                        plugin.IdleColorConfig.Value = hex;
                        FoundationIntegration.IdleVariations = _currentGeneratedVariations;
                        FoundationIntegration.UpdateDropdownOptions(BottleneckState.Idle, _currentGeneratedVariations);
                        break;
                }

                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();

                try
                {
                    string stateName = GetStateDisplayName(_currentState);
                    var msg = Assets.Behaviour.UI.UIStatusMessage.Show(
                        $"Bottleneck Heatmap: Updated {stateName} Color",
                        "Icons_17",
                        false
                    );
                    if (msg != null && msg.StatusText != null)
                    {
                        msg.StatusText.text = $"Bottleneck Heatmap: Updated {stateName} (<color={hex}>{hex}</color>)";
                    }
                }
                catch { }
            }

            Close();
        }

        void ResetStateDefault()
        {
            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin != null)
            {
                string defHex = _currentState switch
                {
                    BottleneckState.Running => "#26FA73",
                    BottleneckState.Starved => "#FFD92E",
                    BottleneckState.Clogged => "#FF3842",
                    BottleneckState.FullStorage => "#B042FF",
                    BottleneckState.Idle => "#8C9EB8",
                    _ => "#26FA73"
                };

                ColorUtility.TryParseHtmlString(defHex, out var defColor);
                Color.RGBToHSV(defColor, out _baseHue, out _baseSaturation, out _baseValue);
                UpdateSurfaces();
                UpdateGeneratedVariations();
                ApplyCurrentColor();
            }
        }

        void BuildUI()
        {
            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // 1. Semi-transparent blocker backdrop
            var backdrop = new GameObject("Backdrop", typeof(RectTransform), typeof(Image), typeof(Button));
            backdrop.transform.SetParent(transform, false);
            var bRt = (RectTransform)backdrop.transform;
            bRt.anchorMin = Vector2.zero;
            bRt.anchorMax = Vector2.one;
            bRt.offsetMin = Vector2.zero;
            bRt.offsetMax = Vector2.zero;
            var bImg = backdrop.GetComponent<Image>();
            bImg.color = new Color(0f, 0f, 0f, 0.65f);
            var bBtn = backdrop.GetComponent<Button>();
            bBtn.onClick.AddListener(Close);

            // 2. Main dialog frame
            var dialog = new GameObject("DialogFrame", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(transform, false);
            var dRt = (RectTransform)dialog.transform;
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.pivot = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(600f, 460f);
            var dImg = dialog.GetComponent<Image>();
            dImg.color = new Color(0.12f, 0.11f, 0.14f, 0.98f);

            // Border outline
            var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
            border.transform.SetParent(dialog.transform, false);
            var brt = (RectTransform)border.transform;
            brt.anchorMin = Vector2.zero;
            brt.anchorMax = Vector2.one;
            brt.offsetMin = new Vector2(-2, -2);
            brt.offsetMax = new Vector2(2, 2);
            var bBorder = border.GetComponent<Image>();
            bBorder.color = new Color(0.38f, 0.33f, 0.25f, 1f);
            border.transform.SetAsFirstSibling();

            // Header Title
            CreateText(dialog.transform, "Title", "Heatmap Color Variations Generator", new Vector2(24, -16), new Vector2(400, 26), 18, Color.yellow, FontStyles.Bold);
            _headerSubtitle = CreateText(dialog.transform, "Subtitle", "Select base color to auto-generate slight variation hues", new Vector2(24, -40), new Vector2(500, 20), 12, new Color(0.75f, 0.72f, 0.65f), FontStyles.Normal);

            // Close button [X]
            CreateButton(dialog.transform, "CloseBtn", "X", new Vector2(550, -16), new Vector2(28, 28), new Color(0.25f, 0.20f, 0.20f), Color.white, Close);

            // State Selection Tabs
            float tabStartX = 24f;
            float tabWidth = 106f;
            float tabY = -66f;

            _tabRunning = CreateTabButton(dialog.transform, "Tab_Running", "Running", new Vector2(tabStartX + 0 * (tabWidth + 4), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Running));
            _tabStarved = CreateTabButton(dialog.transform, "Tab_Starved", "Starved", new Vector2(tabStartX + 1 * (tabWidth + 4), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Starved));
            _tabClogged = CreateTabButton(dialog.transform, "Tab_Clogged", "Clogged", new Vector2(tabStartX + 2 * (tabWidth + 4), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Clogged));
            _tabFullStorage = CreateTabButton(dialog.transform, "Tab_Full", "Full Storage", new Vector2(tabStartX + 3 * (tabWidth + 4), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.FullStorage));
            _tabIdle = CreateTabButton(dialog.transform, "Tab_Idle", "Idle", new Vector2(tabStartX + 4 * (tabWidth + 4), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Idle));

            // Color Picker Area
            // SV Surface (210 x 110)
            var svObj = new GameObject("SVSurface", typeof(RectTransform), typeof(PickerSurface));
            svObj.transform.SetParent(dialog.transform, false);
            var svRt = (RectTransform)svObj.transform;
            svRt.anchorMin = new Vector2(0f, 1f);
            svRt.anchorMax = new Vector2(0f, 1f);
            svRt.pivot = new Vector2(0f, 1f);
            svRt.anchoredPosition = new Vector2(24, -100);
            svRt.sizeDelta = new Vector2(210, 110);
            _svSurface = svObj.GetComponent<PickerSurface>();
            _svSurface.Mode = PickerMode.SaturationValue;
            _svSurface.Changed = p =>
            {
                _baseSaturation = p.x;
                _baseValue = p.y;
                UpdateSurfaces();
                UpdateGeneratedVariations();
            };

            // Hue Strip (24 x 110)
            var hueObj = new GameObject("HueStrip", typeof(RectTransform), typeof(PickerSurface));
            hueObj.transform.SetParent(dialog.transform, false);
            var hueRt = (RectTransform)hueObj.transform;
            hueRt.anchorMin = new Vector2(0f, 1f);
            hueRt.anchorMax = new Vector2(0f, 1f);
            hueRt.pivot = new Vector2(0f, 1f);
            hueRt.anchoredPosition = new Vector2(244, -100);
            hueRt.sizeDelta = new Vector2(24, 110);
            _hueSurface = hueObj.GetComponent<PickerSurface>();
            _hueSurface.Mode = PickerMode.Hue;
            _hueSurface.Changed = p =>
            {
                _baseHue = 1f - p.y;
                UpdateSurfaces();
                UpdateGeneratedVariations();
            };

            // Selected Base Color Preview Box
            var basePrevObj = new GameObject("BaseColorPreview", typeof(RectTransform), typeof(Image));
            basePrevObj.transform.SetParent(dialog.transform, false);
            var bpRt = (RectTransform)basePrevObj.transform;
            bpRt.anchorMin = new Vector2(0f, 1f);
            bpRt.anchorMax = new Vector2(0f, 1f);
            bpRt.pivot = new Vector2(0f, 1f);
            bpRt.anchoredPosition = new Vector2(280, -100);
            bpRt.sizeDelta = new Vector2(50, 48);
            _baseColorPreview = basePrevObj.GetComponent<Image>();
            _baseColorPreview.color = Color.green;

            _baseHexText = CreateText(dialog.transform, "BaseHex", "#26FA73", new Vector2(280, -154), new Vector2(60, 20), 12, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Quick Presets
            CreateText(dialog.transform, "PresetsLabel", "Quick Presets:", new Vector2(350, -100), new Vector2(100, 16), 11, Color.white, FontStyles.Bold);

            float pX = 350f;
            float pY = -120f;
            for (int i = 0; i < Presets.Length; i++)
            {
                string hex = Presets[i];
                ColorUtility.TryParseHtmlString("#" + hex, out var pColor);
                CreateColorButton(dialog.transform, $"Preset_{hex}", pColor, new Vector2(pX + (i % 5) * 44f, pY - (i / 5) * 32f), new Vector2(38f, 26f), () =>
                {
                    Color.RGBToHSV(pColor, out _baseHue, out _baseSaturation, out _baseValue);
                    UpdateSurfaces();
                    UpdateGeneratedVariations();
                });
            }

            // Section 2: Surrounding Hues Cards (Slight Variations)
            _variationsTitle = CreateText(dialog.transform, "VariationsLabel", "Auto-Generated Surrounding Hues:", new Vector2(24, -226), new Vector2(480, 20), 13, Color.yellow, FontStyles.Bold);

            float cardStartX = 24f;
            float cardSpacing = 112f;
            float cardY = -250f;

            for (int i = 0; i < 5; i++)
            {
                int cardIdx = i;
                CreateVariationCard(dialog.transform, $"VarCard_{i}", new Vector2(cardStartX + i * cardSpacing, cardY), out _cards[i], () =>
                {
                    // Clicking a card adopts that variation as the base!
                    Color.RGBToHSV(_cards[cardIdx].Color, out _baseHue, out _baseSaturation, out _baseValue);
                    UpdateSurfaces();
                    UpdateGeneratedVariations();
                });
            }

            // Section 3: Bottom Action Buttons
            _applyButton = CreateButton(dialog.transform, "ApplyBtn", "Apply to State", new Vector2(24, -412), new Vector2(180, 34), new Color(0.20f, 0.48f, 0.28f), Color.white, ApplyCurrentColor);
            CreateButton(dialog.transform, "ResetBtn", "Reset State Default", new Vector2(220, -412), new Vector2(170, 34), new Color(0.28f, 0.28f, 0.32f), Color.white, ResetStateDefault);
            CreateButton(dialog.transform, "CancelBtn", "Close", new Vector2(406, -412), new Vector2(170, 34), new Color(0.25f, 0.20f, 0.20f), Color.white, Close);
        }

        Button CreateTabButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Action onClick)
        {
            var btn = CreateButton(parent, name, label, pos, size, new Color(0.18f, 0.17f, 0.20f, 0.95f), new Color(0.75f, 0.72f, 0.68f), onClick);
            var tmp = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmp) tmp.fontSize = 12;
            return btn;
        }

        void CreateVariationCard(Transform parent, string name, Vector2 pos, out VariationCard card, Action onCardClicked)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(104f, 146f);

            var bg = root.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.17f, 0.20f, 0.95f);

            var btn = root.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() => onCardClicked?.Invoke());

            // Swatch Box
            var sObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            sObj.transform.SetParent(root.transform, false);
            var sRt = (RectTransform)sObj.transform;
            sRt.anchorMin = new Vector2(0.5f, 1f);
            sRt.anchorMax = new Vector2(0.5f, 1f);
            sRt.pivot = new Vector2(0.5f, 1f);
            sRt.anchoredPosition = new Vector2(0, -8);
            sRt.sizeDelta = new Vector2(86, 52);
            var swatch = sObj.GetComponent<Image>();
            swatch.color = Color.white;

            // Variation Title
            var title = CreateText(root.transform, "Title", "Variation", new Vector2(0, -66), new Vector2(104, 30), 11, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            title.textWrappingMode = TextWrappingModes.Normal;

            // Hex Code
            var hex = CreateText(root.transform, "Hex", "#FFFFFF", new Vector2(0, -118), new Vector2(104, 18), 11, new Color(1f, 0.90f, 0.50f), FontStyles.Bold, TextAlignmentOptions.Center);

            card = new VariationCard
            {
                Root = root,
                Swatch = swatch,
                Title = title,
                Hex = hex,
                Color = Color.white
            };
        }

        TMP_Text CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, Color color, FontStyles style, TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        Button CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color bgColor, Color textColor, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = bgColor;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());

            var textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(go.transform, false);
            var tRt = (RectTransform)textObj.transform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = Vector2.zero;
            tRt.offsetMax = Vector2.zero;

            var tmp = textObj.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 13;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Overflow;

            return btn;
        }

        Button CreateColorButton(Transform parent, string name, Color color, Vector2 pos, Vector2 size, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            var img = go.GetComponent<Image>();
            img.color = color;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => onClick?.Invoke());
            return btn;
        }
    }
}
