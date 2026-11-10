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
    internal static partial class FoundationIntegration
    {
        static void RegisterConfigInternal(BottleneckHeatmapPlugin plugin)
        {
            var section = _context.Settings(
                "Bottleneck Heatmap",
                "Real-time visual diagnostic overlay color-coding nodes and pipes by bottleneck status."
            );

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
    }
}
