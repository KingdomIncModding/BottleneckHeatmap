using System;
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

        static readonly (string Label, Key Key)[] HotkeyOptions = new (string, Key)[]
        {
            ("H (Default)", Key.H),
            ("B", Key.B),
            ("C", Key.C),
            ("F", Key.F),
            ("G", Key.G),
            ("J", Key.J),
            ("K", Key.K),
            ("M", Key.M),
            ("N", Key.N),
            ("P", Key.P),
            ("T", Key.T),
            ("V", Key.V),
            ("X", Key.X),
            ("Z", Key.Z),
            ("Tab", Key.Tab),
            ("F1", Key.F1),
            ("F2", Key.F2),
            ("F3", Key.F3),
            ("F4", Key.F4),
            ("F5", Key.F5),
            ("F6", Key.F6),
            ("F7", Key.F7),
            ("F8", Key.F8),
            ("F9", Key.F9),
            ("F10", Key.F10)
        };

        static readonly (string Label, string Hex)[] RunningColors = new (string, string)[]
        {
            ("Emerald Green (#26FA73 - Default)", "#26FA73"),
            ("Neon Mint (#00FFB2)", "#00FFB2"),
            ("Cyan Glow (#00D9FF)", "#00D9FF"),
            ("Lime Green (#A6FF00)", "#A6FF00")
        };

        static readonly (string Label, string Hex)[] StarvedColors = new (string, string)[]
        {
            ("Bright Amber (#FFD92E - Default)", "#FFD92E"),
            ("Warm Orange (#FFA31A)", "#FFA31A"),
            ("Deep Peach (#FF8533)", "#FF8533"),
            ("Sun Yellow (#FFF033)", "#FFF033")
        };

        static readonly (string Label, string Hex)[] CloggedColors = new (string, string)[]
        {
            ("Alert Crimson (#FF3842 - Default)", "#FF3842"),
            ("Ruby Red (#FF1A4B)", "#FF1A4B"),
            ("Hot Coral (#FF5252)", "#FF5252"),
            ("Electric Rose (#FF007F)", "#FF007F")
        };

        static readonly (string Label, string Hex)[] FullStorageColors = new (string, string)[]
        {
            ("Royal Purple (#B042FF - Default)", "#B042FF"),
            ("Electric Violet (#8C28FF)", "#8C28FF"),
            ("Vibrant Magenta (#DF2BFF)", "#DF2BFF"),
            ("Deep Amethyst (#681EC9)", "#681EC9"),
            ("Soft Lavender (#C97DFF)", "#C97DFF")
        };

        static readonly (string Label, string Hex)[] IdleColors = new (string, string)[]
        {
            ("Slate Blue (#8C9EB8 - Default)", "#8C9EB8"),
            ("Dim Charcoal (#505A69)", "#505A69"),
            ("Soft Gray (#A0AAB8)", "#A0AAB8"),
            ("Steel Navy (#41536B)", "#41536B")
        };

        static int FindKeyIndex(Key key)
        {
            for (int i = 0; i < HotkeyOptions.Length; i++)
            {
                if (HotkeyOptions[i].Key == key) return i;
            }
            return 0;
        }

        static int FindColorIndex((string Label, string Hex)[] options, string hex)
        {
            if (string.IsNullOrEmpty(hex)) return 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (string.Equals(options[i].Hex, hex, StringComparison.OrdinalIgnoreCase)) return i;
            }
            return 0;
        }

        static string[] GetLabels((string Label, string Hex)[] options)
        {
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

            // Node Opacity Slider
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

            // Pipe Opacity Slider
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

            // Pipe Glow Width Slider
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
                "Color Palette Generator",
                () => ColorPalettePopup.Open(),
                buttonText: "Open Color Wheel"
            );

            // Running Color Dropdown
            section.AddDropdown(
                "Running Color (Active)",
                GetLabels(RunningColors),
                FindColorIndex(RunningColors, plugin.RunningColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < RunningColors.Length)
                    {
                        plugin.RunningColorConfig.Value = RunningColors[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(RunningColors, plugin.RunningColorConfig.Value)
            );

            // Starved Color Dropdown
            section.AddDropdown(
                "Starved Color (Waiting)",
                GetLabels(StarvedColors),
                FindColorIndex(StarvedColors, plugin.StarvedColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < StarvedColors.Length)
                    {
                        plugin.StarvedColorConfig.Value = StarvedColors[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(StarvedColors, plugin.StarvedColorConfig.Value)
            );

            // Clogged Color Dropdown
            section.AddDropdown(
                "Clogged Color (Blocked)",
                GetLabels(CloggedColors),
                FindColorIndex(CloggedColors, plugin.CloggedColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < CloggedColors.Length)
                    {
                        plugin.CloggedColorConfig.Value = CloggedColors[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(CloggedColors, plugin.CloggedColorConfig.Value)
            );

            // Full Storage Color Dropdown
            section.AddDropdown(
                "Full Storage Color (Containers)",
                GetLabels(FullStorageColors),
                FindColorIndex(FullStorageColors, plugin.FullStorageColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < FullStorageColors.Length)
                    {
                        plugin.FullStorageColorConfig.Value = FullStorageColors[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(FullStorageColors, plugin.FullStorageColorConfig.Value)
            );

            // Idle Color Dropdown
            section.AddDropdown(
                "Idle Color (Dormant)",
                GetLabels(IdleColors),
                FindColorIndex(IdleColors, plugin.IdleColorConfig.Value),
                idx =>
                {
                    if (idx >= 0 && idx < IdleColors.Length)
                    {
                        plugin.IdleColorConfig.Value = IdleColors[idx].Hex;
                        plugin.Config.Save();
                        plugin.UpdateMaterialColors();
                        plugin.RefreshAllOverlays();
                    }
                },
                getter: () => FindColorIndex(IdleColors, plugin.IdleColorConfig.Value)
            );
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

