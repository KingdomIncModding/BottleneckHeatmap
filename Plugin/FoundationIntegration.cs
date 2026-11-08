using System;
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

            // Node Opacity Dropdown
            string[] nodeOpacities = { "35% (Subtle)", "58% (Vibrant - Default)", "75% (Bright)", "90% (High Contrast)" };
            int currentNodeIdx = plugin.NodeOpacity >= 0.82f ? 3 : (plugin.NodeOpacity >= 0.68f ? 2 : (plugin.NodeOpacity >= 0.45f ? 1 : 0));
            section.AddDropdown(
                "Node Overlay Opacity",
                nodeOpacities,
                currentNodeIdx,
                idx =>
                {
                    float op = idx switch { 0 => 0.35f, 1 => 0.58f, 2 => 0.75f, _ => 0.90f };
                    plugin.NodeOpacityConfig.Value = op;
                    plugin.Config.Save();
                    plugin.RefreshAllOverlays();
                },
                getter: () => plugin.NodeOpacity >= 0.82f ? 3 : (plugin.NodeOpacity >= 0.68f ? 2 : (plugin.NodeOpacity >= 0.45f ? 1 : 0))
            );

            // Pipe Opacity Dropdown
            string[] pipeOpacities = { "50% (Subtle)", "70% (Clear)", "85% (Glowing - Default)", "100% (Solid)" };
            int currentPipeIdx = plugin.PipeOpacity >= 0.95f ? 3 : (plugin.PipeOpacity >= 0.78f ? 2 : (plugin.PipeOpacity >= 0.60f ? 1 : 0));
            section.AddDropdown(
                "Pipe Overlay Opacity",
                pipeOpacities,
                currentPipeIdx,
                idx =>
                {
                    float op = idx switch { 0 => 0.50f, 1 => 0.70f, 2 => 0.85f, _ => 1.00f };
                    plugin.PipeOpacityConfig.Value = op;
                    plugin.Config.Save();
                    plugin.RefreshAllOverlays();
                },
                getter: () => plugin.PipeOpacity >= 0.95f ? 3 : (plugin.PipeOpacity >= 0.78f ? 2 : (plugin.PipeOpacity >= 0.60f ? 1 : 0))
            );

            // Pipe Width Scale Dropdown
            string[] widthOptions = { "1.0x (Exact Pipe Width)", "1.3x (Subtle Halo)", "1.6x (Prominent)", "2.0x (Bold Glow)" };
            int currentWidthIdx = plugin.PipeWidthMultiplier >= 1.9f ? 3 : (plugin.PipeWidthMultiplier >= 1.5f ? 2 : (plugin.PipeWidthMultiplier >= 1.2f ? 1 : 0));
            section.AddDropdown(
                "Pipe Glow Width",
                widthOptions,
                currentWidthIdx,
                idx =>
                {
                    float w = idx switch { 0 => 1.0f, 1 => 1.3f, 2 => 1.6f, _ => 2.0f };
                    plugin.PipeWidthConfig.Value = w;
                    plugin.Config.Save();
                    plugin.RefreshAllOverlays();
                },
                getter: () => plugin.PipeWidthMultiplier >= 1.9f ? 3 : (plugin.PipeWidthMultiplier >= 1.5f ? 2 : (plugin.PipeWidthMultiplier >= 1.2f ? 1 : 0))
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
}

