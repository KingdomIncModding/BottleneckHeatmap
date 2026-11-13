using KingdomFoundation.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KingdomBottleneckHeatmap
{
    internal struct VariationCard
    {
        public GameObject Root;
        public Image Border;
        public Image Swatch;
        public TMP_Text Title;
        public TMP_Text Hex;
        public Color Color;
    }
}
