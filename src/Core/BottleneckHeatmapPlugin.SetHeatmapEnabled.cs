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
        public void SetHeatmapEnabled(bool enabled, bool fromUser = false)
        {
            if (EnabledConfig.Value != enabled)
            {
                EnabledConfig.Value = enabled;
                Config.Save();
            }

            FoundationIntegration.SyncModBarToggle(enabled);

            if (fromUser && ShowToastConfig.Value)
            {
                try
                {
                    string runHex = RunningColorConfig?.Value ?? "#26FA73";
                    string starvHex = (EnableStarvedState ? StarvedColorConfig?.Value : runHex) ?? "#FFD92E";
                    string clogHex = (EnableCloggedState ? CloggedColorConfig?.Value : runHex) ?? "#FF3842";
                    string fullHex = (EnableFullStorageState ? FullStorageColorConfig?.Value : runHex) ?? "#B042FF";

                    var msgObj = UIStatusMessage.Show(
                        enabled ? "Bottleneck Heatmap: Active" : "Bottleneck Heatmap: Disabled",
                        "Icons_17",
                        false
                    );

                    if (enabled && msgObj != null && msgObj.StatusText != null)
                    {
                        msgObj.StatusText.text = $"Bottleneck Heatmap: Active (<color={runHex}>Running</color>  <color={starvHex}>Starved</color>  <color={clogHex}>Clogged</color>  <color={fullHex}>Full</color>)";
                    }
                }
                catch
                {
                }
            }

            RefreshAllOverlays();
        }
    }
}
