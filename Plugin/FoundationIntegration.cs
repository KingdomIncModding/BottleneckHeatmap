using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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

        static IModConfigDropdown _runningDropdown;
        static IModConfigDropdown _starvedDropdown;
        static IModConfigDropdown _cloggedDropdown;
        static IModConfigDropdown _fullStorageDropdown;
        static IModConfigDropdown _idleDropdown;

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

        static int FindColorIndex((string Label, string Hex)[] options, string hex)
        {
            if (options == null || options.Length == 0 || string.IsNullOrEmpty(hex)) return 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (string.Equals(options[i].Hex, hex, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return 0;
        }

        static string[] GetLabels((string Label, string Hex)[] options)
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

            // Enabled Toggle
            section.AddToggle(
                "Enable Heatmap",
                plugin.IsHeatmapEnabled,
                val =>
                {
                    plugin.SetHeatmapEnabled(val, fromUser: true);
                },
                getter: () => plugin.IsHeatmapEnabled
            );

            // Node Opacity Slider (uses Foundation native ModConfigSlider)
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

            // Pipe Opacity Slider (uses Foundation native ModConfigSlider)
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

            // Pipe Glow Width Slider (uses Foundation native ModConfigSlider)
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

            // Show Idle
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

            // Show Toast Notification on Toggle
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

            // Toggle Hotkey Button (Vanilla-style interactive rebinding)
            _hotkeyButton = section.AddButton(
                "Toggle Hotkey",
                () => plugin.StartListeningForHotkey(),
                buttonText: $"[ {plugin.ToggleKeyConfig.Value} ]"
            );

            // Color Palette Generator Button
            section.AddButton(
                "Color Palette Wheel",
                () => ColorPalettePopup.Open(),
                buttonText: "Open Color Wheel"
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

            section.AddText("State Colors & Surrounding Hue Variations", isHeader: true);

            // 1. Running State
            section.AddButton(
                "Running Color (Active)",
                () => ColorPalettePopup.Open(BottleneckState.Running),
                buttonText: "Pick Color"
            );
            _runningDropdown = section.AddDropdown(
                "Running Variations",
                GetLabels(RunningVariations),
                FindColorIndex(RunningVariations, plugin.RunningColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < RunningVariations.Length)
                    {
                        plugin.RunningColorConfig.Value = RunningVariations[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(RunningVariations, plugin.RunningColorConfig.Value)
            );

            // 2. Starved State
            section.AddButton(
                "Starved Color (Waiting)",
                () => ColorPalettePopup.Open(BottleneckState.Starved),
                buttonText: "Pick Color"
            );
            _starvedDropdown = section.AddDropdown(
                "Starved Variations",
                GetLabels(StarvedVariations),
                FindColorIndex(StarvedVariations, plugin.StarvedColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < StarvedVariations.Length)
                    {
                        plugin.StarvedColorConfig.Value = StarvedVariations[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(StarvedVariations, plugin.StarvedColorConfig.Value)
            );

            // 3. Clogged State
            section.AddButton(
                "Clogged Color (Blocked)",
                () => ColorPalettePopup.Open(BottleneckState.Clogged),
                buttonText: "Pick Color"
            );
            _cloggedDropdown = section.AddDropdown(
                "Clogged Variations",
                GetLabels(CloggedVariations),
                FindColorIndex(CloggedVariations, plugin.CloggedColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < CloggedVariations.Length)
                    {
                        plugin.CloggedColorConfig.Value = CloggedVariations[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(CloggedVariations, plugin.CloggedColorConfig.Value)
            );

            // 4. Full Storage State
            section.AddButton(
                "Full Storage Color (Containers)",
                () => ColorPalettePopup.Open(BottleneckState.FullStorage),
                buttonText: "Pick Color"
            );
            _fullStorageDropdown = section.AddDropdown(
                "Full Storage Variations",
                GetLabels(FullStorageVariations),
                FindColorIndex(FullStorageVariations, plugin.FullStorageColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < FullStorageVariations.Length)
                    {
                        plugin.FullStorageColorConfig.Value = FullStorageVariations[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(FullStorageVariations, plugin.FullStorageColorConfig.Value)
            );

            // 5. Idle State
            section.AddButton(
                "Idle Color (Dormant)",
                () => ColorPalettePopup.Open(BottleneckState.Idle),
                buttonText: "Pick Color"
            );
            _idleDropdown = section.AddDropdown(
                "Idle Variations",
                GetLabels(IdleVariations),
                FindColorIndex(IdleVariations, plugin.IdleColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < IdleVariations.Length)
                    {
                        plugin.IdleColorConfig.Value = IdleVariations[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(IdleVariations, plugin.IdleColorConfig.Value)
            );
        }

        public static void UpdateDropdownOptions(BottleneckState state, (string Label, string Hex)[] newVariations)
        {
            try
            {
                IModConfigDropdown dropdown = state switch
                {
                    BottleneckState.Running => _runningDropdown,
                    BottleneckState.Starved => _starvedDropdown,
                    BottleneckState.Clogged => _cloggedDropdown,
                    BottleneckState.FullStorage => _fullStorageDropdown,
                    BottleneckState.Idle => _idleDropdown,
                    _ => null
                };

                if (dropdown == null || newVariations == null || newVariations.Length == 0) return;

                string[] labels = GetLabels(newVariations);
                dropdown.Options = labels;

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

                int selectedIndex = FindColorIndex(newVariations, currentHex);
                dropdown.SelectedIndex = selectedIndex;

                // If view is instantiated in Foundation ModConfig, update TMP_Dropdown live
                if (dropdown is ModConfigDropdown mcd)
                {
                    var field = typeof(ModConfigDropdown).GetField("activeDropdown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var tmp = field?.GetValue(mcd) as TMP_Dropdown;
                    if (tmp != null)
                    {
                        tmp.ClearOptions();
                        tmp.AddOptions(new List<string>(labels));
                        tmp.SetValueWithoutNotify(selectedIndex);
                        tmp.RefreshShownValue();
                    }
                }
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
