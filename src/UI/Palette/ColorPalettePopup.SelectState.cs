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
        public void SelectState(BottleneckState state)
        {
            _currentState = state;
            var plugin = BottleneckHeatmapPlugin.Instance;
            Color curColor = Color.green;

            if (plugin != null)
            {
                string hex = state switch
                {
                    BottleneckState.Running => plugin.RunningColorConfig.Value,
                    BottleneckState.Starved => plugin.StarvedColorConfig.Value,
                    BottleneckState.Clogged => plugin.CloggedColorConfig.Value,
                    BottleneckState.FullStorage => plugin.FullStorageColorConfig.Value,
                    BottleneckState.Idle => plugin.IdleColorConfig.Value,
                    _ => "#26FA73"
                };
                ColorUtility.TryParseHtmlString(hex, out curColor);
            }

            Color.RGBToHSV(curColor, out _baseHue, out _baseSaturation, out _baseValue);
            if (_baseSaturation < 0.2f && state != BottleneckState.Idle) _baseSaturation = 0.85f;
            if (_baseValue < 0.4f) _baseValue = 0.95f;

            UpdateTabsVisuals();
            UpdateSurfaces();
            UpdateGeneratedVariations();

            string stateName = GetStateDisplayName(_currentState);
            if (_headerSubtitle) _headerSubtitle.text = $"Select base color for <color=#FFD92E>{stateName}</color> to auto-generate slight variation hues";
            if (_variationsTitle) _variationsTitle.text = $"Auto-Generated Surrounding Hues for {stateName}:";
            if (_applyButton)
            {
                var tmp = _applyButton.GetComponentInChildren<TMP_Text>(true);
                if (tmp) tmp.text = $"Apply to {stateName}";
            }
        }
    }
}
