using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Behaviour.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    public sealed partial class ColorPalettePopup : MonoBehaviour
    {
        void ResetStateDefault()
        {
            var plugin = BottleneckHeatmapPlugin.Instance;
            if (plugin != null)
            {
                string defHex = _currentState switch
                {
                    BottleneckState.Running => "#26FA73",
                    BottleneckState.Starved => "#FFD92E",
                    BottleneckState.Clogged => "#FF3842",
                    BottleneckState.FullStorage => "#B042FF",
                    BottleneckState.Idle => "#8C9EB8",
                    _ => "#26FA73"
                };

                ColorUtility.TryParseHtmlString(defHex, out var defColor);
                Color.RGBToHSV(defColor, out _baseHue, out _baseSaturation, out _baseValue);
                UpdateSurfaces();
                UpdateGeneratedVariations();
                ApplyCurrentColor();
            }
        }
    }
}
