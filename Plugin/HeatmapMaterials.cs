using System;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    internal static class HeatmapMaterials
    {
        static Material _lineMaterial;

        public static Color RunningColor = new Color(0.15f, 0.98f, 0.45f, 1f);
        public static Color StarvedColor = new Color(1.00f, 0.85f, 0.18f, 1f);
        public static Color CloggedColor = new Color(1.00f, 0.22f, 0.26f, 1f);
        public static Color FullStorageColor = new Color(0.69f, 0.26f, 1.00f, 1f); // Default vibrant royal purple
        public static Color IdleColor = new Color(0.55f, 0.62f, 0.75f, 1f);

        public static bool EnableRunning = true;
        public static bool EnableStarved = true;
        public static bool EnableClogged = true;
        public static bool EnableFullStorage = true;
        public static bool EnableIdle = true;

        public static Material LineMaterial
        {
            get
            {
                if (!_lineMaterial)
                {
                    var shader = Shader.Find("Sprites/Default");
                    if (shader)
                    {
                        _lineMaterial = new Material(shader)
                        {
                            name = "BottleneckHeatmap.LineMaterial"
                        };
                    }
                }
                return _lineMaterial;
            }
        }

        public static void SetColors(Color running, Color starved, Color clogged, Color fullStorage, Color idle)
        {
            RunningColor = running;
            StarvedColor = starved;
            CloggedColor = clogged;
            FullStorageColor = fullStorage;
            IdleColor = idle;
        }

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

        public static Color GetPipeColor(BottleneckState state, float opacity)
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

        public static void Release()
        {
            if (_lineMaterial)
            {
                UnityEngine.Object.Destroy(_lineMaterial);
                _lineMaterial = null;
            }
        }
    }
}
