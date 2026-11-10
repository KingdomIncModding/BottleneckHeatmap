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
        void SetTabActive(Button tab, bool active)
        {
            if (!tab) return;
            var img = tab.GetComponent<Image>();
            if (img)
            {
                img.color = active ? new Color(1f, 0.95f, 0.85f, 1f) : new Color(0.70f, 0.68f, 0.65f, 0.9f);
            }
            var text = tab.GetComponentInChildren<TMP_Text>(true);
            if (text)
            {
                text.color = active ? new Color(1f, 0.85f, 0.18f, 1f) : new Color(0.78f, 0.74f, 0.68f, 1f);
                text.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            }
        }
    }
}
