using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal static class FoundationIntegration
    {
        internal static void ClearActiveView(HeatmapBarView view)
        {
            if (ReferenceEquals(_activeView, view))
            {
                _activeView = null;
            }
        }

        static FoundationModContext _context;
        static IModBarRegistration _barRegistration;
        static HeatmapBarView _activeView;
        static IModConfigButton _hotkeyButton;
        static ModConfigStateColorRow _runningRow;
        static ModConfigStateColorRow _starvedRow;
        static ModConfigStateColorRow _cloggedRow;
        static ModConfigStateColorRow _fullStorageRow;
        static ModConfigStateColorRow _idleRow;
        public static (string Label, string Hex)[] RunningVariations;
        public static int FindColorIndex((string Label, string Hex)[] options, string hex)
        {
            if (options == null || options.Length == 0 || string.IsNullOrEmpty(hex))
                return 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (string.Equals(options[i].Hex, hex, StringComparison.OrdinalIgnoreCase))
                    return i;
            }

            return 0;
        }

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
            Color cBright = (s < 0.25f) ? Color.HSVToRGB(h, Mathf.Clamp01(s * 0.70f), Mathf.Min(1f, v * 1.25f)) : Color.HSVToRGB(hBright, Mathf.Clamp01(s * 0.90f), 1f);
            string hexBright = "#" + ColorUtility.ToHtmlStringRGB(cBright);
            string label3 = $"Bright Variation ({hexBright})";
            // 5. Deep Variation (+24 deg or deeper)
            float hDeep = (h + (24f / 360f)) % 1f;
            Color cDeep = (s < 0.25f) ? Color.HSVToRGB(h, Mathf.Min(1f, s * 1.30f), Mathf.Clamp01(v * 0.70f)) : Color.HSVToRGB(hDeep, 1f, Mathf.Clamp01(v * 0.90f));
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

        public static string[] GetLabels((string Label, string Hex)[] options)
        {
            if (options == null)
                return new string[0];
            var labels = new string[options.Length];
            for (int i = 0; i < options.Length; i++)
                labels[i] = options[i].Label;
            return labels;
        }

        static Color ParseColor(string hex, Color fallback) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : fallback;
        public static void Register(BottleneckHeatmapPlugin plugin)
        {
            try
            {
                _context = FoundationApi.ForMod(BottleneckHeatmapPlugin.Id);
                RegisterConfigInternal(plugin);
                RegisterModBarInternal(plugin);
            }
            catch
            {
                _context?.Dispose();
                _context = null;
                throw;
            }
        }

        static void RegisterConfigInternal(BottleneckHeatmapPlugin plugin)
        {
            var section = _context.Settings("Bottleneck Heatmap", "Real-time visual diagnostic overlay color-coding nodes and pipes by bottleneck status.");
            // Initialize dynamic variations based on current saved colors
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
            section.AddToggle("Enable Heatmap", plugin.IsHeatmapEnabled, val =>
            {
                plugin.SetHeatmapEnabled(val, fromUser: true);
            }, getter: () => plugin.IsHeatmapEnabled);
            section.AddToggle("Show Idle Elements", plugin.ShowIdle, val =>
            {
                plugin.ShowIdleConfig.Value = val;
                plugin.Config.Save();
                plugin.RefreshAllOverlays();
            }, getter: () => plugin.ShowIdle);
            section.AddToggle("Show Notification On Toggle", plugin.ShowToastConfig.Value, val =>
            {
                plugin.ShowToastConfig.Value = val;
                plugin.Config.Save();
            }, getter: () => plugin.ShowToastConfig.Value);
            _hotkeyButton = section.AddButton("Toggle Hotkey", () => plugin.StartListeningForHotkey(), buttonText: $"[ {plugin.ToggleKeyConfig.Value} ]");
            // Section 2: Overlay & Visuals
            section.AddText("Overlay & Visuals", isHeader: true);
            section.AddSlider("Node Overlay Opacity", 10f, 100f, Mathf.Round(plugin.NodeOpacity * 100f), val =>
            {
                plugin.NodeOpacityConfig.Value = val / 100f;
                plugin.Config.Save();
                plugin.RefreshAllOverlays();
            }, step: 5f, getter: () => Mathf.Round(plugin.NodeOpacity * 100f), formatValue: val => $"{Mathf.RoundToInt(val)}%");
            section.AddSlider("Pipe Overlay Opacity", 20f, 100f, Mathf.Round(plugin.PipeOpacity * 100f), val =>
            {
                plugin.PipeOpacityConfig.Value = val / 100f;
                plugin.Config.Save();
                plugin.RefreshAllOverlays();
            }, step: 5f, getter: () => Mathf.Round(plugin.PipeOpacity * 100f), formatValue: val => $"{Mathf.RoundToInt(val)}%");
            section.AddSlider("Pipe Glow Width", 1.0f, 2.5f, plugin.PipeWidthMultiplier, val =>
            {
                plugin.PipeWidthConfig.Value = val;
                plugin.Config.Save();
                plugin.RefreshAllOverlays();
            }, step: 0.1f, getter: () => plugin.PipeWidthMultiplier, formatValue: val => $"{val:0.0}x");
            // Section 3: State Colors & Variations (Matches User Mockup)
            section.AddText("State Colors & Variations", isHeader: true);
            _runningRow = new ModConfigStateColorRow(BottleneckState.Running, "Running Color (Active)", RunningVariations, () => plugin.EnableRunningState, val =>
            {
                plugin.EnableRunningStateConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            }, () => plugin.RunningColorConfig.Value, val =>
            {
                plugin.RunningColorConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            });
            section.AddElement(_runningRow);
            _starvedRow = new ModConfigStateColorRow(BottleneckState.Starved, "Starved Color (Waiting)", StarvedVariations, () => plugin.EnableStarvedState, val =>
            {
                plugin.EnableStarvedStateConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            }, () => plugin.StarvedColorConfig.Value, val =>
            {
                plugin.StarvedColorConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            });
            section.AddElement(_starvedRow);
            _cloggedRow = new ModConfigStateColorRow(BottleneckState.Clogged, "Clogged Color (Blocked)", CloggedVariations, () => plugin.EnableCloggedState, val =>
            {
                plugin.EnableCloggedStateConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            }, () => plugin.CloggedColorConfig.Value, val =>
            {
                plugin.CloggedColorConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            });
            section.AddElement(_cloggedRow);
            _fullStorageRow = new ModConfigStateColorRow(BottleneckState.FullStorage, "Full Storage Color (Containers)", FullStorageVariations, () => plugin.EnableFullStorageState, val =>
            {
                plugin.EnableFullStorageStateConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            }, () => plugin.FullStorageColorConfig.Value, val =>
            {
                plugin.FullStorageColorConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            });
            section.AddElement(_fullStorageRow);
            _idleRow = new ModConfigStateColorRow(BottleneckState.Idle, "Idle Color (Dormant)", IdleVariations, () => plugin.EnableIdleState, val =>
            {
                plugin.EnableIdleStateConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            }, () => plugin.IdleColorConfig.Value, val =>
            {
                plugin.IdleColorConfig.Value = val;
                plugin.Config.Save();
                plugin.UpdateMaterialColors();
                plugin.RefreshAllOverlays();
            });
            section.AddElement(_idleRow);
        }

        static void RegisterModBarInternal(BottleneckHeatmapPlugin plugin)
        {
            if (FoundationApi.ModBar == null)
                return;
            var element = new HeatmapBarElement(plugin);
            _barRegistration = _context.Register(element);
        }

        public static void Release()
        {
            try
            {
                _context?.Dispose();
                _context = null;
                _barRegistration = null;
                _activeView = null;
            }
            catch
            {
            }
        }

        internal static void SetActiveView(HeatmapBarView view)
        {
            _activeView = view;
        }

        public static (string Label, string Hex)[] CloggedVariations;
        public static (string Label, string Hex)[] FullStorageVariations;
        public static (string Label, string Hex)[] IdleVariations;
        public static (string Label, string Hex)[] StarvedVariations;
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
    }
}
