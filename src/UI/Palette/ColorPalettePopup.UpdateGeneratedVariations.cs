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
        void UpdateGeneratedVariations()
        {
            Color baseColor = Color.HSVToRGB(_baseHue, Mathf.Clamp01(_baseSaturation), Mathf.Clamp01(_baseValue));
            if (_baseColorPreview) _baseColorPreview.color = baseColor;
            if (_baseHexText) _baseHexText.text = "#" + ColorUtility.ToHtmlStringRGB(baseColor);

            string stateName = GetStateDisplayName(_currentState);
            _currentGeneratedVariations = FoundationIntegration.GenerateVariations(baseColor, stateName);

            string[] cardTitles = { "Selected Base", "Warm Tone", "Cool Tone", "Bright Tone", "Deep Tone" };
            for (int i = 0; i < _cards.Length && i < _currentGeneratedVariations.Length; i++)
            {
                ColorUtility.TryParseHtmlString(_currentGeneratedVariations[i].Hex, out var col);
                SetCard(i, cardTitles[i], col, _currentGeneratedVariations[i].Hex);
            }
        }
    }
}
