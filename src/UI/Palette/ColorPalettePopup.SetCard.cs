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
        void SetCard(int idx, string title, Color col, string hex)
        {
            if (idx >= _cards.Length) return;
            _cards[idx].Color = col;
            if (_cards[idx].Swatch) _cards[idx].Swatch.color = col;
            if (_cards[idx].Title) _cards[idx].Title.text = title;
            if (_cards[idx].Hex) _cards[idx].Hex.text = hex;
        }
    }
}
