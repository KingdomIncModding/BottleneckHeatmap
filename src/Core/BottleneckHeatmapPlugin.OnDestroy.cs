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
    }
}
