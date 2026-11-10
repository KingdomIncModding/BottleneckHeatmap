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
        public void SyncVanillaActionBinding()
        {
            try
            {
                if (Patches.HeatmapAction != null)
                {
                    string path = $"<Keyboard>/{ToggleKeyConfig.Value.ToString().ToLowerInvariant()}";
                    Patches.HeatmapAction.ApplyBindingOverride(0, path);
                }
            }
            catch { }
        }
    }
}
