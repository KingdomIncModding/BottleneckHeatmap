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
            public static void KeyBindingRowSetBindingPostfix(Assets.Behaviour.UI.MainMenu.KeyBindingRow __instance, InputAction binding)
            {
                if (binding != null && binding.name == "BottleneckHeatmapToggle")
                {
                    var label = AccessTools.Field(typeof(Assets.Behaviour.UI.MainMenu.KeyBindingRow), "_label")?.GetValue(__instance) as TMPro.TMP_Text;
                    if (label != null)
                    {
                        label.text = "Bottleneck Heatmap";
                    }
                }
            }
        }
    }
}
