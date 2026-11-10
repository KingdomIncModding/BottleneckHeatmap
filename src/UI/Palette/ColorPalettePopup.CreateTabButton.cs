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
        Button CreateTabButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Action onClick)
        {
            var btn = CreateThemedButton(parent, name, label, pos, size, onClick);
            var tmp = btn.GetComponentInChildren<TMP_Text>(true);
            if (tmp)
            {
                tmp.fontSizeMin = 10;
                tmp.fontSizeMax = 13;
            }
            return btn;
        }
    }
}
