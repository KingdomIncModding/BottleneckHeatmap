using System;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Source.Flow;
using Assets.Source.Items;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class BottleneckHeatmapPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;

            EnabledConfig = Config.Bind(
                "General", "Enabled", true,
                new ConfigDescription("Whether the Bottleneck Heatmap overlay is enabled.")
            );

            ToggleKeyConfig = Config.Bind(
                "Input", "ToggleKey", Key.H,
                new ConfigDescription("Keyboard key used to toggle the Bottleneck Heatmap.")
            );

            NodeOpacityConfig = Config.Bind(
                "Visuals", "NodeOpacity", 0.58f,
                new ConfigDescription("Opacity / transparency of the node overlay (0.1 to 1.0).", new AcceptableValueRange<float>(0.1f, 1.0f))
            );

            PipeOpacityConfig = Config.Bind(
                "Visuals", "PipeOpacity", 0.85f,
                new ConfigDescription("Opacity / transparency of the pipe overlay (0.2 to 1.0).", new AcceptableValueRange<float>(0.2f, 1.0f))
            );

            PipeWidthConfig = Config.Bind(
                "Visuals", "PipeWidthMultiplier", 1.30f,
                new ConfigDescription("Width multiplier for pipe overlay line glow (1.0 to 2.5).", new AcceptableValueRange<float>(1.0f, 2.5f))
            );

            ShowIdleConfig = Config.Bind(
                "Visuals", "ShowIdle", true,
                new ConfigDescription("Display neutral slate overlay on idle or unconfigured nodes and pipes.")
            );

            ShowToastConfig = Config.Bind(
                "UI", "ShowToggleToast", true,
                new ConfigDescription("Display an on-screen status banner notification when toggling heatmap.")
            );

            RunningColorConfig = Config.Bind(
                "Colors", "RunningColor", "#26FA73",
                new ConfigDescription("Hex color for active / running nodes and pipes.")
            );

            StarvedColorConfig = Config.Bind(
                "Colors", "StarvedColor", "#FFD92E",
                new ConfigDescription("Hex color for starved / waiting nodes and pipes.")
            );

            CloggedColorConfig = Config.Bind(
                "Colors", "CloggedColor", "#FF3842",
                new ConfigDescription("Hex color for clogged / blocked nodes and pipes.")
            );

            FullStorageColorConfig = Config.Bind(
                "Colors", "FullStorageColor", "#B042FF",
                new ConfigDescription("Hex color for full storage containers (Stockpiles).")
            );

            IdleColorConfig = Config.Bind(
                "Colors", "IdleColor", "#8C9EB8",
                new ConfigDescription("Hex color for idle / dormant nodes and pipes.")
            );

            EnableRunningStateConfig = Config.Bind(
                "States", "EnableRunningState", true,
                new ConfigDescription("Whether Running status is highlighted. When disabled, overlay is transparent.")
            );

            EnableStarvedStateConfig = Config.Bind(
                "States", "EnableStarvedState", true,
                new ConfigDescription("Whether Starved status is highlighted. When disabled, displays as Running color.")
            );

            EnableCloggedStateConfig = Config.Bind(
                "States", "EnableCloggedState", true,
                new ConfigDescription("Whether Clogged status is highlighted. When disabled, displays as Running color.")
            );

            EnableFullStorageStateConfig = Config.Bind(
                "States", "EnableFullStorageState", true,
                new ConfigDescription("Whether Full Storage status is highlighted. When disabled, displays as Running color.")
            );

            EnableIdleStateConfig = Config.Bind(
                "States", "EnableIdleState", true,
                new ConfigDescription("Whether Idle status is highlighted. When disabled, displays as Running color.")
            );

            UpdateMaterialColors();

            _harmony = new Harmony(Id);
            ApplyHarmonyPatches();

            FoundationIntegration.Register(this);

            Logger.LogInfo($"Bottleneck Heatmap v{Version} loaded. Toggle key: {ToggleKeyConfig.Value}.");
        }
    }
}
