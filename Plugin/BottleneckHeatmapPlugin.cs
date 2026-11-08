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
    [BepInDependency("urakka.kingdominc.foundation", BepInDependency.DependencyFlags.SoftDependency)]
    public sealed class BottleneckHeatmapPlugin : BaseUnityPlugin
    {
        public const string Id = "urakka.kingdominc.bottleneckheatmap";
        public const string Version = "1.1.2";

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

        public bool IsHeatmapEnabled => EnabledConfig?.Value ?? true;
        public float NodeOpacity => NodeOpacityConfig?.Value ?? 0.58f;
        public float PipeOpacity => PipeOpacityConfig?.Value ?? 0.85f;
        public float PipeWidthMultiplier => PipeWidthConfig?.Value ?? 1.30f;
        public bool ShowIdle => ShowIdleConfig?.Value ?? true;

        Harmony _harmony;
        float _nextSweepTime;

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

            UpdateMaterialColors();

            _harmony = new Harmony(Id);
            ApplyHarmonyPatches();

            FoundationIntegration.Register(this);

            Logger.LogInfo($"Bottleneck Heatmap v{Version} loaded. Toggle key: {ToggleKeyConfig.Value}.");
        }

        void Start()
        {
            SweepAndAttachOverlays();
        }

        void Update()
        {
            if (Keyboard.current != null && ToggleKeyConfig != null)
            {
                var keyControl = Keyboard.current[ToggleKeyConfig.Value];
                if (keyControl != null && keyControl.wasPressedThisFrame)
                {
                    SetHeatmapEnabled(!IsHeatmapEnabled, fromUser: true);
                }
            }

            // Periodic sweep for any nodes or pipes created outside hooked paths
            if (Time.time >= _nextSweepTime)
            {
                _nextSweepTime = Time.time + 3.0f;
                SweepAndAttachOverlays();
            }
        }

        public void UpdateMaterialColors()
        {
            Color running = ColorUtility.TryParseHtmlString(RunningColorConfig?.Value, out var c1) ? c1 : new Color(0.15f, 0.98f, 0.45f, 1f);
            Color starved = ColorUtility.TryParseHtmlString(StarvedColorConfig?.Value, out var c2) ? c2 : new Color(1.00f, 0.85f, 0.18f, 1f);
            Color clogged = ColorUtility.TryParseHtmlString(CloggedColorConfig?.Value, out var c3) ? c3 : new Color(1.00f, 0.22f, 0.26f, 1f);
            Color fullStorage = ColorUtility.TryParseHtmlString(FullStorageColorConfig?.Value, out var c4) ? c4 : new Color(0.69f, 0.26f, 1.00f, 1f);
            Color idle = ColorUtility.TryParseHtmlString(IdleColorConfig?.Value, out var c5) ? c5 : new Color(0.55f, 0.62f, 0.75f, 1f);

            HeatmapMaterials.SetColors(running, starved, clogged, fullStorage, idle);
        }

        public void SetHeatmapEnabled(bool enabled, bool fromUser = false)
        {
            if (EnabledConfig.Value != enabled)
            {
                EnabledConfig.Value = enabled;
                Config.Save();
            }

            FoundationIntegration.SyncModBarToggle(enabled);

            if (fromUser && ShowToastConfig.Value)
            {
                try
                {
                    string runHex = RunningColorConfig?.Value ?? "#26FA73";
                    string starvHex = StarvedColorConfig?.Value ?? "#FFD92E";
                    string clogHex = CloggedColorConfig?.Value ?? "#FF3842";
                    string fullHex = FullStorageColorConfig?.Value ?? "#B042FF";

                    var msgObj = UIStatusMessage.Show(
                        enabled ? "Bottleneck Heatmap: Active" : "Bottleneck Heatmap: Disabled",
                        "Icons_17",
                        false
                    );

                    if (enabled && msgObj != null && msgObj.StatusText != null)
                    {
                        msgObj.StatusText.text = $"Bottleneck Heatmap: Active (<color={runHex}>Running</color>  <color={starvHex}>Starved</color>  <color={clogHex}>Clogged</color>  <color={fullHex}>Full</color>)";
                    }
                }
                catch
                {
                }
            }

            RefreshAllOverlays();
        }

        public void SweepAndAttachOverlays()
        {
            try
            {
                var elements = FindObjectsByType<WorldFlowElement>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < elements.Length; i++)
                {
                    HeatmapNodeOverlay.Ensure(elements[i]);
                }

                var pipes = FindObjectsByType<WorldFlowPipe>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
                for (int i = 0; i < pipes.Length; i++)
                {
                    HeatmapPipeOverlay.Ensure(pipes[i]);
                }
            }
            catch (Exception ex)
            {
                Log.LogDebug($"SweepAndAttachOverlays exception: {ex.Message}");
            }
        }

        public void RefreshAllOverlays()
        {
            try
            {
                var nodeOverlays = FindObjectsByType<HeatmapNodeOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < nodeOverlays.Length; i++)
                {
                    nodeOverlays[i].RefreshVisuals(true);
                }

                var pipeOverlays = FindObjectsByType<HeatmapPipeOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                for (int i = 0; i < pipeOverlays.Length; i++)
                {
                    pipeOverlays[i].RefreshVisuals(true);
                }
            }
            catch (Exception ex)
            {
                Log.LogDebug($"RefreshAllOverlays exception: {ex.Message}");
            }
        }

        void ApplyHarmonyPatches()
        {
            // Hook node initialization
            _harmony.Patch(
                AccessTools.Method(typeof(WorldFlowElement), nameof(WorldFlowElement.SetElement)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.NodeSetElementPostfix))
            );

            // Hook pipe initialization
            _harmony.Patch(
                AccessTools.Method(typeof(WorldFlowPipe), nameof(WorldFlowPipe.SetFlowPipe)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.PipeSetFlowPipePostfix))
            );

            // Hook pipe line position updates
            _harmony.Patch(
                AccessTools.Method(typeof(WorldFlowPipe), nameof(WorldFlowPipe.UpdateLinePositions)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.PipeUpdatePositionsPostfix))
            );

            // Hook pipe item transferred (tracks healthy throughput)
            _harmony.Patch(
                AccessTools.Method(typeof(WorldFlowPipe), nameof(WorldFlowPipe.ItemTransferred)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.PipeItemTransferredPostfix))
            );

            // Hook pipe item rejected (tracks blocked / full downstream)
            _harmony.Patch(
                AccessTools.Method(typeof(WorldFlowPipe), nameof(WorldFlowPipe.ItemRejected)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.PipeItemRejectedPostfix))
            );

            // Hook workshop tab switches
            _harmony.Patch(
                AccessTools.Method(typeof(WorldFlowManager), nameof(WorldFlowManager.ShowFlow)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.ShowFlowPostfix))
            );
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            FoundationIntegration.Release();
            HeatmapMaterials.Release();

            foreach (var node in FindObjectsByType<HeatmapNodeOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Destroy(node);

            foreach (var pipe in FindObjectsByType<HeatmapPipeOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Destroy(pipe);
        }

        static class Patches
        {
            public static void NodeSetElementPostfix(WorldFlowElement __instance)
            {
                if (__instance) HeatmapNodeOverlay.Ensure(__instance);
            }

            public static void PipeSetFlowPipePostfix(WorldFlowPipe __instance)
            {
                if (__instance) HeatmapPipeOverlay.Ensure(__instance);
            }

            public static void PipeUpdatePositionsPostfix(WorldFlowPipe __instance)
            {
                if (!__instance) return;
                var overlay = __instance.GetComponent<HeatmapPipeOverlay>();
                if (overlay) overlay.RefreshGeometry();
            }

            public static void PipeItemTransferredPostfix(WorldFlowPipe __instance)
            {
                if (!__instance) return;
                var overlay = __instance.GetComponent<HeatmapPipeOverlay>();
                if (overlay) overlay.NotifyItemTransferred();
            }

            public static void PipeItemRejectedPostfix(WorldFlowPipe __instance)
            {
                if (!__instance) return;
                var overlay = __instance.GetComponent<HeatmapPipeOverlay>();
                if (overlay) overlay.NotifyItemRejected();
            }

            public static void ShowFlowPostfix()
            {
                Instance?.RefreshAllOverlays();
            }
        }
    }
}

