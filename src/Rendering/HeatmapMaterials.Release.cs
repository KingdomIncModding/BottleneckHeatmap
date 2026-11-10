using System;
using UnityEngine;

namespace KingdomBottleneckHeatmap
{
    internal static partial class HeatmapMaterials
    {
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
