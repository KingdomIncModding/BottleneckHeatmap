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
    internal sealed partial class HeatmapLegendTooltip : MonoBehaviour
    {
        static void EnsureInstance(RectTransform anchor)
        {
            if (_instance) return;
            var canvas = anchor.GetComponentInParent<Canvas>();
            if (!canvas) canvas = UnityEngine.Object.FindAnyObjectByType<Canvas>();
            if (!canvas) return;

            var go = new GameObject("HeatmapLegendTooltip", typeof(RectTransform));
            go.transform.SetParent(canvas.transform, false);
            _instance = go.AddComponent<HeatmapLegendTooltip>();
            _instance.BuildUI();
        }
    }
}
