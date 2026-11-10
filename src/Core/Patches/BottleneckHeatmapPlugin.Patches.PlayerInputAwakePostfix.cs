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
        static partial class Patches
        {
            public static void PlayerInputAwakePostfix()
            {
                try
                {
                    if (HeatmapAction == null)
                    {
                        var plugin = Instance;
                        string keyName = plugin?.ToggleKeyConfig?.Value.ToString().ToLowerInvariant() ?? "h";
                        HeatmapAction = new InputAction("BottleneckHeatmapToggle", InputActionType.Button, $"<Keyboard>/{keyName}");
                        HeatmapAction.performed += ctx =>
                        {
                            if (Instance != null && !Instance.IsListeningForHotkey)
                            {
                                Instance.ToggleHeatmapFromHotkey();
                            }
                        };
                        HeatmapAction.Enable();
                    }

                    var list = Assets.Behaviour.Input.PlayerInput.RebindableKeys as System.Collections.Generic.List<InputAction>;
                    if (list != null && !list.Contains(HeatmapAction))
                    {
                        list.Add(HeatmapAction);
                    }
                }
                catch { }
            }
        }
    }
}
