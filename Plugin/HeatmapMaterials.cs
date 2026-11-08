using System;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    internal static class HeatmapMaterials
    {
        static Material _lineMaterial;

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

        public static Color GetNodeColor(BottleneckState state, float opacity)
        {
            return state switch
            {
                BottleneckState.Running => new Color(0.15f, 0.98f, 0.45f, Mathf.Clamp01(opacity)),
                BottleneckState.Starved => new Color(1.00f, 0.85f, 0.18f, Mathf.Clamp01(opacity)),
                BottleneckState.Clogged => new Color(1.00f, 0.22f, 0.26f, Mathf.Clamp01(opacity * 1.15f)),
                BottleneckState.Idle => new Color(0.55f, 0.62f, 0.75f, Mathf.Clamp01(opacity * 0.40f)),
                _ => Color.clear
            };
        }

        public static Color GetPipeColor(BottleneckState state, float opacity)
        {
            return state switch
            {
                BottleneckState.Running => new Color(0.18f, 1.00f, 0.50f, Mathf.Clamp01(opacity)),
                BottleneckState.Starved => new Color(1.00f, 0.88f, 0.20f, Mathf.Clamp01(opacity)),
                BottleneckState.Clogged => new Color(1.00f, 0.25f, 0.28f, Mathf.Clamp01(opacity * 1.15f)),
                BottleneckState.Idle => new Color(0.55f, 0.62f, 0.75f, Mathf.Clamp01(opacity * 0.40f)),
                _ => Color.clear
            };
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
