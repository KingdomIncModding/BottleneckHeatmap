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
        Image CreateLegendRow(Transform parent, string name, string label, string desc, float y)
        {
            var row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            var rRt = (RectTransform)row.transform;
            rRt.anchorMin = new Vector2(0f, 1f);
            rRt.anchorMax = new Vector2(1f, 1f);
            rRt.pivot = new Vector2(0f, 1f);
            rRt.anchoredPosition = new Vector2(16f, y);
            rRt.sizeDelta = new Vector2(-32f, 24f);

            // Swatch Border
            var sBorder = new GameObject("SwatchBorder", typeof(RectTransform), typeof(Image));
            sBorder.transform.SetParent(row.transform, false);
            var sbRt = (RectTransform)sBorder.transform;
            sbRt.anchorMin = new Vector2(0f, 0.5f);
            sbRt.anchorMax = new Vector2(0f, 0.5f);
            sbRt.pivot = new Vector2(0f, 0.5f);
            sbRt.anchoredPosition = Vector2.zero;
            sbRt.sizeDelta = new Vector2(18f, 18f);
            sBorder.GetComponent<Image>().color = new Color(0.35f, 0.30f, 0.24f, 1f);

            // Swatch Fill
            var sFill = new GameObject("SwatchFill", typeof(RectTransform), typeof(Image));
            sFill.transform.SetParent(sBorder.transform, false);
            var sfRt = (RectTransform)sFill.transform;
            sfRt.anchorMin = Vector2.zero;
            sfRt.anchorMax = Vector2.one;
            sfRt.offsetMin = new Vector2(2f, 2f);
            sfRt.offsetMax = new Vector2(-2f, -2f);
            var swatchImg = sFill.GetComponent<Image>();

            // Label
            var lObj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            lObj.transform.SetParent(row.transform, false);
            var lRt = (RectTransform)lObj.transform;
            lRt.anchorMin = new Vector2(0f, 0f);
            lRt.anchorMax = new Vector2(0f, 1f);
            lRt.pivot = new Vector2(0f, 0.5f);
            lRt.anchoredPosition = new Vector2(24f, 0f);
            lRt.sizeDelta = new Vector2(85f, 24f);
            var lText = lObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null) { lText.font = VanillaTheme.Font; lText.fontSharedMaterial = VanillaTheme.FontMaterial; }
            lText.text = label;
            lText.fontSize = 12f;
            lText.fontStyle = FontStyles.Bold;
            lText.color = Color.white;
            lText.alignment = TextAlignmentOptions.MidlineLeft;

            // Description
            var dObj = new GameObject("Desc", typeof(RectTransform), typeof(TextMeshProUGUI));
            dObj.transform.SetParent(row.transform, false);
            var dRt = (RectTransform)dObj.transform;
            dRt.anchorMin = new Vector2(0f, 0f);
            dRt.anchorMax = new Vector2(1f, 1f);
            dRt.pivot = new Vector2(0f, 0.5f);
            dRt.offsetMin = new Vector2(112f, 0f);
            dRt.offsetMax = Vector2.zero;
            var dText = dObj.GetComponent<TextMeshProUGUI>();
            if (VanillaTheme.Font != null) { dText.font = VanillaTheme.Font; dText.fontSharedMaterial = VanillaTheme.FontMaterial; }
            dText.text = desc;
            dText.fontSize = 10f;
            dText.color = new Color(0.80f, 0.77f, 0.72f, 0.95f);
            dText.alignment = TextAlignmentOptions.MidlineLeft;

            return swatchImg;
        }
    }
}
