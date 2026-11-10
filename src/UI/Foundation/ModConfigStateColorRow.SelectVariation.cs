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
    internal sealed partial class ModConfigStateColorRow : IModConfigElement
    {
        void SelectVariation(int index)
        {
            if (_variations == null || index < 0 || index >= _variations.Length) return;
            string hex = _variations[index].Hex;
            HexSetter?.Invoke(hex);
            if (ColorUtility.TryParseHtmlString(hex, out var col) && _leftSwatchImage != null)
            {
                _leftSwatchImage.color = col;
            }
            HighlightSelected(hex);
        }
    }
}
