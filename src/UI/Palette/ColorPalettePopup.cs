using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed class ColorPalettePopup : MonoBehaviour
    {
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
                    var msg = FoundationApi.UiAccess.ShowStatusText($"Bottleneck Heatmap: Updated {stateName} Color", "Icons_17", false);
                    if (msg != null && msg != null)
                    {
                        msg.text = $"Bottleneck Heatmap: Updated {stateName} (<color={hex}>{hex}</color>)";
                    }
                }
                catch
                {
                }
            }

            Close();
        }

        void Awake()
        {
            try
            {
                VanillaTheme.Ensure();
                BuildUI();
            }
            catch (Exception ex)
            {
                BottleneckHeatmapPlugin.Instance?.Log?.LogError($"ColorPalettePopup.Awake error: {ex}");
            }
        }

        void BuildUI()
        {
            VanillaTheme.Ensure();
            var rt = (RectTransform)transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            // 1. Blocker backdrop
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
            // 2. Main dialog frame (matching Kingdom Inc window frame)
            var dialog = new GameObject("DialogFrame", typeof(RectTransform), typeof(Image));
            dialog.transform.SetParent(transform, false);
            var dRt = (RectTransform)dialog.transform;
            dRt.anchorMin = new Vector2(0.5f, 0.5f);
            dRt.anchorMax = new Vector2(0.5f, 0.5f);
            dRt.pivot = new Vector2(0.5f, 0.5f);
            dRt.sizeDelta = new Vector2(620f, 470f);
            var dImg = dialog.GetComponent<Image>();
            if (VanillaTheme.WindowSprite != null)
            {
                dImg.sprite = VanillaTheme.WindowSprite;
                dImg.type = Image.Type.Sliced;
                dImg.color = Color.white;
            }
            else
            {
                dImg.color = new Color(0.14f, 0.12f, 0.16f, 0.98f);
                // Outer gold border fallback
                var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
                border.transform.SetParent(dialog.transform, false);
                var brt = (RectTransform)border.transform;
                brt.anchorMin = Vector2.zero;
                brt.anchorMax = Vector2.one;
                brt.offsetMin = new Vector2(-2, -2);
                brt.offsetMax = new Vector2(2, 2);
                var bBorder = border.GetComponent<Image>();
                bBorder.color = new Color(0.48f, 0.40f, 0.28f, 1f);
                border.transform.SetAsFirstSibling();
            }

            // Header Title
            CreateText(dialog.transform, "Title", "Heatmap Color Variations Generator", new Vector2(24, -18), new Vector2(480, 26), 18, new Color(1f, 0.85f, 0.18f), FontStyles.Bold);
            _headerSubtitle = CreateText(dialog.transform, "Subtitle", "Select base color to auto-generate slight variation hues", new Vector2(24, -42), new Vector2(500, 20), 12, new Color(0.78f, 0.74f, 0.68f), FontStyles.Normal);
            // Close button [X]
            CreateCloseButton(dialog.transform, new Vector2(-16, -16), Close);
            // State Selection Tabs
            float tabStartX = 24f;
            float tabWidth = 110f;
            float tabY = -68f;
            _tabRunning = CreateTabButton(dialog.transform, "Tab_Running", "Running", new Vector2(tabStartX + 0 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Running));
            _tabStarved = CreateTabButton(dialog.transform, "Tab_Starved", "Starved", new Vector2(tabStartX + 1 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Starved));
            _tabClogged = CreateTabButton(dialog.transform, "Tab_Clogged", "Clogged", new Vector2(tabStartX + 2 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Clogged));
            _tabFullStorage = CreateTabButton(dialog.transform, "Tab_Full", "Full Storage", new Vector2(tabStartX + 3 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.FullStorage));
            _tabIdle = CreateTabButton(dialog.transform, "Tab_Idle", "Idle", new Vector2(tabStartX + 4 * (tabWidth + 5), tabY), new Vector2(tabWidth, 26), () => SelectState(BottleneckState.Idle));
            // Color Picker Area Frame
            // SV Surface (210 x 110)
            var svBorder = CreatePanelBox(dialog.transform, "SVBorder", new Vector2(24, -102), new Vector2(212, 112));
            var svObj = new GameObject("SVSurface", typeof(RectTransform), typeof(PickerSurface));
            svObj.transform.SetParent(svBorder.transform, false);
            var svRt = (RectTransform)svObj.transform;
            svRt.anchorMin = Vector2.zero;
            svRt.anchorMax = Vector2.one;
            svRt.offsetMin = new Vector2(1, 1);
            svRt.offsetMax = new Vector2(-1, -1);
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
            var hueBorder = CreatePanelBox(dialog.transform, "HueBorder", new Vector2(244, -102), new Vector2(26, 112));
            var hueObj = new GameObject("HueStrip", typeof(RectTransform), typeof(PickerSurface));
            hueObj.transform.SetParent(hueBorder.transform, false);
            var hueRt = (RectTransform)hueObj.transform;
            hueRt.anchorMin = Vector2.zero;
            hueRt.anchorMax = Vector2.one;
            hueRt.offsetMin = new Vector2(1, 1);
            hueRt.offsetMax = new Vector2(-1, -1);
            _hueSurface = hueObj.GetComponent<PickerSurface>();
            _hueSurface.Mode = PickerMode.Hue;
            _hueSurface.Changed = p =>
            {
                _baseHue = 1f - p.y;
                UpdateSurfaces();
                UpdateGeneratedVariations();
            };
            // Selected Base Color Preview Box
            var bpBorder = CreatePanelBox(dialog.transform, "BaseColorBorder", new Vector2(280, -102), new Vector2(52, 50));
            var basePrevObj = new GameObject("BaseColorPreview", typeof(RectTransform), typeof(Image));
            basePrevObj.transform.SetParent(bpBorder.transform, false);
            var bpRt = (RectTransform)basePrevObj.transform;
            bpRt.anchorMin = Vector2.zero;
            bpRt.anchorMax = Vector2.one;
            bpRt.offsetMin = new Vector2(2, 2);
            bpRt.offsetMax = new Vector2(-2, -2);
            _baseColorPreview = basePrevObj.GetComponent<Image>();
            _baseColorPreview.color = Color.green;
            _baseHexText = CreateText(dialog.transform, "BaseHex", "#26FA73", new Vector2(280, -156), new Vector2(62, 20), 12, new Color(1f, 0.85f, 0.18f), FontStyles.Bold, TextAlignmentOptions.Left);
            // Quick Presets
            CreateText(dialog.transform, "PresetsLabel", "Quick Presets:", new Vector2(352, -102), new Vector2(100, 16), 11, new Color(0.85f, 0.82f, 0.76f), FontStyles.Bold);
            float pX = 352f;
            float pY = -122f;
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
            _variationsTitle = CreateText(dialog.transform, "VariationsLabel", "Auto-Generated Surrounding Hues:", new Vector2(24, -228), new Vector2(480, 20), 13, new Color(1f, 0.85f, 0.18f), FontStyles.Bold);
            float cardStartX = 24f;
            float cardSpacing = 116f;
            float cardY = -252f;
            for (int i = 0; i < 5; i++)
            {
                int cardIdx = i;
                CreateVariationCard(dialog.transform, $"VarCard_{i}", new Vector2(cardStartX + i * cardSpacing, cardY), out _cards[i], () =>
                {
                    Color.RGBToHSV(_cards[cardIdx].Color, out _baseHue, out _baseSaturation, out _baseValue);
                    UpdateSurfaces();
                    UpdateGeneratedVariations();
                });
            }

            // Section 3: Bottom Action Buttons
            _applyButton = CreateThemedButton(dialog.transform, "ApplyBtn", "Apply to State", new Vector2(24, -420), new Vector2(184, 36), ApplyCurrentColor);
            CreateThemedButton(dialog.transform, "ResetBtn", "Reset State Default", new Vector2(220, -420), new Vector2(174, 36), ResetStateDefault);
            CreateThemedButton(dialog.transform, "CancelBtn", "Close", new Vector2(406, -420), new Vector2(174, 36), Close);
        }

        public static void Close()
        {
            if (_instance)
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
                _instance.gameObject.SetActive(false);
            }
        }

        void CreateCloseButton(Transform parent, Vector2 pos, Action onClick)
        {
            var closeObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            closeObj.transform.SetParent(parent, false);
            var rt = (RectTransform)closeObj.transform;
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(28f, 28f);
            var img = closeObj.GetComponent<Image>();
            if (VanillaTheme.CloseButtonSprite != null)
            {
                img.sprite = VanillaTheme.CloseButtonSprite;
                img.color = Color.white;
            }
            else if (VanillaTheme.ButtonSprite != null)
            {
                img.sprite = VanillaTheme.ButtonSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.35f, 0.18f, 0.18f, 1f);
            }

            var btn = closeObj.GetComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.2f, 0.8f, 0.8f, 1f);
            colors.pressedColor = new Color(0.8f, 0.5f, 0.5f, 1f);
            btn.colors = colors;
            var textObj = new GameObject("X", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(closeObj.transform, false);
            var t = textObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                t.font = VanillaTheme.Font;
                t.fontSharedMaterial = VanillaTheme.FontMaterial;
            }

            t.text = "✕";
            t.fontSize = 16;
            t.fontStyle = FontStyles.Bold;
            t.color = new Color(0.95f, 0.85f, 0.65f);
            t.alignment = TextAlignmentOptions.Center;
            t.raycastTarget = false;
            var trt = (RectTransform)textObj.transform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = trt.offsetMax = Vector2.zero;
            btn.onClick.AddListener(() =>
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
                onClick?.Invoke();
            });
        }

        void CreateColorButton(Transform parent, string name, Color color, Vector2 pos, Vector2 size, Action onClick)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var borderImg = go.GetComponent<Image>();
            borderImg.color = new Color(0.32f, 0.28f, 0.22f, 1f);
            var innerObj = new GameObject("Inner", typeof(RectTransform), typeof(Image));
            innerObj.transform.SetParent(go.transform, false);
            var irt = (RectTransform)innerObj.transform;
            irt.anchorMin = Vector2.zero;
            irt.anchorMax = Vector2.one;
            irt.offsetMin = new Vector2(2, 2);
            irt.offsetMax = new Vector2(-2, -2);
            var innerImg = innerObj.GetComponent<Image>();
            innerImg.color = color;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = borderImg;
            btn.onClick.AddListener(() =>
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
                onClick?.Invoke();
            });
        }

        GameObject CreatePanelBox(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = go.GetComponent<Image>();
            img.color = new Color(0.36f, 0.32f, 0.24f, 1f); // Dark gold border
            return go;
        }

        Button CreateTabButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Action onClick)
        {
            var btn = CreateThemedButton(parent, name, label, pos, size, onClick);
            var tmp = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmp)
            {
                tmp.fontSizeMin = 10;
                tmp.fontSizeMax = 13;
            }

            return btn;
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
            if (VanillaTheme.Font != null)
            {
                tmp.font = VanillaTheme.Font;
                tmp.fontSharedMaterial = VanillaTheme.FontMaterial;
            }

            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            return tmp;
        }

        Button CreateThemedButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Action onClick)
        {
            var btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(parent, false);
            var rt = (RectTransform)btnObj.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = btnObj.GetComponent<Image>();
            if (VanillaTheme.ButtonSprite != null)
            {
                img.sprite = VanillaTheme.ButtonSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.24f, 0.20f, 0.16f, 1f);
            }

            var btn = btnObj.GetComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.10f, 1f);
            colors.pressedColor = new Color(0.80f, 0.75f, 0.70f, 1f);
            btn.colors = colors;
            var textObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(btnObj.transform, false);
            var tRt = (RectTransform)textObj.transform;
            tRt.anchorMin = Vector2.zero;
            tRt.anchorMax = Vector2.one;
            tRt.offsetMin = new Vector2(4, 2);
            tRt.offsetMax = new Vector2(-4, -2);
            var btnText = textObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                btnText.font = VanillaTheme.Font;
                btnText.fontSharedMaterial = VanillaTheme.FontMaterial;
            }

            btnText.text = label;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontSizeMin = 10;
            btnText.fontSizeMax = 14;
            btnText.enableAutoSizing = true;
            btnText.color = new Color(0.96f, 0.94f, 0.90f);
            btnText.raycastTarget = false;
            btn.onClick.AddListener(() =>
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
                onClick?.Invoke();
            });
            return btn;
        }

        void CreateVariationCard(Transform parent, string name, Vector2 pos, out global::KingdomBottleneckHeatmap.VariationCard card, Action onCardClicked)
        {
            var root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            root.transform.SetParent(parent, false);
            var rt = (RectTransform)root.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(108f, 150f);
            var bg = root.GetComponent<Image>();
            if (VanillaTheme.ButtonSprite != null)
            {
                bg.sprite = VanillaTheme.ButtonSprite;
                bg.type = Image.Type.Sliced;
                bg.color = new Color(0.85f, 0.82f, 0.78f, 1f);
            }
            else
            {
                bg.color = new Color(0.18f, 0.16f, 0.20f, 0.98f);
            }

            var btn = root.GetComponent<Button>();
            btn.targetGraphic = bg;
            btn.onClick.AddListener(() =>
            {
                FoundationApi.UiAccess.PlaySound(GameUiSound.TurnPage);
                onCardClicked?.Invoke();
            });
            // Card Swatch Box with inset border
            var sBorder = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            sBorder.transform.SetParent(root.transform, false);
            var sbRt = (RectTransform)sBorder.transform;
            sbRt.anchorMin = new Vector2(0.5f, 1f);
            sbRt.anchorMax = new Vector2(0.5f, 1f);
            sbRt.pivot = new Vector2(0.5f, 1f);
            sbRt.anchoredPosition = new Vector2(0, -8);
            sbRt.sizeDelta = new Vector2(92, 54);
            var sbImg = sBorder.GetComponent<Image>();
            sbImg.color = new Color(0.32f, 0.28f, 0.22f, 1f);
            var sObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            sObj.transform.SetParent(sBorder.transform, false);
            var sRt = (RectTransform)sObj.transform;
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(2, 2);
            sRt.offsetMax = new Vector2(-2, -2);
            var swatch = sObj.GetComponent<Image>();
            swatch.color = Color.white;
            // Variation Title
            var title = CreateText(root.transform, "Title", "Variation", new Vector2(0, -68), new Vector2(108, 30), 11, new Color(0.92f, 0.90f, 0.85f), FontStyles.Bold, TextAlignmentOptions.Center);
            title.textWrappingMode = TextWrappingModes.Normal;
            // Hex Code
            var hex = CreateText(root.transform, "Hex", "#FFFFFF", new Vector2(0, -120), new Vector2(108, 18), 11, new Color(1f, 0.85f, 0.18f), FontStyles.Bold, TextAlignmentOptions.Center);
            card = new global::KingdomBottleneckHeatmap.VariationCard
            {
                Root = root,
                Border = sbImg,
                Swatch = swatch,
                Title = title,
                Hex = hex,
                Color = Color.white
            };
        }

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
        readonly global::KingdomBottleneckHeatmap.VariationCard[] _cards = new global::KingdomBottleneckHeatmap.VariationCard[5];
        (string Label, string Hex)[] _currentGeneratedVariations;
        static readonly string[] Presets = new string[]
        {
            "26FA73",
            "00FFB2",
            "00D9FF",
            "2E75FF",
            "8C28FF",
            "B042FF",
            "DF2BFF",
            "FF2B85",
            "FF3842",
            "FFD92E"
        };
        public static void Open(BottleneckState targetState = BottleneckState.Running)
        {
            try
            {
                EnsureInstance();
                if (!_instance)
                {
                    BottleneckHeatmapPlugin.Instance?.Log?.LogError("ColorPalettePopup.Open: instance failed to create");
                    return;
                }

                _instance.gameObject.SetActive(true);
                _instance.transform.SetAsLastSibling();
                _instance.SelectState(targetState);
            }
            catch (Exception ex)
            {
                BottleneckHeatmapPlugin.Instance?.Log?.LogError($"ColorPalettePopup.Open error: {ex}");
            }
        }

        static void EnsureInstance()
        {
            if (_instance && _instance.gameObject)
                return;
            var go = new GameObject("BottleneckColorPalettePopup", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster), typeof(CanvasScaler));
            global::UnityEngine.Object.DontDestroyOnLoad(go);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 32000;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            _instance = go.AddComponent<ColorPalettePopup>();
        }

        string GetStateDisplayName(BottleneckState state) => state switch
        {
            BottleneckState.Running => "Running",
            BottleneckState.Starved => "Starved",
            BottleneckState.Clogged => "Clogged",
            BottleneckState.FullStorage => "Full Storage",
            BottleneckState.Idle => "Idle",
            _ => state.ToString()};
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
            if (_baseSaturation < 0.2f && state != BottleneckState.Idle)
                _baseSaturation = 0.85f;
            if (_baseValue < 0.4f)
                _baseValue = 0.95f;
            UpdateTabsVisuals();
            UpdateSurfaces();
            UpdateGeneratedVariations();
            string stateName = GetStateDisplayName(_currentState);
            if (_headerSubtitle)
                _headerSubtitle.text = $"Select base color for <color=#FFD92E>{stateName}</color> to auto-generate slight variation hues";
            if (_variationsTitle)
                _variationsTitle.text = $"Auto-Generated Surrounding Hues for {stateName}:";
            if (_applyButton)
            {
                var tmp = _applyButton.GetComponentInChildren<TMP_Text>(true);
                if (tmp)
                    tmp.text = $"Apply to {stateName}";
            }
        }

        void SetCard(int idx, string title, Color col, string hex)
        {
            if (idx >= _cards.Length)
                return;
            _cards[idx].Color = col;
            if (_cards[idx].Swatch)
                _cards[idx].Swatch.color = col;
            if (_cards[idx].Title)
                _cards[idx].Title.text = title;
            if (_cards[idx].Hex)
                _cards[idx].Hex.text = hex;
        }

        void SetTabActive(Button tab, bool active)
        {
            if (!tab)
                return;
            var img = tab.GetComponent<Image>();
            if (img)
            {
                img.color = active ? new Color(1f, 0.95f, 0.85f, 1f) : new Color(0.70f, 0.68f, 0.65f, 0.9f);
            }

            var text = tab.GetComponentInChildren<TMP_Text>(true);
            if (text)
            {
                text.color = active ? new Color(1f, 0.85f, 0.18f, 1f) : new Color(0.78f, 0.74f, 0.68f, 1f);
                text.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Close();
            }
        }

        void UpdateGeneratedVariations()
        {
            Color baseColor = Color.HSVToRGB(_baseHue, Mathf.Clamp01(_baseSaturation), Mathf.Clamp01(_baseValue));
            if (_baseColorPreview)
                _baseColorPreview.color = baseColor;
            if (_baseHexText)
                _baseHexText.text = "#" + ColorUtility.ToHtmlStringRGB(baseColor);
            string stateName = GetStateDisplayName(_currentState);
            _currentGeneratedVariations = FoundationIntegration.GenerateVariations(baseColor, stateName);
            string[] cardTitles =
            {
                "Selected Base",
                "Warm Tone",
                "Cool Tone",
                "Bright Tone",
                "Deep Tone"
            };
            for (int i = 0; i < _cards.Length && i < _currentGeneratedVariations.Length; i++)
            {
                ColorUtility.TryParseHtmlString(_currentGeneratedVariations[i].Hex, out var col);
                SetCard(i, cardTitles[i], col, _currentGeneratedVariations[i].Hex);
            }
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

        void UpdateTabsVisuals()
        {
            SetTabActive(_tabRunning, _currentState == BottleneckState.Running);
            SetTabActive(_tabStarved, _currentState == BottleneckState.Starved);
            SetTabActive(_tabClogged, _currentState == BottleneckState.Clogged);
            SetTabActive(_tabFullStorage, _currentState == BottleneckState.FullStorage);
            SetTabActive(_tabIdle, _currentState == BottleneckState.Idle);
        }
    }
}
