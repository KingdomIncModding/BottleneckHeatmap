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
    public sealed partial class PickerSurface : MaskableGraphic, IPointerDownHandler, IDragHandler
    {
        Color At(float x, float y)
        {
            if (Mode == PickerMode.SaturationValue) return Color.HSVToRGB(Hue, x, y);
            return Color.HSVToRGB(1 - y, 1, 1);
        }
    }
}
