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
        // 5 Variation Cards
        partial struct VariationCard
        {
            public GameObject Root;
            public Image Border;
            public Image Swatch;
            public TMP_Text Title;
            public TMP_Text Hex;
            public Color Color;
        }
    }
}
