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
        static ColorPalettePopup _instance;

        BottleneckState _currentState = BottleneckState.Running;

        // Current base color in HSV
        float _baseHue = 0.38f;
        float _baseSaturation = 0.85f;
        float _baseValue = 0.98f;

        // UI references
        PickerSurface _svSurface;
        PickerSurface _hueSurface;
        Image _baseColorPreview;
        TMP_Text _baseHexText;
        TMP_Text _headerSubtitle;
        TMP_Text _variationsTitle;
        Button _applyButton;

        // Tab buttons
        Button _tabRunning, _tabStarved, _tabClogged, _tabFullStorage, _tabIdle;

        readonly VariationCard[] _cards = new VariationCard[5];
        (string Label, string Hex)[] _currentGeneratedVariations;

        static readonly string[] Presets = new string[]
        {
            "26FA73", "00FFB2", "00D9FF", "2E75FF", "8C28FF",
            "B042FF", "DF2BFF", "FF2B85", "FF3842", "FFD92E"
        };


        public static void Open(BottleneckState targetState = BottleneckState.Running)
        {
            try
            {
                EnsureInstance();
                if (!_instance)
                {
                    BottleneckHeatmapPlugin.Instance?.Log?.LogError("ColorPalettePopup.Open: instance failed to create");
                    return;
                }

                _instance.gameObject.SetActive(true);
                _instance.transform.SetAsLastSibling();
                _instance.SelectState(targetState);
            }
            catch (Exception ex)
            {
                BottleneckHeatmapPlugin.Instance?.Log?.LogError($"ColorPalettePopup.Open error: {ex}");
            }
        }
    }
}
