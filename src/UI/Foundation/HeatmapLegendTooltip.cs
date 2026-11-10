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
    /// <summary>
    /// Rich interactive hover legend tooltip displayed when hovering over the Heatmap button on the Mod Bar.
    /// </summary>
    internal sealed partial class HeatmapLegendTooltip : MonoBehaviour
    {
        static HeatmapLegendTooltip _instance;

        Image _swatchRunning;
        Image _swatchStarved;
        Image _swatchClogged;
        Image _swatchFullStorage;
        Image _swatchIdle;


        public static void Show(RectTransform anchor)
        {
            if (!anchor) return;
            EnsureInstance(anchor);
            if (!_instance) return;

            _instance.UpdateColors();
            _instance.PositionAt(anchor);
            _instance.gameObject.SetActive(true);
            _instance.transform.SetAsLastSibling();
        }
    }
}
