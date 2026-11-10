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
    internal sealed partial class HeatmapBarView : IModBarView
    {
        public void Refresh()
        {
            if (_text != null)
            {
                _text.text = GetLabel();
                _text.color = _plugin.IsHeatmapEnabled
                    ? new Color(0.95f, 1f, 0.95f)
                    : new Color(0.72f, 0.72f, 0.75f);
            }
            if (_button != null && _image != null)
            {
                var colors = _button.colors;
                Color baseTint = _plugin.IsHeatmapEnabled
                    ? new Color(0.85f, 1f, 0.88f, 1f)
                    : new Color(0.75f, 0.75f, 0.78f, 0.9f);
                colors.normalColor = baseTint;
                colors.highlightedColor = _plugin.IsHeatmapEnabled
                    ? new Color(0.92f, 1f, 0.95f, 1f)
                    : new Color(0.85f, 0.85f, 0.88f, 1f);
                colors.selectedColor = baseTint;
                _button.colors = colors;
                _image.color = baseTint;
            }
        }
    }
}
