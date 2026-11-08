using System;
using System.Collections.Generic;
using Assets.Behaviour.UI;
using Assets.Source.Util;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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

            // Section 3: State Colors & Variations (Matches User Mockup)
            section.AddText("State Colors & Variations", isHeader: true);

            _runningRow = new ModConfigStateColorRow(
                BottleneckState.Running,
                "Running Color (Active)",
                RunningVariations,
                () => plugin.EnableRunningState,
                val =>
                {
                    plugin.EnableRunningStateConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                },
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
                () => plugin.EnableStarvedState,
                val =>
                {
                    plugin.EnableStarvedStateConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                },
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
                () => plugin.EnableCloggedState,
                val =>
                {
                    plugin.EnableCloggedStateConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                },
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
                () => plugin.EnableFullStorageState,
                val =>
                {
                    plugin.EnableFullStorageStateConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                },
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
                () => plugin.EnableIdleState,
                val =>
                {
                    plugin.EnableIdleStateConfig.Value = val;
                    plugin.Config.Save();
                    plugin.UpdateMaterialColors();
                    plugin.RefreshAllOverlays();
                },
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

    /// <summary>
    /// Custom row element displaying:
    /// [Toggle Checkbox] [Active Swatch] Label    [Pick Color Button]  |  [Var1][Var2][Var3][Var4][Var5]
    /// Completely matches user mockup image on a single unified line!
    /// </summary>
    internal sealed class ModConfigStateColorRow : IModConfigElement
    {
        public string Id { get; }
        public bool IsVisible { get; set; } = true;
        public BottleneckState State { get; }
        public string Label { get; }
        public Func<bool> EnabledGetter { get; }
        public Action<bool> EnabledSetter { get; }
        public Func<string> HexGetter { get; }
        public Action<string> HexSetter { get; }

        (string Label, string Hex)[] _variations;
        Image _leftSwatchImage;
        readonly Image[] _swatchBorders = new Image[5];
        readonly Image[] _swatchFills = new Image[5];

        public ModConfigStateColorRow(
            BottleneckState state,
            string label,
            (string Label, string Hex)[] initialVariations,
            Func<bool> enabledGetter,
            Action<bool> enabledSetter,
            Func<string> hexGetter,
            Action<string> hexSetter,
            string id = null)
        {
            State = state;
            Label = label;
            _variations = initialVariations;
            EnabledGetter = enabledGetter;
            EnabledSetter = enabledSetter;
            HexGetter = hexGetter;
            HexSetter = hexSetter;
            Id = id ?? Guid.NewGuid().ToString("N");
        }

        public GameObject CreateView(IModConfigContext context)
        {
            VanillaTheme.Ensure();

            var row = context.Templates.CreateEmptyRow(context.Container, 46f);
            row.name = $"StateColorRow_{State}";

            bool isEnabled = EnabledGetter != null ? EnabledGetter() : true;
            string currentHex = HexGetter != null ? HexGetter() : "#FFFFFF";
            Color curColor = ColorUtility.TryParseHtmlString(currentHex, out var c) ? c : Color.white;

            // 1. Toggle Checkbox cloned from vanilla template
            var dummyToggleRow = context.Templates.CreateToggleRow(context.Container, "", isEnabled, null, out var toggle);
            var toggleObj = toggle.gameObject;
            toggleObj.transform.SetParent(row.transform, false);
            var tRt = (RectTransform)toggleObj.transform;
            tRt.anchorMin = new Vector2(0f, 0.5f);
            tRt.anchorMax = new Vector2(0f, 0.5f);
            tRt.pivot = new Vector2(0f, 0.5f);
            tRt.anchoredPosition = new Vector2(6f, 0f);
            tRt.sizeDelta = new Vector2(24f, 24f);

            toggle.onValueChanged = new Toggle.ToggleEvent();
            toggle.SetIsOnWithoutNotify(isEnabled);
            toggle.onValueChanged.AddListener(val =>
            {
                UISounds.CraftStep();
                EnabledSetter?.Invoke(val);
            });
            UnityEngine.Object.Destroy(dummyToggleRow);

            // 2. Left Swatch Border (22x22)
            var swatchBorderObj = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            swatchBorderObj.transform.SetParent(row.transform, false);
            var sbRt = (RectTransform)swatchBorderObj.transform;
            sbRt.anchorMin = new Vector2(0f, 0.5f);
            sbRt.anchorMax = new Vector2(0f, 0.5f);
            sbRt.pivot = new Vector2(0f, 0.5f);
            sbRt.anchoredPosition = new Vector2(36f, 0f);
            sbRt.sizeDelta = new Vector2(22f, 22f);
            var sbImg = swatchBorderObj.GetComponent<Image>();
            sbImg.color = new Color(0.36f, 0.32f, 0.24f, 1f);

            // Left Swatch Image
            var swatchObj = new GameObject("Swatch", typeof(RectTransform), typeof(Image));
            swatchObj.transform.SetParent(swatchBorderObj.transform, false);
            var sRt = (RectTransform)swatchObj.transform;
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.offsetMin = new Vector2(2f, 2f);
            sRt.offsetMax = new Vector2(-2f, -2f);
            _leftSwatchImage = swatchObj.GetComponent<Image>();
            _leftSwatchImage.color = curColor;

            // 3. Row Label
            var labelObj = new GameObject("RowLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObj.transform.SetParent(row.transform, false);
            var lRt = (RectTransform)labelObj.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0.40f, 1f);
            lRt.offsetMin = new Vector2(64f, 0f);
            lRt.offsetMax = new Vector2(-4f, 0f);
            var tmp = labelObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null)
            {
                tmp.font = VanillaTheme.Font;
                tmp.fontSharedMaterial = VanillaTheme.FontMaterial;
            }
            tmp.fontSize = 14f;
            tmp.fontSizeMin = 10f;
            tmp.fontSizeMax = 15f;
            tmp.enableAutoSizing = true;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;
            tmp.text = Label ?? "";

            // 3. "Pick Color" Button cloned from vanilla
            var dummyBtnRow = context.Templates.CreateButtonRow(context.Container, "", "Pick Color", null, out var button, out var btnText);

            var btnObj = button.gameObject;
            btnObj.transform.SetParent(row.transform, false);
            var bRt = (RectTransform)btnObj.transform;
            bRt.anchorMin = new Vector2(0.40f, 0.10f);
            bRt.anchorMax = new Vector2(0.57f, 0.90f);
            bRt.offsetMin = Vector2.zero;
            bRt.offsetMax = Vector2.zero;
            if (btnText != null)
            {
                btnText.text = "Pick Color";
                btnText.fontSizeMin = 10f;
                btnText.fontSizeMax = 14f;
                btnText.enableAutoSizing = true;
            }

            // Explicitly rebind onClick to guarantee it opens ColorPalettePopup
            var targetState = State;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() =>
            {
                UISounds.TurnPage();
                ColorPalettePopup.Open(targetState);
            });

            UnityEngine.Object.Destroy(dummyBtnRow);

            // 4. Subtle Vertical Divider "|"
            var divObj = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            divObj.transform.SetParent(row.transform, false);
            var divRt = (RectTransform)divObj.transform;
            divRt.anchorMin = new Vector2(0.585f, 0.14f);
            divRt.anchorMax = new Vector2(0.585f, 0.86f);
            divRt.sizeDelta = new Vector2(1.5f, 0f);
            var divImg = divObj.GetComponent<Image>();
            divImg.color = new Color(0.42f, 0.36f, 0.28f, 0.85f);

            // 5. 5 Variation Swatches Container with HorizontalLayoutGroup
            var swatchesContainer = new GameObject("SwatchesContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            swatchesContainer.transform.SetParent(row.transform, false);
            var scRt = (RectTransform)swatchesContainer.transform;
            scRt.anchorMin = new Vector2(0.60f, 0.12f);
            scRt.anchorMax = new Vector2(0.99f, 0.88f);
            scRt.offsetMin = Vector2.zero;
            scRt.offsetMax = Vector2.zero;

            var hlg = swatchesContainer.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            for (int i = 0; i < 5; i++)
            {
                int varIdx = i;
                var sBtnObj = new GameObject($"VarSwatch_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                sBtnObj.transform.SetParent(swatchesContainer.transform, false);

                var borderImg = sBtnObj.GetComponent<Image>();
                if (VanillaTheme.ButtonSprite != null)
                {
                    borderImg.sprite = VanillaTheme.ButtonSprite;
                    borderImg.type = Image.Type.Sliced;
                }
                borderImg.color = new Color(0.35f, 0.30f, 0.24f, 0.9f);
                _swatchBorders[i] = borderImg;

                var fillObj = new GameObject("Fill", typeof(RectTransform), typeof(Image));
                fillObj.transform.SetParent(sBtnObj.transform, false);
                var fRt = (RectTransform)fillObj.transform;
                fRt.anchorMin = Vector2.zero;
                fRt.anchorMax = Vector2.one;
                fRt.offsetMin = new Vector2(2f, 2f);
                fRt.offsetMax = new Vector2(-2f, -2f);
                var fillImg = fillObj.GetComponent<Image>();
                fillImg.raycastTarget = false;
                _swatchFills[i] = fillImg;

                if (_variations != null && i < _variations.Length)
                {
                    ColorUtility.TryParseHtmlString(_variations[i].Hex, out var varCol);
                    fillImg.color = varCol;
                }

                var btn = sBtnObj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    UISounds.TurnPage();
                    SelectVariation(varIdx);
                });
            }

            HighlightSelected(currentHex);

            row.SetActive(IsVisible);
            return row;
        }

        void SelectVariation(int index)
        {
            if (_variations == null || index < 0 || index >= _variations.Length) return;
            string hex = _variations[index].Hex;
            HexSetter?.Invoke(hex);
            if (ColorUtility.TryParseHtmlString(hex, out var col) && _leftSwatchImage != null)
            {
                _leftSwatchImage.color = col;
            }
            HighlightSelected(hex);
        }

        void HighlightSelected(string currentHex)
        {
            for (int i = 0; i < 5; i++)
            {
                if (_swatchBorders[i] == null) continue;
                bool isSelected = _variations != null && i < _variations.Length &&
                    string.Equals(_variations[i].Hex, currentHex, StringComparison.OrdinalIgnoreCase);

                if (isSelected)
                {
                    _swatchBorders[i].color = new Color(1f, 0.85f, 0.18f, 1f); // Bright gold border!
                }
                else
                {
                    _swatchBorders[i].color = new Color(0.35f, 0.30f, 0.24f, 0.9f); // Dark bronze border
                }
            }
        }

        public void UpdateVariations((string Label, string Hex)[] newVariations, string currentHex)
        {
            _variations = newVariations;
            if (ColorUtility.TryParseHtmlString(currentHex, out var col) && _leftSwatchImage != null)
            {
                _leftSwatchImage.color = col;
            }

            if (newVariations != null)
            {
                for (int i = 0; i < 5 && i < newVariations.Length; i++)
                {
                    if (_swatchFills[i] != null && ColorUtility.TryParseHtmlString(newVariations[i].Hex, out var vCol))
                    {
                        _swatchFills[i].color = vCol;
                    }
                }
            }

            HighlightSelected(currentHex);
        }

        public void Commit() { }
        public void Cancel() { }
    }

    /// <summary>
    /// Rich interactive hover legend tooltip displayed when hovering over the Heatmap button on the Mod Bar.
    /// </summary>
    internal sealed class HeatmapLegendTooltip : MonoBehaviour
    {
        static HeatmapLegendTooltip _instance;

        Image _swatchRunning;
        Image _swatchStarved;
        Image _swatchClogged;
        Image _swatchFullStorage;
        Image _swatchIdle;

        public static void Show(RectTransform anchor)
        {
            if (!anchor) return;
            EnsureInstance(anchor);
            if (!_instance) return;

            _instance.UpdateColors();
            _instance.PositionAt(anchor);
            _instance.gameObject.SetActive(true);
            _instance.transform.SetAsLastSibling();
        }

        public static void Hide()
        {
            if (_instance)
            {
                _instance.gameObject.SetActive(false);
            }
        }

        static void EnsureInstance(RectTransform anchor)
        {
            if (_instance) return;
            var canvas = anchor.GetComponentInParent<Canvas>();
            if (!canvas) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (!canvas) return;

            var go = new GameObject("HeatmapLegendTooltip", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            _instance = go.AddComponent<HeatmapLegendTooltip>();
            _instance.BuildUI();
        }

        void BuildUI()
        {
            VanillaTheme.Ensure();

            var rt = (RectTransform)transform;
            rt.sizeDelta = new Vector2(330f, 218f);

            var cg = gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;

            var img = gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            if (VanillaTheme.WindowSprite != null)
            {
                img.sprite = VanillaTheme.WindowSprite;
                img.type = Image.Type.Sliced;
                img.color = Color.white;
            }
            else
            {
                img.color = new Color(0.12f, 0.10f, 0.14f, 0.98f);
            }

            // Title
            CreateText(transform, "Title", "Bottleneck Heatmap Legend", new Vector2(0f, -14f), new Vector2(330f, 20f), 14f, new Color(1f, 0.85f, 0.18f), FontStyles.Bold, TextAlignmentOptions.Center);

            // Separator 1
            CreateSeparator(transform, -36f);

            // 5 Status Rows
            float startY = -46f;
            float rowHeight = 26f;

            _swatchRunning = CreateLegendRow(transform, "Running", "Running", "Processing & transferring items", startY + 0 * -rowHeight);
            _swatchStarved = CreateLegendRow(transform, "Starved", "Starved", "Waiting for input reagents", startY + 1 * -rowHeight);
            _swatchClogged = CreateLegendRow(transform, "Clogged", "Clogged", "Output blocked / rejected", startY + 2 * -rowHeight);
            _swatchFullStorage = CreateLegendRow(transform, "FullStorage", "Full Storage", "Stockpiles & Armories at cap", startY + 3 * -rowHeight);
            _swatchIdle = CreateLegendRow(transform, "Idle", "Idle", "Dormant / unconfigured node", startY + 4 * -rowHeight);

            // Separator 2
            CreateSeparator(transform, -180f);

            // Footer tip
            CreateText(transform, "Tip", "Left-Click: Toggle  *  Right-Click: Colors", new Vector2(0f, -190f), new Vector2(330f, 18f), 11f, new Color(0.78f, 0.74f, 0.68f), FontStyles.Normal, TextAlignmentOptions.Center);

            gameObject.SetActive(false);
        }

        Image CreateLegendRow(Transform parent, string name, string label, string desc, float y)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rRt = (RectTransform)row.transform;
            rRt.anchorMin = new Vector2(0f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot = new Vector2(0f, 1f);
            rRt.anchoredPosition = new Vector2(16f, y);
            rRt.sizeDelta = new Vector2(-32f, 24f);

            // Swatch Border
            var sBorder = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            sBorder.transform.SetParent(row.transform, false);
            var sbRt = (RectTransform)sBorder.transform;
            sbRt.anchorMin = new Vector2(0f, 0.5f);
            sbRt.anchorMax = new Vector2(0f, 0.5f);
            sbRt.pivot = new Vector2(0f, 0.5f);
            sbRt.anchoredPosition = Vector2.zero;
            sbRt.sizeDelta = new Vector2(18f, 18f);
            sBorder.GetComponent<Image>().color = new Color(0.35f, 0.30f, 0.24f, 1f);

            // Swatch Fill
            var sFill = new GameObject("SwatchFill", typeof(RectTransform), typeof(Image));
            sFill.transform.SetParent(sBorder.transform, false);
            var sfRt = (RectTransform)sFill.transform;
            sfRt.anchorMin = Vector2.zero;
            sfRt.anchorMax = Vector2.one;
            sfRt.offsetMin = new Vector2(2f, 2f);
            sfRt.offsetMax = new Vector2(-2f, -2f);
            var swatchImg = sFill.GetComponent<Image>();

            // Label
            var lObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lObj.transform.SetParent(row.transform, false);
            var lRt = (RectTransform)lObj.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0f, 1f);
            lRt.pivot = new Vector2(0f, 0.5f);
            lRt.anchoredPosition = new Vector2(24f, 0f);
            lRt.sizeDelta = new Vector2(85f, 24f);
            var lText = lObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null) { lText.font = VanillaTheme.Font; lText.fontSharedMaterial = VanillaTheme.FontMaterial; }
            lText.text = label;
            lText.fontSize = 12f;
            lText.fontStyle = FontStyles.Bold;
            lText.color = Color.white;
            lText.alignment = TextAlignmentOptions.MidlineLeft;

            // Description
            var dObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            dObj.transform.SetParent(row.transform, false);
            var dRt = (RectTransform)dObj.transform;
            dRt.anchorMin = new Vector2(0f, 0f);
            dRt.anchorMax = new Vector2(1f, 1f);
            dRt.pivot = new Vector2(0f, 0.5f);
            dRt.offsetMin = new Vector2(112f, 0f);
            dRt.offsetMax = Vector2.zero;
            var dText = dObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null) { dText.font = VanillaTheme.Font; dText.fontSharedMaterial = VanillaTheme.FontMaterial; }
            dText.text = desc;
            dText.fontSize = 10f;
            dText.color = new Color(0.80f, 0.77f, 0.72f, 0.95f);
            dText.alignment = TextAlignmentOptions.MidlineLeft;

            return swatchImg;
        }

        void CreateSeparator(Transform parent, float y)
        {
            var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(parent, false);
            var srt = (RectTransform)sep.transform;
            srt.anchorMin = new Vector2(0f, 1f);
            srt.anchorMax = new Vector2(1f, 1f);
            srt.anchoredPosition = new Vector2(0f, y);
            srt.sizeDelta = new Vector2(-28f, 1.5f);
            sep.GetComponent<Image>().color = new Color(0.38f, 0.32f, 0.24f, 0.85f);
        }

        TMP_Text CreateText(Transform parent, string name, string text, Vector2 pos, Vector2 size, float fontSize, Color color, FontStyles style, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
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
            return tmp;
        }

        void UpdateColors()
        {
            if (_swatchRunning) _swatchRunning.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Running);
            if (_swatchStarved) _swatchStarved.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Starved);
            if (_swatchClogged) _swatchClogged.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Clogged);
            if (_swatchFullStorage) _swatchFullStorage.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.FullStorage);
            if (_swatchIdle) _swatchIdle.color = HeatmapMaterials.GetEffectiveStateColor(BottleneckState.Idle);
        }

        void PositionAt(RectTransform anchor)
        {
            var canvas = GetComponentInParent<Canvas>();
            if (!canvas) return;

            Vector3[] corners = new Vector3[4];
            anchor.GetWorldCorners(corners);
            Vector3 bottomCenter = (corners[0] + corners[3]) * 0.5f;
            Vector3 topCenter = (corners[1] + corners[2]) * 0.5f;

            bool inUpperHalf = bottomCenter.y > Screen.height * 0.5f;

            var rt = (RectTransform)transform;
            rt.pivot = inUpperHalf ? new Vector2(0.5f, 1f) : new Vector2(0.5f, 0f);
            rt.position = inUpperHalf ? bottomCenter + new Vector3(0, -6f, 0) : topCenter + new Vector3(0, 6f, 0);

            var canvasRt = (RectTransform)canvas.transform;
            float halfWidth = rt.rect.width * 0.5f;
            Vector3 local = rt.localPosition;
            float minX = -canvasRt.rect.width * 0.5f + halfWidth + 10f;
            float maxX = canvasRt.rect.width * 0.5f - halfWidth - 10f;
            local.x = Mathf.Clamp(local.x, minX, maxX);
            rt.localPosition = local;
        }
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
            clickHandler.OnHoverEnter = () => HeatmapLegendTooltip.Show(_button.transform as RectTransform);
            clickHandler.OnHoverExit = () => HeatmapLegendTooltip.Hide();

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
            HeatmapLegendTooltip.Hide();
            FoundationIntegration.ClearActiveView(this);
        }
    }

    internal sealed class HeatmapBarClickHandler : MonoBehaviour,
        IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Action OnRightClick;
        public Action OnHoverEnter;
        public Action OnHoverExit;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick?.Invoke();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverExit?.Invoke();
        }

        void OnDisable()
        {
            OnHoverExit?.Invoke();
        }
    }
}
