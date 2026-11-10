using System;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    internal static partial class HeatmapMaterials
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
    }
}
