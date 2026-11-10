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
    }
}
