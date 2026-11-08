using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal static class FoundationIntegration
    {
        static IModBarRegistration _barRegistration;
        static HeatmapBarView _activeView;
        static IModConfigButton _hotkeyButton;

        public static (string Label, string Hex)[] RunningVariations;
        public static (string Label, string Hex)[] StarvedVariations;
        public static (string Label, string Hex)[] CloggedVariations;
        public static (string Label, string Hex)[] FullStorageVariations;
        public static (string Label, string Hex)[] IdleVariations;

        static ModConfigStateColorRow _runningRow;
        static ModConfigStateColorRow _starvedRow;
        static ModConfigStateColorRow _cloggedRow;
        static ModConfigStateColorRow _fullStorageRow;
        static ModConfigStateColorRow _idleRow;

        public static (string Label, string Hex)[] GenerateVariations(Color baseColor, string stateName)
        {
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            string baseHex = "#" + ColorUtility.ToHtmlStringRGB(baseColor);

            // 1. Base / Selected
            string label0 = $"{stateName} Selected ({baseHex})";

            // 2. Warm Variation (-12 deg)
            float hWarm = (h - (12f / 360f) + 1f) % 1f;
            Color cWarm = Color.HSVToRGB(hWarm, Mathf.Clamp01(s * 0.96f), Mathf.Min(1f, v * 1.02f));
            string hexWarm = "#" + ColorUtility.ToHtmlStringRGB(cWarm);
            string label1 = $"Warm Variation ({hexWarm})";

            // 3. Cool Variation (+12 deg)
            float hCool = (h + (12f / 360f)) % 1f;
            Color cCool = Color.HSVToRGB(hCool, Mathf.Clamp01(s * 1.02f), v);
            string hexCool = "#" + ColorUtility.ToHtmlStringRGB(cCool);
            string label2 = $"Cool Variation ({hexCool})";

            // 4. Bright Variation (-24 deg or lighter)
            float hBright = (h - (24f / 360f) + 1f) % 1f;
            Color cBright = (s < 0.25f)
                ? Color.HSVToRGB(h, Mathf.Clamp01(s * 0.70f), Mathf.Min(1f, v * 1.25f))
                : Color.HSVToRGB(hBright, Mathf.Clamp01(s * 0.90f), 1f);
            string hexBright = "#" + ColorUtility.ToHtmlStringRGB(cBright);
            string label3 = $"Bright Variation ({hexBright})";

            // 5. Deep Variation (+24 deg or deeper)
            float hDeep = (h + (24f / 360f)) % 1f;
            Color cDeep = (s < 0.25f)
                ? Color.HSVToRGB(h, Mathf.Min(1f, s * 1.30f), Mathf.Clamp01(v * 0.70f))
                : Color.HSVToRGB(hDeep, 1f, Mathf.Clamp01(v * 0.90f));
            string hexDeep = "#" + ColorUtility.ToHtmlStringRGB(cDeep);
            string label4 = $"Deep Variation ({hexDeep})";

            return new (string Label, string Hex)[]
            {
                (label0, baseHex),
                (label1, hexWarm),
                (label2, hexCool),
                (label3, hexBright),
                (label4, hexDeep)
            };
        }

        public static int FindColorIndex((string Label, string Hex)[] options, string hex)
        {
            if (options == null || options.Length == 0 || string.IsNullOrEmpty(hex)) return 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (string.Equals(options[i].Hex, hex, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return 0;
        }

        public static string[] GetLabels((string Label, string Hex)[] options)
        {
            if (options == null) return new string[0];
            var labels = new string[options.Length];
            for (int i = 0; i < options.Length; i++) labels[i] = options[i].Label;
            return labels;
        }

        public static bool IsAvailable
        {
            get
            {
                try
                {
                    return AppDomain.CurrentDomain.GetAssemblies() != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static void Register(BottleneckHeatmapPlugin plugin)
        {
            try
            {
                RegisterConfigInternal(plugin);
                RegisterModBarInternal(plugin);
            }
            catch (Exception ex)
            {
                plugin.Log.LogWarning($"Foundation integration registration skipped: {ex.Message}");
            }
        }

        static void RegisterConfigInternal(BottleneckHeatmapPlugin plugin)
        {
            var section = FoundationApi.ModConfig.GetOrCreateSection(
                BottleneckHeatmapPlugin.Id,
                "Bottleneck Heatmap",
                "Real-time visual diagnostic overlay color-coding nodes and pipes by bottleneck status."
            );

            // Initialize dynamic variations based on current saved colors
            Color ParseColor(string hex, Color fallback) =>
                ColorUtility.TryParseHtmlString(hex, out var c) ? c : fallback;

            Color cRunning = ParseColor(plugin.RunningColorConfig.Value, new Color(0.15f, 0.98f, 0.45f));
            Color cStarved = ParseColor(plugin.StarvedColorConfig.Value, new Color(1.00f, 0.85f, 0.18f));
            Color cClogged = ParseColor(plugin.CloggedColorConfig.Value, new Color(1.00f, 0.22f, 0.26f));
            Color cFull = ParseColor(plugin.FullStorageColorConfig.Value, new Color(0.69f, 0.26f, 1.00f));
            Color cIdle = ParseColor(plugin.IdleColorConfig.Value, new Color(0.55f, 0.62f, 0.75f));

            RunningVariations = GenerateVariations(cRunning, "Running");
            StarvedVariations = GenerateVariations(cStarved, "Starved");
            CloggedVariations = GenerateVariations(cClogged, "Clogged");
            FullStorageVariations = GenerateVariations(cFull, "Full Storage");
            IdleVariations = GenerateVariations(cIdle, "Idle");

            // Section 1: General
            section.AddText("General", isHeader: true);

            section.AddToggle(
                "Enable Heatmap",
                plugin.IsHeatmapEnabled,
                val =>
                {
                    plugin.SetHeatmapEnabled(val, fromUser: true);
                },
                getter: () => plugin.IsHeatmapEnabled
            );

            section.AddToggle(
                "Show Idle Elements",
                plugin.ShowIdle,
                val =>
                {
                    plugin.ShowIdleConfig.Value = val;
                    plugin.Config.Save();
                    plugin.RefreshAllOverlays();
                },
                getter: () => plugin.ShowIdle
            );

            section.AddToggle(
                "Show Notification On Toggle",
                plugin.ShowToastConfig.Value,
                val =>
                {
                    plugin.ShowToastConfig.Value = val;
                    plugin.Config.Save();
                },
                getter: () => plugin.ShowToastConfig.Value
            );

            _hotkeyButton = section.AddButton(
                "Toggle Hotkey",
                () => plugin.StartListeningForHotkey(),
                buttonText: $"[ {plugin.ToggleKeyConfig.Value} ]"
            );

            // Section 2: Overlay & Visuals
            section.AddText("Overlay & Visuals", isHeader: true);

            section.AddSlider(
                "Node Overlay Opacity",
                10f,
                100f,
                Mathf.Round(plugin.NodeOpacity * 100f),
                val =>
                {
                    plugin.NodeOpacityConfig.Value = val / 100f;
                    plugin.Config.Save();
                    plugin.RefreshAllOverlays();
                },
                step: 5f,
                getter: () => Mathf.Round(plugin.NodeOpacity * 100f),
                formatValue: val => $"{Mathf.RoundToInt(val)}%"
            );

            section.AddSlider(
                "Pipe Overlay Opacity",
                20f,
                100f,
                Mathf.Round(plugin.PipeOpacity * 100f),
                val =>
                {
                    plugin.PipeOpacityConfig.Value = val / 100f;
                    plugin.Config.Save();
                    plugin.RefreshAllOverlays();
                },
                step: 5f,
                getter: () => Mathf.Round(plugin.PipeOpacity * 100f),
                formatValue: val => $"{Mathf.RoundToInt(val)}%"
            );

            section.AddSlider(
                "Pipe Glow Width",
                1.0f,
                2.5f,
                plugin.PipeWidthMultiplier,
                val =>
                {
                    plugin.PipeWidthConfig.Value = val;
                    plugin.Config.Save();
                    plugin.RefreshAllOverlays();
                },
                step: 0.1f,
                getter: () => plugin.PipeWidthMultiplier,
                formatValue: val => $"{val:0.0}x"
            );

            // Section 3: State Colors & Variations
            section.AddText("State Colors & Variations", isHeader: true);

            _runningRow = new ModConfigStateColorRow(
                BottleneckState.Running,
                "Running Color (Active)",
                RunningVariations,
                () => plugin.RunningColorConfig.Value,
                val =>
                {
                    plugin.RunningColorConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                }
            );
            section.AddElement(_runningRow);

            _starvedRow = new ModConfigStateColorRow(
                BottleneckState.Starved,
                "Starved Color (Waiting)",
                StarvedVariations,
                () => plugin.StarvedColorConfig.Value,
                val =>
                {
                    plugin.StarvedColorConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                }
            );
            section.AddElement(_starvedRow);

            _cloggedRow = new ModConfigStateColorRow(
                BottleneckState.Clogged,
                "Clogged Color (Blocked)",
                CloggedVariations,
                () => plugin.CloggedColorConfig.Value,
                val =>
                {
                    plugin.CloggedColorConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                }
            );
            section.AddElement(_cloggedRow);

            _fullStorageRow = new ModConfigStateColorRow(
                BottleneckState.FullStorage,
                "Full Storage Color (Containers)",
                FullStorageVariations,
                () => plugin.FullStorageColorConfig.Value,
                val =>
                {
                    plugin.FullStorageColorConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                }
            );
            section.AddElement(_fullStorageRow);

            _idleRow = new ModConfigStateColorRow(
                BottleneckState.Idle,
                "Idle Color (Dormant)",
                IdleVariations,
                () => plugin.IdleColorConfig.Value,
                val =>
                {
                    plugin.IdleColorConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                }
            );
            section.AddElement(_idleRow);
        }

        public static void UpdateDropdownOptions(BottleneckState state, (string Label, string Hex)[] newVariations)
        {
            try
            {
                var plugin = BottleneckHeatmapPlugin.Instance;
                string currentHex = state switch
                {
                    BottleneckState.Running => plugin?.RunningColorConfig?.Value,
                    BottleneckState.Starved => plugin?.StarvedColorConfig?.Value,
                    BottleneckState.Clogged => plugin?.CloggedColorConfig?.Value,
                    BottleneckState.FullStorage => plugin?.FullStorageColorConfig?.Value,
                    BottleneckState.Idle => plugin?.IdleColorConfig?.Value,
                    _ => null
                };

                ModConfigStateColorRow row = state switch
                {
                    BottleneckState.Running => _runningRow,
                    BottleneckState.Starved => _starvedRow,
                    BottleneckState.Clogged => _cloggedRow,
                    BottleneckState.FullStorage => _fullStorageRow,
                    BottleneckState.Idle => _idleRow,
                    _ => null
                };

                row?.UpdateVariations(newVariations, currentHex);
            }
            catch
            {
            }
        }

        internal static void SetActiveView(HeatmapBarView view)
        {
            _activeView = view;
        }

        internal static void ClearActiveView(HeatmapBarView view)
        {
            if (ReferenceEquals(_activeView, view))
            {
                _activeView = null;
            }
        }

        static void RegisterModBarInternal(BottleneckHeatmapPlugin plugin)
        {
            if (FoundationApi.ModBar == null) return;

            var element = new HeatmapBarElement(plugin);
            _barRegistration = FoundationApi.ModBar.Register(element);
        }

        public static void UpdateHotkeyButtonText(string text)
        {
            try
            {
                if (_hotkeyButton != null)
                {
                    _hotkeyButton.ButtonText = text;
                }
            }
            catch
            {
            }
        }

        public static void SyncModBarToggle(bool enabled)
        {
            try
            {
                _activeView?.Refresh();
                _barRegistration?.Refresh();
            }
            catch
            {
            }
        }

        public static void Release()
        {
            try
            {
                _barRegistration?.Dispose();
                _barRegistration = null;
                _activeView = null;
            }
            catch
            {
            }
        }
    }

    internal sealed class ModConfigStateColorRow : IModConfigElement
    {
        public string Id { get; }
        public bool IsVisible { get; set; } = true;
        public BottleneckState State { get; }
        public string Label { get; }
        public Func<string> HexGetter { get; }
        public Action<string> HexSetter { get; }

        (string Label, string Hex)[] _variations;
        Image _swatchImage;
        TMP_Dropdown _activeDropdown;

        public ModConfigStateColorRow(
            BottleneckState state,
            string label,
            (string Label, string Hex)[] initialVariations,
            Func<string> hexGetter,
            Action<string> hexSetter,
            string id = null)
        {
            State = state;
            Label = label;
            _variations = initialVariations;
            HexGetter = hexGetter;
            HexSetter = hexSetter;
            Id = id ?? Guid.NewGuid().ToString("N");
        }

        public GameObject CreateView(IModConfigContext context)
        {
            var row = context.Templates.CreateEmptyRow(context.Container, 48f);
            row.name = $"StateColorRow_{State}";

            string currentHex = HexGetter != null ? HexGetter() : "#FFFFFF";
            Color curColor = ColorUtility.TryParseHtmlString(currentHex, out var c) ? c : Color.white;

            // 1. Swatch Border
            var swatchBorderObj = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            swatchBorderObj.transform.SetParent(row.transform, false);
            var sbRt = (RectTransform)swatchBorderObj.transform;
            sbRt.anchorMin = new Vector2(0f, 0.5f);
            sbRt.anchorMax = new Vector2(0f, 0.5f);
            sbRt.pivot = new Vector2(0f, 0.5f);
            sbRt.anchoredPosition = new Vector2(6f, 0f);
            sbRt.sizeDelta = new Vector2(24f, 24f);
            var sbImg = swatchBorderObj.GetComponent<Image>();
            sbImg.color = new Color(0.36f, 0.32f, 0.24f, 1f);

            // 2. Swatch Image
            var swatchObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            swatchObj.transform.SetParent(swatchBorderObj.transform, false);
            var sRt = (RectTransform)swatchObj.transform;
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(2f, 2f);
            sRt.offsetMax = new Vector2(-2f, -2f);
            _swatchImage = swatchObj.GetComponent<Image>();
            _swatchImage.color = curColor;

            // 3. Row Label
            var labelObj = new GameObject("RowLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(row.transform, false);
            var lRt = (RectTransform)labelObj.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0.44f, 1f);
            lRt.offsetMin = new Vector2(36f, 0f);
            lRt.offsetMax = new Vector2(-4f, 0f);
            var tmp = labelObj.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = 15;
            tmp.fontSizeMin = 11;
            tmp.fontSizeMax = 16;
            tmp.enableAutoSizing = true;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            tmp.text = Label ?? "";

            // 4. "Pick Color" Button cloned from vanilla
            var dummyBtnRow = context.Templates.CreateButtonRow(context.Container, "", "Pick Color", () =>
            {
                ColorPalettePopup.Open(State);
            }, out var button, out var btnText);

            var btnObj = button.gameObject;
            btnObj.transform.SetParent(row.transform, false);
            var bRt = (RectTransform)btnObj.transform;
            bRt.anchorMin = new Vector2(0.45f, 0.08f);
            bRt.anchorMax = new Vector2(0.66f, 0.92f);
            bRt.offsetMin = Vector2.zero;
            bRt.offsetMax = Vector2.zero;
            if (btnText != null)
            {
                btnText.text = "Pick Color";
                btnText.fontSizeMin = 11;
                btnText.fontSizeMax = 15;
                btnText.enableAutoSizing = true;
            }
            UnityEngine.Object.Destroy(dummyBtnRow);

            // 5. Dropdown cloned from vanilla
            string[] labels = FoundationIntegration.GetLabels(_variations);
            int selectedIndex = FoundationIntegration.FindColorIndex(_variations, currentHex);

            var dummyDropRow = context.Templates.CreateDropdownRow(context.Container, "", labels, selectedIndex, idx =>
            {
                if (_variations != null && idx >= 0 && idx < _variations.Length)
                {
                    string chosenHex = _variations[idx].Hex;
                    HexSetter?.Invoke(chosenHex);
                    if (ColorUtility.TryParseHtmlString(chosenHex, out var col) && _swatchImage != null)
                    {
                        _swatchImage.color = col;
                    }
                }
            }, out _activeDropdown);

            var dropObj = _activeDropdown.gameObject;
            dropObj.transform.SetParent(row.transform, false);
            var dRt = (RectTransform)dropObj.transform;
            dRt.anchorMin = new Vector2(0.67f, 0.08f);
            dRt.anchorMax = new Vector2(0.98f, 0.92f);
            dRt.offsetMin = Vector2.zero;
            dRt.offsetMax = Vector2.zero;
            if (_activeDropdown.captionText != null)
            {
                _activeDropdown.captionText.fontSizeMin = 9;
                _activeDropdown.captionText.fontSizeMax = 13;
                _activeDropdown.captionText.alignment = TextAlignmentOptions.MidlineLeft;
            }
            UnityEngine.Object.Destroy(dummyDropRow);

            row.SetActive(IsVisible);
            return row;
        }

        public void UpdateVariations((string Label, string Hex)[] newVariations, string currentHex)
        {
            _variations = newVariations;
            if (ColorUtility.TryParseHtmlString(currentHex, out var col) && _swatchImage != null)
            {
                _swatchImage.color = col;
            }

            if (_activeDropdown != null && newVariations != null)
            {
                string[] labels = FoundationIntegration.GetLabels(newVariations);
                int idx = FoundationIntegration.FindColorIndex(newVariations, currentHex);
                _activeDropdown.ClearOptions();
                _activeDropdown.AddOptions(new List<string>(labels));
                _activeDropdown.SetValueWithoutNotify(idx);
                _activeDropdown.RefreshShownValue();
            }
        }

        public void Commit() { }
        public void Cancel() { }
    }

    internal sealed class HeatmapBarElement : ModBarElement
    {
        readonly BottleneckHeatmapPlugin _plugin;

        public HeatmapBarElement(BottleneckHeatmapPlugin plugin)
            : base(BottleneckHeatmapPlugin.Id, "heatmap_toggle", 160, 20, ModBarAlignment.Right)
        {
            _plugin = plugin;
        }

        public override IModBarView CreateView(IModBarContext context)
        {
            var view = new HeatmapBarView(_plugin, context);
            FoundationIntegration.SetActiveView(view);
            return view;
        }
    }

    internal sealed class HeatmapBarView : IModBarView
    {
        readonly BottleneckHeatmapPlugin _plugin;
        readonly Button _button;
        readonly TMP_Text _text;
        readonly Image _image;

        public HeatmapBarView(BottleneckHeatmapPlugin plugin, IModBarContext context)
        {
            _plugin = plugin;
            _button = context.CreateButton(GetLabel(), OnClick);
            var clickHandler = _button.gameObject.AddComponent<HeatmapBarClickHandler>();
            clickHandler.OnRightClick = () => ColorPalettePopup.Open();

            _image = _button.GetComponent<Image>();
            _text = _button.GetComponentInChildren<TMP_Text>(true);
            if (_text != null)
            {
                _text.enableAutoSizing = true;
                _text.fontSizeMin = 13;
                _text.fontSizeMax = 18;
                _text.overflowMode = TextOverflowModes.Overflow;
                _text.rectTransform.offsetMin = new Vector2(4, 2);
                _text.rectTransform.offsetMax = new Vector2(-4, -2);
            }
            Refresh();
        }

        string GetLabel() => _plugin.IsHeatmapEnabled ? "Heatmap: ON" : "Heatmap: OFF";

        void OnClick()
        {
            _plugin.SetHeatmapEnabled(!_plugin.IsHeatmapEnabled, fromUser: true);
        }

        public void Refresh()
        {
            if (_text != null)
            {
                _text.text = GetLabel();
                _text.color = _plugin.IsHeatmapEnabled
                    ? new Color(0.95f, 1f, 0.95f)
                    : new Color(0.72f, 0.72f, 0.75f);
            }
            if (_button != null && _image != null)
            {
                var colors = _button.colors;
                Color baseTint = _plugin.IsHeatmapEnabled
                    ? new Color(0.85f, 1f, 0.88f, 1f)
                    : new Color(0.75f, 0.75f, 0.78f, 0.9f);
                colors.normalColor = baseTint;
                colors.highlightedColor = _plugin.IsHeatmapEnabled
                    ? new Color(0.92f, 1f, 0.95f, 1f)
                    : new Color(0.85f, 0.85f, 0.88f, 1f);
                colors.selectedColor = baseTint;
                _button.colors = colors;
                _image.color = baseTint;
            }
        }

        public void Dispose()
        {
            FoundationIntegration.ClearActiveView(this);
        }
    }

    internal sealed class HeatmapBarClickHandler : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public Action OnRightClick;

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (eventData.button == UnityEngine.EventSystems.PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke();
            }
        }
    }
}
