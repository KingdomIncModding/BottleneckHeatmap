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
        public void UpdateVariations((string Label, string Hex)[] newVariations, string currentHex)
        {
            _variations = newVariations;
            if (ColorUtility.TryParseHtmlString(currentHex, out var col) && _leftSwatchImage != null)
            {
                _leftSwatchImage.color = col;
            }

            if (newVariations != null)
            {
                for (int i = 0; i < 5 && i < newVariations.Length; i++)
                {
                    if (_swatchFills[i] != null && ColorUtility.TryParseHtmlString(newVariations[i].Hex, out var vCol))
                    {
                        _swatchFills[i].color = vCol;
                    }
                }
            }

            HighlightSelected(currentHex);
        }
    }
}
