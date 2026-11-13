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
                    HeatmapAction.performed += ctx =>
                    {
                        if (global::KingdomBottleneckHeatmap.BottleneckHeatmapPlugin.Instance != null && !global::KingdomBottleneckHeatmap.BottleneckHeatmapPlugin.Instance.IsListeningForHotkey)
                        {
                            global::KingdomBottleneckHeatmap.BottleneckHeatmapPlugin.Instance.ToggleHeatmapFromHotkey();
                        }
                    };
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
