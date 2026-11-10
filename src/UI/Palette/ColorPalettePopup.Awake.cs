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
        void Awake()
        {
            try
            {
                VanillaTheme.Ensure();
                BuildUI();
            }
            catch (Exception ex)
            {
                BottleneckHeatmapPlugin.Instance?.Log?.LogError($"ColorPalettePopup.Awake error: {ex}");
            }
        }
    }
}
