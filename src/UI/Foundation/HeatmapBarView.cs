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
    }
}
