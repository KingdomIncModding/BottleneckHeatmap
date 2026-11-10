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
        void CreateSeparator(Transform parent, float y)
        {
            var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(parent, false);
            var srt = (RectTransform)sep.transform;
            srt.anchorMin = new Vector2(0f, 1f);
            srt.anchorMax = new Vector2(1f, 1f);
            srt.anchoredPosition = new Vector2(0f, y);
            srt.sizeDelta = new Vector2(-28f, 1.5f);
            sep.GetComponent<Image>().color = new Color(0.38f, 0.32f, 0.24f, 0.85f);
        }
    }
}
