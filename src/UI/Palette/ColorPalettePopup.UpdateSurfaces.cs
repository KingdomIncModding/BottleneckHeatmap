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
        void UpdateSurfaces()
        {
            if (_svSurface)
            {
                _svSurface.Hue = _baseHue;
                _svSurface.Saturation = _baseSaturation;
                _svSurface.Brightness = _baseValue;
                _svSurface.SetVerticesDirty();
            }

            if (_hueSurface)
            {
                _hueSurface.Hue = _baseHue;
                _hueSurface.SetVerticesDirty();
            }
        }
    }
}
