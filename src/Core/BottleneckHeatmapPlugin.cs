using KingdomFoundation.Api;
using System;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomBottleneckHeatmap
{
    [BepInPlugin(Id, "Bottleneck Heatmap", Version)]
    [BepInDependency("urakka.kingdominc.foundation")]
    public sealed class BottleneckHeatmapPlugin : BaseUnityPlugin
    {
        void Awake()
        {
            Instance = this;
            EnabledConfig = Config.Bind("General", "Enabled", true, new ConfigDescription("Whether the Bottleneck Heatmap overlay is enabled."));
            ToggleKeyConfig = Config.Bind("Input", "ToggleKey", Key.H, new ConfigDescription("Keyboard key used to toggle the Bottleneck Heatmap."));
            NodeOpacityConfig = Config.Bind("Visuals", "NodeOpacity", 0.58f, new ConfigDescription("Opacity / transparency of the node overlay (0.1 to 1.0).", new AcceptableValueRange<float>(0.1f, 1.0f)));
            PipeOpacityConfig = Config.Bind("Visuals", "PipeOpacity", 0.85f, new ConfigDescription("Opacity / transparency of the pipe overlay (0.2 to 1.0).", new AcceptableValueRange<float>(0.2f, 1.0f)));
            PipeWidthConfig = Config.Bind("Visuals", "PipeWidthMultiplier", 1.30f, new ConfigDescription("Width multiplier for pipe overlay line glow (1.0 to 2.5).", new AcceptableValueRange<float>(1.0f, 2.5f)));
            ShowIdleConfig = Config.Bind("Visuals", "ShowIdle", true, new ConfigDescription("Display neutral slate overlay on idle or unconfigured nodes and pipes."));
            ShowToastConfig = Config.Bind("UI", "ShowToggleToast", true, new ConfigDescription("Display an on-screen status banner notification when toggling heatmap."));
            RunningColorConfig = Config.Bind("Colors", "RunningColor", "#26FA73", new ConfigDescription("Hex color for active / running nodes and pipes."));
            StarvedColorConfig = Config.Bind("Colors", "StarvedColor", "#FFD92E", new ConfigDescription("Hex color for starved / waiting nodes and pipes."));
            CloggedColorConfig = Config.Bind("Colors", "CloggedColor", "#FF3842", new ConfigDescription("Hex color for clogged / blocked nodes and pipes."));
            FullStorageColorConfig = Config.Bind("Colors", "FullStorageColor", "#B042FF", new ConfigDescription("Hex color for full storage containers (Stockpiles)."));
            IdleColorConfig = Config.Bind("Colors", "IdleColor", "#8C9EB8", new ConfigDescription("Hex color for idle / dormant nodes and pipes."));
            EnableRunningStateConfig = Config.Bind("States", "EnableRunningState", true, new ConfigDescription("Whether Running status is highlighted. When disabled, overlay is transparent."));
            EnableStarvedStateConfig = Config.Bind("States", "EnableStarvedState", true, new ConfigDescription("Whether Starved status is highlighted. When disabled, displays as Running color."));
            EnableCloggedStateConfig = Config.Bind("States", "EnableCloggedState", true, new ConfigDescription("Whether Clogged status is highlighted. When disabled, displays as Running color."));
            EnableFullStorageStateConfig = Config.Bind("States", "EnableFullStorageState", true, new ConfigDescription("Whether Full Storage status is highlighted. When disabled, displays as Running color."));
            EnableIdleStateConfig = Config.Bind("States", "EnableIdleState", true, new ConfigDescription("Whether Idle status is highlighted. When disabled, displays as Running color."));
            UpdateMaterialColors();
            ApplyHarmonyPatches();
            FoundationIntegration.Register(this);
            Logger.LogInfo($"Bottleneck Heatmap v{Version} loaded. Toggle key: {ToggleKeyConfig.Value}.");
        }

        public const string Id = "urakka.kingdominc.bottleneckheatmap";
        public const string Version = "1.2.7";
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

        readonly System.Collections.Generic.List<IDisposable> _hooks = new System.Collections.Generic.List<IDisposable>();
        float _nextSweepTime;
        float _lastHotkeyToggleTime;
        bool _isListeningForHotkey;
        public bool IsListeningForHotkey => _isListeningForHotkey;

        public void ToggleHeatmapFromHotkey()
        {
            if (Time.unscaledTime - _lastHotkeyToggleTime < 0.25f)
                return;
            _lastHotkeyToggleTime = Time.unscaledTime;
            SetHeatmapEnabled(!IsHeatmapEnabled, fromUser: true);
        }

        void ApplyHarmonyPatches()
        {
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BuildingBound, after: call => global::KingdomBottleneckHeatmap.Patches.NodeSetElementPostfix(call.View)));
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.PipeBound, after: call => global::KingdomBottleneckHeatmap.Patches.PipeSetFlowPipePostfix(call.View)));
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.PipeUpdateGeometry, after: call => global::KingdomBottleneckHeatmap.Patches.PipeUpdatePositionsPostfix(call.View)));
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.PipeItemTransferred, after: call => global::KingdomBottleneckHeatmap.Patches.PipeItemTransferredPostfix(call.View)));
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.PipeItemRejected, after: call => global::KingdomBottleneckHeatmap.Patches.PipeItemRejectedPostfix(call.View)));
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.WorkspaceShown, after: call => global::KingdomBottleneckHeatmap.Patches.ShowFlowPostfix()));
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.InputInitialized, after: call => global::KingdomBottleneckHeatmap.Patches.PlayerInputAwakePostfix()));
            _hooks.Add(FoundationApi.GameHooks.Subscribe(Id, GameHookPoints.BindingRowBound, after: call => global::KingdomBottleneckHeatmap.Patches.KeyBindingRowSetBindingPostfix(call.View, call.Argument<InputAction>(0))));
        }

        void OnDestroy()
        {
            foreach (var hook in _hooks)
                hook.Dispose();
            _hooks.Clear();
            FoundationIntegration.Release();
            HeatmapMaterials.Release();
            foreach (var node in FindObjectsByType<HeatmapNodeOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Destroy(node);
            foreach (var pipe in FindObjectsByType<HeatmapPipeOverlay>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Destroy(pipe);
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
                    string starvHex = (EnableStarvedState ? StarvedColorConfig?.Value : runHex) ?? "#FFD92E";
                    string clogHex = (EnableCloggedState ? CloggedColorConfig?.Value : runHex) ?? "#FF3842";
                    string fullHex = (EnableFullStorageState ? FullStorageColorConfig?.Value : runHex) ?? "#B042FF";
                    var msgObj = FoundationApi.UiAccess.ShowStatusText(enabled ? "Bottleneck Heatmap: Active" : "Bottleneck Heatmap: Disabled", "Icons_17", false);
                    if (enabled && msgObj != null && msgObj != null)
                    {
                        msgObj.text = $"Bottleneck Heatmap: Active (<color={runHex}>Running</color>  <color={starvHex}>Starved</color>  <color={clogHex}>Clogged</color>  <color={fullHex}>Full</color>)";
                    }
                }
                catch
                {
                }
            }

            RefreshAllOverlays();
        }

        void Start()
        {
            SweepAndAttachOverlays();
        }

        public void StartListeningForHotkey()
        {
            _isListeningForHotkey = true;
            FoundationIntegration.UpdateHotkeyButtonText("[ Press Any Key... ]");
        }

        public void StopListeningForHotkey(bool cancelled = false)
        {
            _isListeningForHotkey = false;
            FoundationIntegration.UpdateHotkeyButtonText($"[ {ToggleKeyConfig.Value} ]");
        }

        public void SweepAndAttachOverlays()
        {
            try
            {
                if (!FoundationApi.WorkspaceAccess.IsReady)
                    return;
                foreach (var item in FoundationApi.WorkspaceAccess.AllObjects)
                {
                    if (item.Kind == WorkspaceObjectKind.Building && item.World && item.World.gameObject.activeInHierarchy)
                        HeatmapNodeOverlay.Ensure(item.World);
                }

                foreach (var workspace in FoundationApi.Kingdom.Workspaces)
                    foreach (var pipe in workspace.Pipes)
                        if (pipe.World && pipe.World.gameObject.activeInHierarchy)
                            HeatmapPipeOverlay.Ensure(pipe.World);
            }
            catch (Exception ex)
            {
                Log.LogDebug($"SweepAndAttachOverlays exception: {ex.Message}");
            }
        }

        public void SyncVanillaActionBinding()
        {
            try
            {
                if (global::KingdomBottleneckHeatmap.Patches.HeatmapAction != null)
                {
                    string path = $"<Keyboard>/{ToggleKeyConfig.Value.ToString().ToLowerInvariant()}";
                    global::KingdomBottleneckHeatmap.Patches.HeatmapAction.ApplyBindingOverride(0, path);
                }
            }
            catch
            {
            }
        }

        void Update()
        {
            if (_isListeningForHotkey)
            {
                if (Keyboard.current != null)
                {
                    if (Keyboard.current.escapeKey.wasPressedThisFrame)
                    {
                        StopListeningForHotkey(true);
                    }
                    else
                    {
                        foreach (var key in Keyboard.current.allKeys)
                        {
                            if (key.wasPressedThisFrame && key.keyCode != Key.None && key.keyCode != Key.Escape)
                            {
                                ToggleKeyConfig.Value = key.keyCode;
                                Config.Save();
                                SyncVanillaActionBinding();
                                StopListeningForHotkey(false);
                                break;
                            }
                        }
                    }
                }

                return;
            }

            // If native InputAction is active, it handles hotkey via performed callback
            if (global::KingdomBottleneckHeatmap.Patches.HeatmapAction == null || !global::KingdomBottleneckHeatmap.Patches.HeatmapAction.enabled)
            {
                if (Keyboard.current != null && ToggleKeyConfig != null)
                {
                    var keyControl = Keyboard.current[ToggleKeyConfig.Value];
                    if (keyControl != null && keyControl.wasPressedThisFrame)
                    {
                        ToggleHeatmapFromHotkey();
                    }
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
            HeatmapMaterials.EnableRunning = EnableRunningState;
            HeatmapMaterials.EnableStarved = EnableStarvedState;
            HeatmapMaterials.EnableClogged = EnableCloggedState;
            HeatmapMaterials.EnableFullStorage = EnableFullStorageState;
            HeatmapMaterials.EnableIdle = EnableIdleState;
            HeatmapMaterials.SetColors(running, starved, clogged, fullStorage, idle);
        }
    }
}
