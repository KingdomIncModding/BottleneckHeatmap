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
