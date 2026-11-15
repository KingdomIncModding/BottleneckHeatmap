using KingdomFoundation.Api;
using System;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomBottleneckHeatmap
{
    internal static class Patches
    {
        public static InputAction HeatmapAction;
        private static Action<InputAction.CallbackContext> performedHandler;

        public static void CleanupInput()
        {
            if (HeatmapAction == null)
                return;
            if (performedHandler != null)
            {
                HeatmapAction.performed -= performedHandler;
                performedHandler = null;
            }
            FoundationApi.Bindings.RemoveRebindable(HeatmapAction);
            HeatmapAction.Disable();
            HeatmapAction.Dispose();
            HeatmapAction = null;
        }
        public static void NodeSetElementPostfix(Component __instance)
        {
            if (__instance)
                HeatmapNodeOverlay.Ensure(__instance);
        }

        public static void KeyBindingRowSetBindingPostfix(Component __instance, InputAction binding)
        {
            if (binding != null && binding.name == "BottleneckHeatmapToggle")
            {
                var label = FoundationApi.Bindings.Label(__instance);
                if (label != null)
                {
                    label.text = "Bottleneck Heatmap";
                }
            }
        }

        public static void PipeItemRejectedPostfix(Component __instance)
        {
            if (!__instance)
                return;
            var overlay = __instance.GetComponent<HeatmapPipeOverlay>();
            if (overlay)
                overlay.NotifyItemRejected();
        }

        public static void PipeItemTransferredPostfix(Component __instance)
        {
            if (!__instance)
                return;
            var overlay = __instance.GetComponent<HeatmapPipeOverlay>();
            if (overlay)
                overlay.NotifyItemTransferred();
        }

        public static void PipeSetFlowPipePostfix(Component __instance)
        {
            if (__instance)
                HeatmapPipeOverlay.Ensure(__instance);
        }

        public static void PipeUpdatePositionsPostfix(Component __instance)
        {
            if (!__instance)
                return;
            var overlay = __instance.GetComponent<HeatmapPipeOverlay>();
            if (overlay)
                overlay.RefreshGeometry();
        }

        public static void PlayerInputAwakePostfix()
        {
            try
            {
                if (HeatmapAction == null)
                {
                    var plugin = global::KingdomBottleneckHeatmap.BottleneckHeatmapPlugin.Instance;
                    string keyName = plugin?.ToggleKeyConfig?.Value.ToString().ToLowerInvariant() ?? "h";
                    HeatmapAction = new InputAction("BottleneckHeatmapToggle", InputActionType.Button, $"<Keyboard>/{keyName}");
                    performedHandler = ctx =>
                    {
                        var pluginInstance = global::KingdomBottleneckHeatmap.BottleneckHeatmapPlugin.Instance;
                        if (pluginInstance != null && pluginInstance.CanHandleHotkey())
                            pluginInstance.ToggleHeatmapFromHotkey();
                    };
                    HeatmapAction.performed += performedHandler;
                    HeatmapAction.Enable();
                }

                FoundationApi.Bindings.AddRebindable(HeatmapAction);
            }
            catch
            {
            }
        }

        public static void ShowFlowPostfix()
        {
            global::KingdomBottleneckHeatmap.BottleneckHeatmapPlugin.Instance?.RefreshAllOverlays();
        }
    }
}
