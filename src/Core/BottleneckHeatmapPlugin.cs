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
    [BepInPlugin(Id, "Bottleneck Heatmap", Version)]
    [BepInDependency("urakka.kingdominc.foundation")]
    public sealed partial class BottleneckHeatmapPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.bottleneckheatmap";
        public const string Version = "1.2.6";

        public static BottleneckHeatmapPlugin Instance { get; private set; }
        public BepInEx.Logging.ManualLogSource Log => Logger;

        public ConfigEntry<bool> EnabledConfig;
        public ConfigEntry<Key> ToggleKeyConfig;
        public ConfigEntry<float> NodeOpacityConfig;
        public ConfigEntry<float> PipeOpacityConfig;
        public ConfigEntry<float> PipeWidthConfig;
        public ConfigEntry<bool> ShowIdleConfig;
        public ConfigEntry<bool> ShowToastConfig;
        public ConfigEntry<string> RunningColorConfig;
        public ConfigEntry<string> StarvedColorConfig;
        public ConfigEntry<string> CloggedColorConfig;
        public ConfigEntry<string> FullStorageColorConfig;
        public ConfigEntry<string> IdleColorConfig;
        public ConfigEntry<bool> EnableRunningStateConfig;
        public ConfigEntry<bool> EnableStarvedStateConfig;
        public ConfigEntry<bool> EnableCloggedStateConfig;
        public ConfigEntry<bool> EnableFullStorageStateConfig;
        public ConfigEntry<bool> EnableIdleStateConfig;

        public bool IsHeatmapEnabled => EnabledConfig?.Value ?? true;
        public float NodeOpacity => NodeOpacityConfig?.Value ?? 0.58f;
        public float PipeOpacity => PipeOpacityConfig?.Value ?? 0.85f;
        public float PipeWidthMultiplier => PipeWidthConfig?.Value ?? 1.30f;
        public bool ShowIdle => ShowIdleConfig?.Value ?? true;
        public bool EnableRunningState => EnableRunningStateConfig?.Value ?? true;
        public bool EnableStarvedState => EnableStarvedStateConfig?.Value ?? true;
        public bool EnableCloggedState => EnableCloggedStateConfig?.Value ?? true;
        public bool EnableFullStorageState => EnableFullStorageStateConfig?.Value ?? true;
        public bool EnableIdleState => EnableIdleStateConfig?.Value ?? true;

        Harmony _harmony;
        float _nextSweepTime;
        float _lastHotkeyToggleTime;
        bool _isListeningForHotkey;

        public bool IsListeningForHotkey => _isListeningForHotkey;


        public void ToggleHeatmapFromHotkey()
        {
            if (Time.unscaledTime - _lastHotkeyToggleTime < 0.25f) return;
            _lastHotkeyToggleTime = Time.unscaledTime;
            SetHeatmapEnabled(!IsHeatmapEnabled, fromUser: true);
        }
    }
}
