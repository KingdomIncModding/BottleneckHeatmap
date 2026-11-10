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
        void HighlightSelected(string currentHex)
        {
            for (int i = 0; i < 5; i++)
            {
                if (_swatchBorders[i] == null) continue;
                bool isSelected = _variations != null && i < _variations.Length &&
                    string.Equals(_variations[i].Hex, currentHex, StringComparison.OrdinalIgnoreCase);

                if (isSelected)
                {
                    _swatchBorders[i].color = new Color(1f, 0.85f, 0.18f, 1f); // Bright gold border!
                }
                else
                {
                    _swatchBorders[i].color = new Color(0.35f, 0.30f, 0.24f, 0.9f); // Dark bronze border
                }
            }
        }
    }
}
