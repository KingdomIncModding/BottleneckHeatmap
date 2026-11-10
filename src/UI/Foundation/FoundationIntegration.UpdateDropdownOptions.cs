using System;
using System.Collections.Generic;
using Assets.Behaviour.UI;
using Assets.Source.Util;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal static partial class FoundationIntegration
    {
        public static void UpdateDropdownOptions(BottleneckState state, (string Label, string Hex)[] newVariations)
        {
            try
            {
                var plugin = BottleneckHeatmapPlugin.Instance;
                string currentHex = state switch
                {
                    BottleneckState.Running => plugin?.RunningColorConfig?.Value,
                    BottleneckState.Starved => plugin?.StarvedColorConfig?.Value,
                    BottleneckState.Clogged => plugin?.CloggedColorConfig?.Value,
                    BottleneckState.FullStorage => plugin?.FullStorageColorConfig?.Value,
                    BottleneckState.Idle => plugin?.IdleColorConfig?.Value,
                    _ => null
                };

                ModConfigStateColorRow row = state switch
                {
                    BottleneckState.Running => _runningRow,
                    BottleneckState.Starved => _starvedRow,
                    BottleneckState.Clogged => _cloggedRow,
                    BottleneckState.FullStorage => _fullStorageRow,
                    BottleneckState.Idle => _idleRow,
                    _ => null
                };

                row?.UpdateVariations(newVariations, currentHex);
            }
            catch
            {
            }
        }
    }
}
