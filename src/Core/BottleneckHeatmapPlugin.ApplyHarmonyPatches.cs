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

            // Hook vanilla Key Bindings
            var playerInputAwake = AccessTools.Method(typeof(Assets.Behaviour.Input.PlayerInput), "Awake");
            if (playerInputAwake != null)
            {
                _harmony.Patch(playerInputAwake, postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.PlayerInputAwakePostfix)));
            }

            var keyBindingRowSetBinding = AccessTools.Method(typeof(Assets.Behaviour.UI.MainMenu.KeyBindingRow), nameof(Assets.Behaviour.UI.MainMenu.KeyBindingRow.SetBinding));
            if (keyBindingRowSetBinding != null)
            {
                _harmony.Patch(keyBindingRowSetBinding, postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.KeyBindingRowSetBindingPostfix)));
            }
        }
    }
}
