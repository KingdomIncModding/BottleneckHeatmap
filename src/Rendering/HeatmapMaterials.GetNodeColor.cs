using System;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    internal static partial class HeatmapMaterials
    {
        public static Color GetNodeColor(BottleneckState state, float opacity)
        {
            Color effectiveRunning = EnableRunning ? RunningColor : Color.clear;
            Color baseCol;
            float alphaMult;

            switch (state)
            {
                case BottleneckState.Running:
                    baseCol = effectiveRunning;
                    alphaMult = 1.0f;
                    break;
                case BottleneckState.Starved:
                    baseCol = EnableStarved ? StarvedColor : effectiveRunning;
                    alphaMult = 1.0f;
                    break;
                case BottleneckState.Clogged:
                    baseCol = EnableClogged ? CloggedColor : effectiveRunning;
                    alphaMult = EnableClogged ? 1.15f : 1.0f;
                    break;
                case BottleneckState.FullStorage:
                    baseCol = EnableFullStorage ? FullStorageColor : effectiveRunning;
                    alphaMult = EnableFullStorage ? 1.10f : 1.0f;
                    break;
                case BottleneckState.Idle:
                    baseCol = EnableIdle ? IdleColor : effectiveRunning;
                    alphaMult = EnableIdle ? 0.40f : 1.0f;
                    break;
                default:
                    baseCol = Color.clear;
                    alphaMult = 0f;
                    break;
            }

            if (baseCol == Color.clear || baseCol.a <= 0f) return Color.clear;
            return new Color(baseCol.r, baseCol.g, baseCol.b, Mathf.Clamp01(opacity * alphaMult));
        }
    }
}
