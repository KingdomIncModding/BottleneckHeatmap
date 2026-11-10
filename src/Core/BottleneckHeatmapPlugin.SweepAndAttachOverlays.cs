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
    }
}
