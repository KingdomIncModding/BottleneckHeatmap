using System;
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

        // Current base color in HSV
        float _baseHue = 0.38f;        // Default green (~137 deg)
        float _baseSaturation = 0.85f;
        float _baseValue = 0.98f;

        // UI references
        PickerSurface _svSurface;
        PickerSurface _hueSurface;
        Image _baseColorPreview;
        TMP_Text _baseHexText;

        // 5 Auto-generated state previews
        Image _runningSwatch;
        TMP_Text _runningHex;
        Image _starvedSwatch;
        TMP_Text _starvedHex;
        Image _cloggedSwatch;
        TMP_Text _cloggedHex;
        Image _fullStorageSwatch;
        TMP_Text _fullStorageHex;
        Image _idleSwatch;
        TMP_Text _idleHex;

        // Computed colors
        Color _compRunning;
        Color _compStarved;
        Color _compClogged;
        Color _compFullStorage;
        Color _compIdle;

        static readonly string[] Presets = new string[]
        {
            "26FA73", "00FFB2", "00D9FF", "2E75FF", "8C28FF",
            "B042FF", "DF2BFF", "FF2B85", "FF3842", "FFD92E"
        };

        public static void Open()
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
            _instance.InitializeFromCurrentColors();
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

        void InitializeFromCurrentColors()
        {
            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin != null)
            {
                Color run = HeatmapMaterials.RunningColor;
                Color.RGBToHSV(run, out _baseHue, out _baseSaturation, out _baseValue);
                if (_baseSaturation < 0.4f) _baseSaturation = 0.85f;
                if (_baseValue < 0.5f) _baseValue = 0.98f;
            }

            UpdateSurfaces();
            UpdateGeneratedColors();
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

        void UpdateGeneratedColors()
        {
            // 1. Running: base color chosen by user
            _compRunning = Color.HSVToRGB(_baseHue, Mathf.Clamp01(_baseSaturation), Mathf.Clamp01(_baseValue));

            // 2. Starved: analogous warm neighbor (-44 deg on wheel)
            float starvedHue = (_baseHue - 0.12f + 1f) % 1f;
            _compStarved = Color.HSVToRGB(starvedHue, Mathf.Clamp01(_baseSaturation * 0.92f), 1f);

            // 3. Clogged: contrasting alert complement (+155 deg on wheel)
            float cloggedHue = (_baseHue + 0.43f) % 1f;
            _compClogged = Color.HSVToRGB(cloggedHue, 0.92f, 1f);

            // 4. Full Storage: triadic accent (+100 deg on wheel)
            float fullHue = (_baseHue + 0.28f) % 1f;
            _compFullStorage = Color.HSVToRGB(fullHue, 0.85f, 1f);

            // 5. Idle: desaturated harmonious tint
            _compIdle = Color.HSVToRGB(_baseHue, 0.22f, 0.65f);

            // Update UI preview
            if (_baseColorPreview) _baseColorPreview.color = _compRunning;
            if (_baseHexText) _baseHexText.text = "#" + ColorUtility.ToHtmlStringRGB(_compRunning);

            if (_runningSwatch) _runningSwatch.color = _compRunning;
            if (_runningHex) _runningHex.text = "#" + ColorUtility.ToHtmlStringRGB(_compRunning);

            if (_starvedSwatch) _starvedSwatch.color = _compStarved;
            if (_starvedHex) _starvedHex.text = "#" + ColorUtility.ToHtmlStringRGB(_compStarved);

            if (_cloggedSwatch) _cloggedSwatch.color = _compClogged;
            if (_cloggedHex) _cloggedHex.text = "#" + ColorUtility.ToHtmlStringRGB(_compClogged);

            if (_fullStorageSwatch) _fullStorageSwatch.color = _compFullStorage;
            if (_fullStorageHex) _fullStorageHex.text = "#" + ColorUtility.ToHtmlStringRGB(_compFullStorage);

            if (_idleSwatch) _idleSwatch.color = _compIdle;
            if (_idleHex) _idleHex.text = "#" + ColorUtility.ToHtmlStringRGB(_compIdle);
        }

        void ApplyPalette()
        {
            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin != null)
            {
                plugin.RunningColorConfig.Value = "#" + ColorUtility.ToHtmlStringRGB(_compRunning);
                plugin.StarvedColorConfig.Value = "#" + ColorUtility.ToHtmlStringRGB(_compStarved);
                plugin.CloggedColorConfig.Value = "#" + ColorUtility.ToHtmlStringRGB(_compClogged);
                plugin.FullStorageColorConfig.Value = "#" + ColorUtility.ToHtmlStringRGB(_compFullStorage);
                plugin.IdleColorConfig.Value = "#" + ColorUtility.ToHtmlStringRGB(_compIdle);

                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();

                try
                {
                    var msg = Assets.Behaviour.UI.UIStatusMessage.Show(
                        "Bottleneck Heatmap: Palette Applied",
                        "Icons_17",
                        false
                    );
                    if (msg != null && msg.StatusText != null)
                    {
                        string rHex = plugin.RunningColorConfig.Value;
                        string sHex = plugin.StarvedColorConfig.Value;
                        string cHex = plugin.CloggedColorConfig.Value;
                        string fHex = plugin.FullStorageColorConfig.Value;
                        msg.StatusText.text = $"Bottleneck Heatmap: Palette Applied (<color={rHex}>Running</color>  <color={sHex}>Starved</color>  <color={cHex}>Clogged</color>  <color={fHex}>Full</color>)";
                    }
                }
                catch { }
            }

            Close();
        }

        void ResetDefaults()
        {
            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin != null)
            {
                plugin.RunningColorConfig.Value = "#26FA73";
                plugin.StarvedColorConfig.Value = "#FFD92E";
                plugin.CloggedColorConfig.Value = "#FF3842";
                plugin.FullStorageColorConfig.Value = "#B042FF";
                plugin.IdleColorConfig.Value = "#8C9EB8";

                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            }

            InitializeFromCurrentColors();
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
            dRt.sizeDelta = new Vector2(580f, 440f);
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
            CreateText(dialog.transform, "Title", "Heatmap Palette Generator", new Vector2(24, -18), new Vector2(400, 26), 18, Color.yellow, FontStyles.Bold);
            CreateText(dialog.transform, "Subtitle", "Pick a base color to auto-generate harmonized bottleneck hues", new Vector2(24, -42), new Vector2(480, 20), 12, new Color(0.75f, 0.72f, 0.65f), FontStyles.Normal);

            // Close button [X]
            CreateButton(dialog.transform, "CloseBtn", "X", new Vector2(534, -18), new Vector2(28, 28), new Color(0.25f, 0.20f, 0.20f), Color.white, Close);

            // Section 1: Color Picker Surface
            CreateText(dialog.transform, "PickerLabel", "1. Pick Base Color:", new Vector2(24, -68), new Vector2(200, 18), 13, Color.white, FontStyles.Bold);

            // SV Surface (210 x 110)
            var svObj = new GameObject("SVSurface", typeof(RectTransform), typeof(PickerSurface));
            svObj.transform.SetParent(dialog.transform, false);
            var svRt = (RectTransform)svObj.transform;
            svRt.anchorMin = new Vector2(0f, 1f);
            svRt.anchorMax = new Vector2(0f, 1f);
            svRt.pivot = new Vector2(0f, 1f);
            svRt.anchoredPosition = new Vector2(24, -90);
            svRt.sizeDelta = new Vector2(210, 110);
            _svSurface = svObj.GetComponent<PickerSurface>();
            _svSurface.Mode = PickerMode.SaturationValue;
            _svSurface.Changed = p =>
            {
                _baseSaturation = p.x;
                _baseValue = p.y;
                UpdateSurfaces();
                UpdateGeneratedColors();
            };

            // Hue Strip (24 x 110)
            var hueObj = new GameObject("HueStrip", typeof(RectTransform), typeof(PickerSurface));
            hueObj.transform.SetParent(dialog.transform, false);
            var hueRt = (RectTransform)hueObj.transform;
            hueRt.anchorMin = new Vector2(0f, 1f);
            hueRt.anchorMax = new Vector2(0f, 1f);
            hueRt.pivot = new Vector2(0f, 1f);
            hueRt.anchoredPosition = new Vector2(244, -90);
            hueRt.sizeDelta = new Vector2(24, 110);
            _hueSurface = hueObj.GetComponent<PickerSurface>();
            _hueSurface.Mode = PickerMode.Hue;
            _hueSurface.Changed = p =>
            {
                _baseHue = 1f - p.y;
                UpdateSurfaces();
                UpdateGeneratedColors();
            };

            // Selected Base Color Preview Box
            var basePrevObj = new GameObject("BaseColorPreview", typeof(RectTransform), typeof(Image));
            basePrevObj.transform.SetParent(dialog.transform, false);
            var bpRt = (RectTransform)basePrevObj.transform;
            bpRt.anchorMin = new Vector2(0f, 1f);
            bpRt.anchorMax = new Vector2(0f, 1f);
            bpRt.pivot = new Vector2(0f, 1f);
            bpRt.anchoredPosition = new Vector2(280, -90);
            bpRt.sizeDelta = new Vector2(50, 48);
            _baseColorPreview = basePrevObj.GetComponent<Image>();
            _baseColorPreview.color = Color.green;

            _baseHexText = CreateText(dialog.transform, "BaseHex", "#26FA73", new Vector2(280, -144), new Vector2(60, 20), 12, Color.white, FontStyles.Bold, TextAlignmentOptions.Left);

            // Presets Swatches Grid
            CreateText(dialog.transform, "PresetsLabel", "Presets:", new Vector2(350, -68), new Vector2(100, 18), 12, Color.white, FontStyles.Bold);

            float pX = 350f;
            float pY = -90f;
            for (int i = 0; i < Presets.Length; i++)
            {
                string hex = Presets[i];
                ColorUtility.TryParseHtmlString("#" + hex, out var pColor);
                CreateColorButton(dialog.transform, $"Preset_{hex}", pColor, new Vector2(pX + (i % 5) * 42f, pY - (i / 5) * 32f), new Vector2(36f, 26f), () =>
                {
                    Color.RGBToHSV(pColor, out _baseHue, out _baseSaturation, out _baseValue);
                    UpdateSurfaces();
                    UpdateGeneratedColors();
                });
            }

            // Section 2: Auto-Generated Palette Cards
            CreateText(dialog.transform, "GenLabel", "2. Auto-Harmonized Hues for Heatmap States:", new Vector2(24, -212), new Vector2(400, 20), 13, Color.yellow, FontStyles.Bold);

            float cardStartX = 24f;
            float cardSpacing = 108f;
            float cardY = -236f;

            CreateStateCard(dialog.transform, "Card_Running", "Running", "Active line", new Vector2(cardStartX + 0 * cardSpacing, cardY), out _runningSwatch, out _runningHex);
            CreateStateCard(dialog.transform, "Card_Starved", "Starved", "Waiting input", new Vector2(cardStartX + 1 * cardSpacing, cardY), out _starvedSwatch, out _starvedHex);
            CreateStateCard(dialog.transform, "Card_Clogged", "Clogged", "Blocked pipe", new Vector2(cardStartX + 2 * cardSpacing, cardY), out _cloggedSwatch, out _cloggedHex);
            CreateStateCard(dialog.transform, "Card_Full", "Full Storage", "Containers", new Vector2(cardStartX + 3 * cardSpacing, cardY), out _fullStorageSwatch, out _fullStorageHex);
            CreateStateCard(dialog.transform, "Card_Idle", "Idle", "Dormant", new Vector2(cardStartX + 4 * cardSpacing, cardY), out _idleSwatch, out _idleHex);

            // Section 3: Bottom Action Buttons
            CreateButton(dialog.transform, "ApplyBtn", "Apply Palette", new Vector2(24, -394), new Vector2(170, 34), new Color(0.20f, 0.48f, 0.28f), Color.white, ApplyPalette);
            CreateButton(dialog.transform, "ResetBtn", "Reset Defaults", new Vector2(210, -394), new Vector2(160, 34), new Color(0.28f, 0.28f, 0.32f), Color.white, ResetDefaults);
            CreateButton(dialog.transform, "CancelBtn", "Cancel", new Vector2(386, -394), new Vector2(170, 34), new Color(0.25f, 0.20f, 0.20f), Color.white, Close);
        }

        void CreateStateCard(Transform parent, string name, string title, string sub, Vector2 pos, out Image swatch, out TMP_Text hexText)
        {
            var card = new GameObject(name, typeof(RectTransform), typeof(Image));
            card.transform.SetParent(parent, false);
            var rt = (RectTransform)card.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(100f, 146f);
            var bg = card.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.17f, 0.20f, 0.95f);

            // Swatch Box
            var sObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            sObj.transform.SetParent(card.transform, false);
            var sRt = (RectTransform)sObj.transform;
            sRt.anchorMin = new Vector2(0.5f, 1f);
            sRt.anchorMax = new Vector2(0.5f, 1f);
            sRt.pivot = new Vector2(0.5f, 1f);
            sRt.anchoredPosition = new Vector2(0, -10);
            sRt.sizeDelta = new Vector2(76, 52);
            swatch = sObj.GetComponent<Image>();
            swatch.color = Color.white;

            // State Title
            CreateText(card.transform, "Title", title, new Vector2(0, -68), new Vector2(100, 20), 13, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
            // State Subtitle
            CreateText(card.transform, "Sub", sub, new Vector2(0, -88), new Vector2(100, 16), 10, new Color(0.70f, 0.68f, 0.65f), FontStyles.Normal, TextAlignmentOptions.Center);
            // Hex Code
            hexText = CreateText(card.transform, "Hex", "#FFFFFF", new Vector2(0, -114), new Vector2(100, 18), 11, new Color(1f, 0.90f, 0.50f), FontStyles.Bold, TextAlignmentOptions.Center);
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
            tmp.fontSize = 14;
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
