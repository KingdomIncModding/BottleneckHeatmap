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
    internal static partial class FoundationIntegration
    {
        public static (string Label, string Hex)[] GenerateVariations(Color baseColor, string stateName)
        {
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            string baseHex = "#" + ColorUtility.ToHtmlStringRGB(baseColor);

            // 1. Base / Selected
            string label0 = $"{stateName} Selected ({baseHex})";

            // 2. Warm Variation (-12 deg)
            float hWarm = (h - (12f / 360f) + 1f) % 1f;
            Color cWarm = Color.HSVToRGB(hWarm, Mathf.Clamp01(s * 0.96f), Mathf.Min(1f, v * 1.02f));
            string hexWarm = "#" + ColorUtility.ToHtmlStringRGB(cWarm);
            string label1 = $"Warm Variation ({hexWarm})";

            // 3. Cool Variation (+12 deg)
            float hCool = (h + (12f / 360f)) % 1f;
            Color cCool = Color.HSVToRGB(hCool, Mathf.Clamp01(s * 1.02f), v);
            string hexCool = "#" + ColorUtility.ToHtmlStringRGB(cCool);
            string label2 = $"Cool Variation ({hexCool})";

            // 4. Bright Variation (-24 deg or lighter)
            float hBright = (h - (24f / 360f) + 1f) % 1f;
            Color cBright = (s < 0.25f)
                ? Color.HSVToRGB(h, Mathf.Clamp01(s * 0.70f), Mathf.Min(1f, v * 1.25f))
                : Color.HSVToRGB(hBright, Mathf.Clamp01(s * 0.90f), 1f);
            string hexBright = "#" + ColorUtility.ToHtmlStringRGB(cBright);
            string label3 = $"Bright Variation ({hexBright})";

            // 5. Deep Variation (+24 deg or deeper)
            float hDeep = (h + (24f / 360f)) % 1f;
            Color cDeep = (s < 0.25f)
                ? Color.HSVToRGB(h, Mathf.Min(1f, s * 1.30f), Mathf.Clamp01(v * 0.70f))
                : Color.HSVToRGB(hDeep, 1f, Mathf.Clamp01(v * 0.90f));
            string hexDeep = "#" + ColorUtility.ToHtmlStringRGB(cDeep);
            string label4 = $"Deep Variation ({hexDeep})";

            return new (string Label, string Hex)[]
            {
                (label0, baseHex),
                (label1, hexWarm),
                (label2, hexCool),
                (label3, hexBright),
                (label4, hexDeep)
            };
        }
    }
}
