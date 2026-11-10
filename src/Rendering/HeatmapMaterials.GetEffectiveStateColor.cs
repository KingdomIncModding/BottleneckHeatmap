using System;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    internal static partial class HeatmapMaterials
    {
        public static Color GetEffectiveStateColor(BottleneckState state)
        {
            Color effectiveRunning = EnableRunning ? RunningColor : Color.clear;

            return state switch
            {
                BottleneckState.Running => effectiveRunning,
                BottleneckState.Starved => EnableStarved ? StarvedColor : effectiveRunning,
                BottleneckState.Clogged => EnableClogged ? CloggedColor : effectiveRunning,
                BottleneckState.FullStorage => EnableFullStorage ? FullStorageColor : effectiveRunning,
                BottleneckState.Idle => EnableIdle ? IdleColor : effectiveRunning,
                _ => Color.clear
            };
        }
    }
}
