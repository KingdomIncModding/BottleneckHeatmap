using System;
using System.Collections.Generic;
using KingdomFoundation.Api;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal sealed class HeatmapBarView : IModBarView
    {
        readonly BottleneckHeatmapPlugin _plugin;
        readonly Button _button;
        readonly TMP_Text _text;
        readonly Image _image;
        public HeatmapBarView(BottleneckHeatmapPlugin plugin, IModBarContext context)
        {
            _plugin = plugin;
            _button = context.CreateButton(GetLabel(), OnClick);
            var clickHandler = _button.gameObject.AddComponent<HeatmapBarClickHandler>();
            clickHandler.OnRightClick = () => ColorPalettePopup.Open();
            clickHandler.OnHoverEnter = () => HeatmapLegendTooltip.Show(_button.transform as RectTransform);
            clickHandler.OnHoverExit = () => HeatmapLegendTooltip.Hide();
            _image = _button.GetComponent<Image>();
            _text = _button.GetComponentInChildren<TMP_Text>(true);
            if (_text != null)
            {
                _text.enableAutoSizing = true;
                _text.fontSizeMin = 13;
                _text.fontSizeMax = 18;
                _text.overflowMode = TextOverflowModes.Overflow;
                _text.rectTransform.offsetMin = new Vector2(4, 2);
                _text.rectTransform.offsetMax = new Vector2(-4, -2);
            }

            Refresh();
        }

        public void Dispose()
        {
            HeatmapLegendTooltip.Hide();
            FoundationIntegration.ClearActiveView(this);
        }

        string GetLabel() => _plugin.IsHeatmapEnabled ? "Heatmap: ON" : "Heatmap: OFF";
        void OnClick()
        {
            _plugin.SetHeatmapEnabled(!_plugin.IsHeatmapEnabled, fromUser: true);
        }

        public void Refresh()
        {
            if (_text != null)
            {
                _text.text = GetLabel();
                _text.color = _plugin.IsHeatmapEnabled ? new Color(0.95f, 1f, 0.95f) : new Color(0.72f, 0.72f, 0.75f);
            }

            if (_button != null && _image != null)
            {
                var colors = _button.colors;
                Color baseTint = _plugin.IsHeatmapEnabled ? new Color(0.85f, 1f, 0.88f, 1f) : new Color(0.75f, 0.75f, 0.78f, 0.9f);
                colors.normalColor = baseTint;
                colors.highlightedColor = _plugin.IsHeatmapEnabled ? new Color(0.92f, 1f, 0.95f, 1f) : new Color(0.85f, 0.85f, 0.88f, 1f);
                colors.selectedColor = baseTint;
                _button.colors = colors;
                _image.color = baseTint;
            }
        }
    }
}
